using System.Globalization;

namespace MariaDigital.QueuePoller;

public sealed record PollerOptions(
    string AccountId,
    string ApiToken,
    string QueueId,
    string ApiBaseUrl,
    string WebhookSecret,
    TimeSpan PollInterval,
    int BatchSize,
    TimeSpan VisibilityTimeout
)
{
    public const string DefaultApiBaseUrl = "https://localhost:7028";
    public const int MinSecretLength = 32;

    /// <summary>Reads and validates every setting; throws one error listing all problems.</summary>
    public static PollerOptions Resolve(Func<string, string?> getEnv)
    {
        var errors = new List<string>();

        var accountId = Require(getEnv, "CLOUDFLARE_ACCOUNT_ID", errors);
        var apiToken = Require(getEnv, "CLOUDFLARE_API_TOKEN", errors);
        var queueId = Require(getEnv, "QUEUE_ID", errors);
        var secret = Require(getEnv, "UploadWebhook__Secret", errors);
        if (secret is not null && secret.Length < MinSecretLength)
        {
            errors.Add(
                $"UploadWebhook__Secret must be at least {MinSecretLength} characters (it is {secret.Length})."
            );
        }

        var apiBaseUrl = (Read(getEnv, "API_BASE_URL") ?? DefaultApiBaseUrl).TrimEnd('/');
        if (!IsHttpUrl(apiBaseUrl))
        {
            errors.Add($"API_BASE_URL must be an absolute http(s) URL (got '{apiBaseUrl}').");
        }

        var pollSeconds = ReadInt(getEnv, "POLL_INTERVAL_SECONDS", 3, 1, 3600, errors);
        var batchSize = ReadInt(getEnv, "BATCH_SIZE", 10, 1, 100, errors);
        var visibilityMs = ReadInt(
            getEnv,
            "VISIBILITY_TIMEOUT_MS",
            30_000,
            1_000,
            43_200_000,
            errors
        );

        if (errors.Count > 0)
        {
            throw new ArgumentException(
                "Invalid configuration:"
                    + string.Concat(errors.Select(e => $"{Environment.NewLine}  - {e}"))
            );
        }

        return new PollerOptions(
            accountId!,
            apiToken!,
            queueId!,
            apiBaseUrl,
            secret!,
            TimeSpan.FromSeconds(pollSeconds),
            batchSize,
            TimeSpan.FromMilliseconds(visibilityMs)
        );
    }

    private static string? Read(Func<string, string?> getEnv, string name)
    {
        var value = getEnv(name)?.Trim();
        return string.IsNullOrEmpty(value) ? null : value;
    }

    private static string? Require(Func<string, string?> getEnv, string name, List<string> errors)
    {
        var value = Read(getEnv, name);
        if (value is null)
            errors.Add($"{name} is not set.");
        return value;
    }

    private static bool IsHttpUrl(string url) =>
        Uri.TryCreate(url, UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);

    private static int ReadInt(
        Func<string, string?> getEnv,
        string name,
        int fallback,
        int min,
        int max,
        List<string> errors
    )
    {
        var raw = Read(getEnv, name);
        if (raw is null)
            return fallback;
        if (
            !int.TryParse(raw, NumberStyles.None, CultureInfo.InvariantCulture, out var value)
            || value < min
            || value > max
        )
        {
            errors.Add($"{name} must be an integer between {min} and {max} (got '{raw}').");
            return fallback;
        }

        return value;
    }
}
