namespace MariaDigital.LocalScanner;

public class OllamaBootstrapper
{
    private readonly HttpClient _http;
    private readonly Func<IDisposable> _startServer;
    private readonly TimeSpan _startupTimeout;
    private readonly TimeSpan _pollInterval;

    public OllamaBootstrapper(
        HttpClient http,
        Func<IDisposable> startServer,
        TimeSpan startupTimeout,
        TimeSpan pollInterval
    )
    {
        _http = http;
        _startServer = startServer;
        _startupTimeout = startupTimeout;
        _pollInterval = pollInterval;
    }

    /// <summary>
    /// Makes sure the server answers and the model is pulled. The returned handle stops the
    /// server only if this call started it.
    /// </summary>
    public async Task<IDisposable> EnsureReadyAsync(OllamaOptions options, CancellationToken ct)
    {
        IDisposable? started = null;
        var tags = await TryGetTagsAsync(options, ct);
        if (tags is null)
        {
            started = _startServer();
            tags = await WaitForTagsAsync(options, ct);
            if (tags is null)
            {
                started.Dispose();
                throw new InvalidOperationException(
                    $"Ollama did not respond at {options.BaseUrl} within {_startupTimeout.TotalSeconds:0}s."
                );
            }
        }

        if (!OllamaTags.IsModelInstalled(tags, options.Model))
        {
            started?.Dispose();
            throw new InvalidOperationException(
                $"Model '{options.Model}' is not installed. Run: ollama pull {options.Model}"
            );
        }

        return started ?? NoOp.Instance;
    }

    private async Task<string?> WaitForTagsAsync(OllamaOptions options, CancellationToken ct)
    {
        var deadline = DateTime.UtcNow + _startupTimeout;
        while (DateTime.UtcNow < deadline)
        {
            await Task.Delay(_pollInterval, ct);
            var tags = await TryGetTagsAsync(options, ct);
            if (tags is not null)
            {
                return tags;
            }
        }

        return null;
    }

    private async Task<string?> TryGetTagsAsync(OllamaOptions options, CancellationToken ct)
    {
        try
        {
            using var response = await _http.GetAsync($"{options.BaseUrl}/api/tags", ct);
            return response.IsSuccessStatusCode
                ? await response.Content.ReadAsStringAsync(ct)
                : null;
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    private sealed class NoOp : IDisposable
    {
        public static readonly NoOp Instance = new();

        public void Dispose() { }
    }
}
