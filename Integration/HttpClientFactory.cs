#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GpsTrackerProtocol.Integration;

using Microsoft.Extensions.Logging;

/// <summary>
/// HTTP client factory for managing external API integrations.
/// Configures timeouts, retries, and user agents.
/// </summary>
public interface IHttpClientFactory
{
    HttpClient CreateClient(string name = "default");
    HttpClient CreateClientWithAuth(string name, string authToken);
}

public class HttpClientFactoryService : IHttpClientFactory
{
    private readonly ILogger<HttpClientFactoryService> _logger;
    private readonly Dictionary<string, HttpClient> _clients = new();

    public HttpClientFactoryService(ILogger<HttpClientFactoryService> logger)
    {
        _logger = logger;
    }

    public HttpClient CreateClient(string name = "default")
    {
        lock (_clients)
        {
            if (_clients.ContainsKey(name))
                return _clients[name];

            var handler = new HttpClientHandler
            {
                AllowAutoRedirect = true,
                UseCookies = false
            };

            var client = new HttpClient(handler, disposeHandler: false)
            {
                Timeout = TimeSpan.FromSeconds(30)
            };

            client.DefaultRequestHeaders.Add("User-Agent", "GpsTrackerProtocol/1.0");
            client.DefaultRequestHeaders.Add("Accept", "application/json");

            _clients[name] = client;
            _logger.LogInformation("HTTP client '{ClientName}' created", name);

            return client;
        }
    }

    public HttpClient CreateClientWithAuth(string name, string authToken)
    {
        var client = CreateClient(name);
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);
        return client;
    }
}

/// <summary>
/// Base class for external API integrations with retry logic.
/// </summary>
public abstract class ExternalApiClient
{
    protected readonly HttpClient _httpClient;
    protected readonly ILogger _logger;
    protected readonly int _maxRetries = 3;
    protected readonly TimeSpan _retryDelay = TimeSpan.FromSeconds(1);

    protected ExternalApiClient(HttpClient httpClient, ILogger logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    protected async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> request)
    {
        int attempt = 0;
        while (attempt < _maxRetries)
        {
            try
            {
                return await request();
            }
            catch (HttpRequestException ex) when (attempt < _maxRetries - 1)
            {
                attempt++;
                _logger.LogWarning(ex, "Request failed, attempt {Attempt}/{MaxRetries}", attempt, _maxRetries);
                await Task.Delay(_retryDelay);
            }
        }

        throw new InvalidOperationException("All retry attempts failed");
    }

    protected string BuildQueryString(Dictionary<string, string> parameters)
    {
        if (parameters is null || parameters.Count == 0)
            return string.Empty;

        var query = string.Join("&", parameters.Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));
        return "?" + query;
    }
}
