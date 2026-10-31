namespace MariaDigital.QueuePoller;

internal static class Program
{
    private static async Task<int> Main()
    {
        PollerOptions options;
        try
        {
            options = PollerOptions.Resolve(Environment.GetEnvironmentVariable);
        }
        catch (ArgumentException ex)
        {
            Console.Error.WriteLine(ex.Message);
            return 2;
        }

        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };

        using var cloudflare = new HttpClient
        {
            BaseAddress = CloudflareQueueClient.BaseAddress,
            Timeout = TimeSpan.FromSeconds(60),
        };
        // The local API often runs on the ASP.NET dev certificate; accept it for loopback only.
        using var backendHandler = new HttpClientHandler();
        if (IsLoopback(options.ApiBaseUrl))
        {
            backendHandler.ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
        }
        using var backend = new HttpClient(backendHandler);

        Action<string> log = message => Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {message}");
        var processor = new MessageProcessor(
            new WebhookClient(
                backend,
                options.ApiBaseUrl,
                options.WebhookSecret,
                TimeProvider.System
            ),
            Random.Shared.NextDouble,
            log
        );
        var loop = new PollLoop(
            new CloudflareQueueClient(cloudflare, options),
            processor,
            options.PollInterval,
            log
        );

        log(
            $"Polling queue {options.QueueId}, confirming to {options.ApiBaseUrl}. Ctrl+C to stop."
        );
        try
        {
            await loop.RunAsync(cts.Token);
        }
        catch (FatalConfigurationException ex)
        {
            Console.Error.WriteLine($"FATAL: {ex.Message}");
            return 1;
        }

        log("Stopped.");
        return 0;
    }

    private static bool IsLoopback(string url) =>
        Uri.TryCreate(url, UriKind.Absolute, out var uri) && uri.IsLoopback;
}
