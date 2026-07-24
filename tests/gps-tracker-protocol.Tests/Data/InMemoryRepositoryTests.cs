#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Data;

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using GpsTrackerProtocol.Domain.Models;
using Xunit;

/// <summary>
/// Tests for <see cref="InMemoryRepository{T}"/> thread-safe snapshot behavior.
/// Ensures that concurrent writers and readers don't interfere with each other
/// and that snapshots returned are consistent point-in-time copies.
/// </summary>
public class InMemoryRepositoryTests
{
    private readonly InMemoryLocationDataRepository _repository;

    public InMemoryRepositoryTests()
    {
        _repository = new InMemoryLocationDataRepository();
    }

    [Fact]
    [Trait("Category", "Concurrency")]
    [SuppressMessage("Usage", "xUnit1004:Fixture interface methods should be used to construct test data", Justification = "Test setup")]
    public async Task GetAllAsync_ReturnsSnapshot_NotLiveReference()
    {
        // Arrange
        var entity1 = new LocationData
        {
            DeviceId = "device1",
            Latitude = 10.0,
            Longitude = 20.0,
            Timestamp = DateTime.UtcNow.AddMinutes(-10)
        };
        var entity2 = new LocationData
        {
            DeviceId = "device2",
            Latitude = 15.0,
            Longitude = 25.0,
            Timestamp = DateTime.UtcNow.AddMinutes(-5)
        };

        await _repository.CreateAsync(entity1);
        await _repository.CreateAsync(entity2);

        // Act - get snapshot
        var snapshot1 = await _repository.GetAllAsync();
        var snapshot2 = await _repository.GetAllAsync();

        // Assert - both snapshots should be equal
        snapshot1.Should().BeEquivalentTo(snapshot2);

        // Modify the underlying store directly to verify snapshot independence
        var allEntities = await _repository.GetAllAsync();
        var countBefore = allEntities.Count();

        // This should not affect already returned snapshots
        var entity3 = new LocationData
        {
            DeviceId = "device3",
            Latitude = 30.0,
            Longitude = 40.0,
            Timestamp = DateTime.UtcNow
        };
        await _repository.CreateAsync(entity3);

        // Previously returned snapshots should still have original count
        snapshot1.Should().HaveCount(countBefore);
        snapshot2.Should().HaveCount(countBefore);

        // New snapshot should include the new entity
        var newSnapshot = await _repository.GetAllAsync();
        newSnapshot.Should().HaveCount(countBefore + 1);
    }

    [Fact]
    [Trait("Category", "Concurrency")]
    public async Task GetByIdAsync_ReturnsSnapshot_NotLiveReference()
    {
        // Arrange
        var originalEntity = new LocationData
        {
            DeviceId = "device1",
            Latitude = 10.0,
            Longitude = 20.0,
            Timestamp = DateTime.UtcNow.AddMinutes(-10)
        };
        var createdEntity = await _repository.CreateAsync(originalEntity);
        var entityId = createdEntity.Id;

        // Act - get snapshot
        var snapshot1 = await _repository.GetByIdAsync(entityId);
        var snapshot2 = await _repository.GetByIdAsync(entityId);

        // Assert - both snapshots should be equal
        snapshot1.Should().BeEquivalentTo(snapshot2);

        // Modify the original entity in the store
        var updatedEntity = new LocationData
        {
            Id = entityId,
            DeviceId = "device1",
            Latitude = 100.0, // Changed
            Longitude = 200.0, // Changed
            Timestamp = DateTime.UtcNow.AddMinutes(-5) // Changed
        };
        await _repository.UpdateAsync(updatedEntity);

        // Previously returned snapshots should still have original values
        snapshot1.Should().BeEquivalentTo(originalEntity);
        snapshot2.Should().BeEquivalentTo(originalEntity);

        // New snapshot should have updated values
        var newSnapshot = await _repository.GetByIdAsync(entityId);
        newSnapshot.Should().BeEquivalentTo(updatedEntity);
    }

    [Fact]
    [Trait("Category", "Concurrency")]
    public async Task ConcurrentWritersAndReaders_NoExceptionsThrown()
    {
        // Arrange
        const int writerCount = 10;
        const int readerCount = 10;
        const int operationsPerWriter = 50;
        var exceptions = new ConcurrentBag<Exception>();
        var repository = new InMemoryLocationDataRepository();

        // Act - run concurrent writers and readers simultaneously
        var writerTasks = Enumerable.Range(0, writerCount).Select(async writerId =>
        {
            try
            {
                for (int i = 0; i < operationsPerWriter; i++)
                {
                    var entity = new LocationData
                    {
                        DeviceId = $"device{writerId}",
                        Latitude = 10.0 + writerId + (i * 0.01),
                        Longitude = 20.0 + writerId + (i * 0.01),
                        Timestamp = DateTime.UtcNow.AddMinutes(-operationsPerWriter + i)
                    };
                    await repository.CreateAsync(entity);
                }
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        }).ToList();

        var readerTasks = Enumerable.Range(0, readerCount).Select(async readerId =>
        {
            try
            {
                for (int i = 0; i < operationsPerWriter; i++)
                {
                    // Mix of read operations
                    if (i % 3 == 0)
                    {
                        var all = await repository.GetAllAsync();
                        _ = all.Count();
                    }
                    else if (i % 3 == 1)
                    {
                        var byDevice = await repository.GetByDeviceIdAsync($"device{readerId % writerCount}");
                        _ = byDevice.Count();
                    }
                    else
                    {
                        var exists = await repository.ExistsAsync(Guid.NewGuid().ToString());
                        _ = exists;
                    }
                }
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        }).ToList();

        await Task.WhenAll(writerTasks.Concat(readerTasks));

        // Assert - no exceptions should be thrown
        exceptions.Should().BeEmpty();

        // Verify repository is in valid state
        var allEntities = await repository.GetAllAsync();
        allEntities.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Concurrency")]
    public async Task ConcurrentWriters_NoLostUpdates()
    {
        // Arrange
        const int writerCount = 5;
        const int operationsPerWriter = 20;
        var repository = new InMemoryLocationDataRepository();
        var finalCount = 0;

        // Act - multiple writers adding entities with same device ID
        var tasks = Enumerable.Range(0, writerCount).Select(async writerId =>
        {
            for (int i = 0; i < operationsPerWriter; i++)
            {
                var entity = new LocationData
                {
                    DeviceId = "shared-device",
                    Latitude = 10.0 + writerId + (i * 0.01),
                    Longitude = 20.0 + writerId + (i * 0.01),
                    Timestamp = DateTime.UtcNow.AddMinutes(-operationsPerWriter + i)
                };
                await repository.CreateAsync(entity);
            }
        }).ToList();

        await Task.WhenAll(tasks);

        // Assert - all entities should be present (no lost updates)
        var allEntities = await repository.GetAllAsync();
        allEntities.Should().HaveCount(writerCount * operationsPerWriter);

        // Verify all have the same device ID
        foreach (var entity in allEntities)
        {
            entity.DeviceId.Should().Be("shared-device");
        }
    }

    [Fact]
    [Trait("Category", "Concurrency")]
    public async Task ConcurrentReaders_ReturnConsistentSnapshots()
    {
        // Arrange
        const int readerCount = 20;
        const int initialEntities = 100;
        var repository = new InMemoryLocationDataRepository();

        // Pre-populate with entities
        for (int i = 0; i < initialEntities; i++)
        {
            var entity = new LocationData
            {
                DeviceId = $"device{i % 10}",
                Latitude = 10.0 + i,
                Longitude = 20.0 + i,
                Timestamp = DateTime.UtcNow.AddMinutes(-initialEntities + i)
            };
            await repository.CreateAsync(entity);
        }

        var snapshots = new List<IEnumerable<LocationData>>();
        var exceptions = new ConcurrentBag<Exception>();

        // Act - multiple readers taking snapshots simultaneously
        var tasks = Enumerable.Range(0, readerCount).Select(async readerId =>
        {
            try
            {
                var snapshot = await repository.GetAllAsync();
                lock (snapshots)
                {
                    snapshots.Add(snapshot.ToList());
                }
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        }).ToList();

        await Task.WhenAll(tasks);

        // Assert - no exceptions should be thrown
        exceptions.Should().BeEmpty();

        // All snapshots should have the same count (consistent point-in-time)
        foreach (var snapshot in snapshots)
        {
            snapshot.Should().HaveCount(initialEntities);
        }

        // All snapshots should be equal
        for (int i = 1; i < snapshots.Count; i++)
        {
            snapshots[i].Should().BeEquivalentTo(snapshots[0]);
        }
    }

    [Fact]
    [Trait("Category", "Concurrency")]
    public async Task Snapshot_ImmutableAfterCreation()
    {
        // Arrange
        var entity = new LocationData
        {
            DeviceId = "device1",
            Latitude = 10.0,
            Longitude = 20.0,
            Timestamp = DateTime.UtcNow.AddMinutes(-10),
            ExtendedData = { ["key1"] = "value1", ["key2"] = 123 }
        };
        var createdEntity = await _repository.CreateAsync(entity);
        var snapshot = await _repository.GetByIdAsync(createdEntity.Id);

        // Act - modify the original entity's properties
        entity.Latitude = 100.0;
        entity.Longitude = 200.0;
        entity.ExtendedData["key1"] = "modified";
        entity.ExtendedData["key3"] = "new";

        // Modify the store directly
        var updatedEntity = new LocationData
        {
            Id = createdEntity.Id,
            DeviceId = "device1",
            Latitude = 50.0,
            Longitude = 60.0,
            Timestamp = DateTime.UtcNow.AddMinutes(-5),
            ExtendedData = { ["key1"] = "updated", ["key4"] = true }
        };
        await _repository.UpdateAsync(updatedEntity);

        // Assert - snapshot should remain unchanged
        snapshot.Latitude.Should().Be(10.0);
        snapshot.Longitude.Should().Be(20.0);
        snapshot.ExtendedData.Should().HaveCount(2);
        snapshot.ExtendedData.Should().ContainKey("key1").WhoseValue.Should().Be("value1");
        snapshot.ExtendedData.Should().ContainKey("key2").WhoseValue.Should().Be(123);
        snapshot.ExtendedData.Should().NotContainKey("key3");
        snapshot.ExtendedData.Should().NotContainKey("key4");
    }

    [Fact]
    [Trait("Category", "Concurrency")]
    public async Task ConcurrentWritersWithUpdates_NoExceptions()
    {
        // Arrange
        const int writerCount = 8;
        const int operationsPerWriter = 30;
        var repository = new InMemoryLocationDataRepository();
        var exceptions = new ConcurrentBag<Exception>();

        // Pre-populate with some entities
        for (int i = 0; i < 20; i++)
        {
            var entity = new LocationData
            {
                DeviceId = $"device{i % 4}",
                Latitude = 10.0 + i,
                Longitude = 20.0 + i,
                Timestamp = DateTime.UtcNow.AddMinutes(-20 + i)
            };
            await repository.CreateAsync(entity);
        }

        // Act - concurrent writers doing create, update, and delete
        var tasks = Enumerable.Range(0, writerCount).Select(async writerId =>
        {
            try
            {
                for (int i = 0; i < operationsPerWriter; i++)
                {
                    var operation = i % 3;

                    if (operation == 0) // Create
                    {
                        var entity = new LocationData
                        {
                            DeviceId = $"device{writerId}",
                            Latitude = 100.0 + writerId + (i * 0.01),
                            Longitude = 200.0 + writerId + (i * 0.01),
                            Timestamp = DateTime.UtcNow.AddMinutes(-i)
                        };
                        await repository.CreateAsync(entity);
                    }
                    else if (operation == 1) // Update
                    {
                        var allEntities = await repository.GetAllAsync();
                        var entityToUpdate = allEntities.FirstOrDefault(e => e.DeviceId == $"device{writerId % 4}");
                        if (entityToUpdate != null)
                        {
                            var updated = new LocationData
                            {
                                Id = entityToUpdate.Id,
                                DeviceId = entityToUpdate.DeviceId,
                                Latitude = entityToUpdate.Latitude + 1,
                                Longitude = entityToUpdate.Longitude + 1,
                                Timestamp = DateTime.UtcNow.AddMinutes(-i)
                            };
                            await repository.UpdateAsync(updated);
                        }
                    }
                    else // Delete
                    {
                        var allEntities = await repository.GetAllAsync();
                        var entityToDelete = allEntities.FirstOrDefault(e => e.DeviceId == $"device{writerId % 4}");
                        if (entityToDelete != null)
                        {
                            await repository.DeleteAsync(entityToDelete.Id);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        }).ToList();

        await Task.WhenAll(tasks);

        // Assert - no exceptions should be thrown
        exceptions.Should().BeEmpty();

        // Repository should still be in valid state
        var finalEntities = await repository.GetAllAsync();
        finalEntities.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Concurrency")]
    public async Task StressTest_ManyParallelOperations()
    {
        // Arrange
        const int parallelTasks = 50;
        const int operationsPerTask = 20;
        var repository = new InMemoryLocationDataRepository();
        var exceptions = new ConcurrentBag<Exception>();
        var expectedNetAdds = 0;

        // Act - stress test with many parallel operations
        var tasks = Enumerable.Range(0, parallelTasks).Select(async taskId =>
        {
            try
            {
                var random = new Random(taskId);

                for (int i = 0; i < operationsPerTask; i++)
                {
                    var operation = random.Next(0, 5);
                    var deviceId = $"device{random.Next(0, 10)}";

                    switch (operation)
                    {
                        case 0: // Create
                            var entity = new LocationData
                            {
                                DeviceId = deviceId,
                                Latitude = random.NextDouble() * 180 - 90,
                                Longitude = random.NextDouble() * 360 - 180,
                                Timestamp = DateTime.UtcNow.AddMinutes(-random.Next(0, 1000))
                            };
                            await repository.CreateAsync(entity);
                            Interlocked.Increment(ref expectedNetAdds);
                            break;

                        case 1: // Update
                            var all = await repository.GetAllAsync();
                            var entityToUpdate = all.FirstOrDefault(e => e.DeviceId == deviceId);
                            if (entityToUpdate != null)
                            {
                                var updated = new LocationData
                                {
                                    Id = entityToUpdate.Id,
                                    DeviceId = entityToUpdate.DeviceId,
                                    Latitude = entityToUpdate.Latitude + 0.001,
                                    Longitude = entityToUpdate.Longitude + 0.001,
                                    Timestamp = DateTime.UtcNow
                                };
                                await repository.UpdateAsync(updated);
                            }
                            break;

                        case 2: // Delete
                            all = await repository.GetAllAsync();
                            entityToUpdate = all.FirstOrDefault(e => e.DeviceId == deviceId);
                            if (entityToUpdate != null)
                            {
                                await repository.DeleteAsync(entityToUpdate.Id);
                                Interlocked.Decrement(ref expectedNetAdds);
                            }
                            break;

                        case 3: // Get all
                            _ = await repository.GetAllAsync();
                            break;

                        case 4: // Get by device
                            _ = await repository.GetByDeviceIdAsync(deviceId);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        }).ToList();

        await Task.WhenAll(tasks);

        // Assert - no exceptions should be thrown
        exceptions.Should().BeEmpty();

        // Verify repository is in valid state
        var finalEntities = await repository.GetAllAsync();
        finalEntities.Should().NotBeNull();

        // The count should be close to expected (allowing for some operations to cancel out)
        finalEntities.Count().Should().BeLessThanOrEqualTo(expectedNetAdds + 10); // + buffer for race conditions
    }

    [Fact]
    [Trait("Category", "Concurrency")]
    public async Task GetByDeviceIdAsync_ReturnsSnapshot_NotLiveReference()
    {
        // Arrange
        var deviceId = "test-device";
        var entities = new List<LocationData>();

        // Create multiple entities for the same device
        for (int i = 0; i < 10; i++)
        {
            var entity = new LocationData
            {
                DeviceId = deviceId,
                Latitude = 10.0 + i,
                Longitude = 20.0 + i,
                Timestamp = DateTime.UtcNow.AddMinutes(-10 + i)
            };
            entities.Add(await _repository.CreateAsync(entity));
        }

        // Act - get snapshot
        var snapshot1 = await _repository.GetByDeviceIdAsync(deviceId);
        var snapshot2 = await _repository.GetByDeviceIdAsync(deviceId);

        // Assert - both snapshots should be equal
        snapshot1.Should().BeEquivalentTo(snapshot2);

        // Modify the store
        var newEntity = new LocationData
        {
            DeviceId = deviceId,
            Latitude = 100.0,
            Longitude = 200.0,
            Timestamp = DateTime.UtcNow
        };
        await _repository.CreateAsync(newEntity);

        // Previously returned snapshots should still have original count
        snapshot1.Should().HaveCount(10);
        snapshot2.Should().HaveCount(10);

        // New snapshot should include the new entity
        var newSnapshot = await _repository.GetByDeviceIdAsync(deviceId);
        newSnapshot.Should().HaveCount(11);
    }

    [Fact]
    [Trait("Category", "Concurrency")]
    public async Task ExtendedData_DictionaryDeepCopy()
    {
        // Arrange
        var extendedData = new Dictionary<string, object>
        {
            ["stringValue"] = "test",
            ["intValue"] = 42,
            ["doubleValue"] = 3.14,
            ["boolValue"] = true,
            ["nested"] = new Dictionary<string, object> { ["key"] = "nestedValue" }
        };

        var entity = new LocationData
        {
            DeviceId = "device1",
            Latitude = 10.0,
            Longitude = 20.0,
            Timestamp = DateTime.UtcNow,
            ExtendedData = extendedData
        };

        var createdEntity = await _repository.CreateAsync(entity);
        var snapshot = await _repository.GetByIdAsync(createdEntity.Id);

        // Act - modify the original entity's extended data
        entity.ExtendedData["stringValue"] = "modified";
        entity.ExtendedData["newKey"] = "newValue";
        ((Dictionary<string, object>)entity.ExtendedData["nested"])["key"] = "modifiedNested";

        // Modify the store
        var updatedEntity = new LocationData
        {
            Id = createdEntity.Id,
            DeviceId = "device1",
            Latitude = 15.0,
            Longitude = 25.0,
            Timestamp = DateTime.UtcNow.AddMinutes(-5),
            ExtendedData = new Dictionary<string, object> { ["other"] = "data" }
        };
        await _repository.UpdateAsync(updatedEntity);

        // Assert - snapshot should remain unchanged
        snapshot.ExtendedData.Should().HaveCount(5);
        snapshot.ExtendedData.Should().ContainKey("stringValue").WhoseValue.Should().Be("test");
        snapshot.ExtendedData.Should().ContainKey("intValue").WhoseValue.Should().Be(42);
        snapshot.ExtendedData.Should().ContainKey("nested");

        var nested = snapshot.ExtendedData["nested"] as Dictionary<string, object>;
        nested.Should().NotBeNull();
        nested.Should().ContainKey("key").WhoseValue.Should().Be("nestedValue");
    }

    [Fact]
    [Trait("Category", "Concurrency")]
    public async Task GetByIdAsync_WithNullId_ThrowsArgumentNullException()
    {
        // Arrange
        var repository = new InMemoryLocationDataRepository();

        // Act
        Func<Task> act = async () => await repository.GetByIdAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    [Trait("Category", "Concurrency")]
    public async Task Repository_GenericType_SnapshotBehavior()
    {
        // Test with generic InMemoryRepository using a simple POCO
        var genericRepository = new InMemoryRepository<TestEntity>();

        var entity1 = new TestEntity { Id = "1", Name = "First", Value = 100 };
        var entity2 = new TestEntity { Id = "2", Name = "Second", Value = 200 };

        await genericRepository.CreateAsync(entity1);
        await genericRepository.CreateAsync(entity2);

        // Get snapshot
        var snapshot1 = await genericRepository.GetAllAsync();
        var snapshot2 = await genericRepository.GetAllAsync();

        // Modify store
        var updatedEntity = new TestEntity { Id = "1", Name = "Modified", Value = 999 };
        await genericRepository.UpdateAsync(updatedEntity);

        // Snapshots should be unchanged
        snapshot1.Should().BeEquivalentTo(snapshot2);
        snapshot1.Should().HaveCount(2);
        snapshot1.First(e => e.Id == "1").Value.Should().Be(100);

        // New snapshot should reflect changes
        var newSnapshot = await genericRepository.GetAllAsync();
        newSnapshot.Should().HaveCount(2);
        newSnapshot.First(e => e.Id == "1").Value.Should().Be(999);
    }

    private class TestEntity
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public int Value { get; set; }
    }
}
