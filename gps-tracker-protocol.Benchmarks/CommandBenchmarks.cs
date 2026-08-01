using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Engines;
using System;
using System.Collections.Generic;
using System.Linq;
using GpsTrackerProtocol.Domain.Models;

namespace GpsTrackerProtocol.Benchmarks
{
    [MemoryDiagnoser]
    public class CommandBenchmarks
    {
        private Command _command;
        private List<Command> _commands;

        [GlobalSetup]
        public void Setup()
        {
            _command = new Command();
            _commands = new List<Command>();
            for (int i = 0; i < 1000; i++)
            {
                _commands.Add(new Command());
            }
        }

        [Params(10, 100, 1000)]
        public int N;

        [Benchmark]
        public void BenchmarkCommandToString()
        {
            for (int i = 0; i < N; i++)
            {
                _command.ToString();
            }
        }

        [Benchmark]
        public void BenchmarkCommandEquals()
        {
            for (int i = 0; i < N; i++)
            {
                _command.Equals(_commands[i]);
            }
        }

        [Benchmark]
        public void BenchmarkCommandGetHashCode()
        {
            for (int i = 0; i < N; i++)
            {
                _command.GetHashCode();
            }
        }
    }
}
