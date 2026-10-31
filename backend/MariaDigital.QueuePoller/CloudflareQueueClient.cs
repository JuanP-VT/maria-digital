using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace MariaDigital.QueuePoller;

public interface IQueueClient
{
    Task<IReadOnlyList<QueueMessage>> PullAsync(CancellationToken ct);

    Task AckAsync(IReadOnlyList<SettledMessage> settled, CancellationToken ct);
}

/// <summary>Cloudflare Queues HTTP pull consumer API.</summary>
public sealed class CloudflareQueueClient(HttpClient http, PollerOptions options) : IQueueClient
{
    public static readonly Uri BaseAddress = new("https://api.cloudflare.com/client/v4/");

    private string Endpoint(string action) =>
        $"accounts/{options.AccountId}/queues/{options.QueueId}/messages/{action}";

    public async Task<IReadOnlyList<QueueMessage>> PullAsync(CancellationToken ct)
    {
        var payload = new
        {
            batch_size = options.BatchSize,
            visibility_timeout_ms = (int)options.VisibilityTimeout.TotalMilliseconds,
        };
        using var document = await SendAsync(Endpoint("pull"), payload, ct);
        return ParsePull(document.RootElement);
    }

    public async Task AckAsync(IReadOnlyList<SettledMessage> settled, CancellationToken ct)
    {
        if (settled.Count == 0)
            return;

        using var _ = await SendAsync(Endpoint("ack"), BuildAck(settled), ct);
    }

    public static IReadOnlyList<QueueMessage> ParsePull(JsonElement root)
    {
        if (
            !root.TryGetProperty("result", out var result)
            || !result.TryGetProperty("messages", out var messages)
            || messages.ValueKind != JsonValueKind.Array
        )
            return [];

        var list = new List<QueueMessage>();
        foreach (var item in messages.EnumerateArray())
        {
            var leaseId = item.GetProperty("lease_id").GetString()!;
            var attempts = item.TryGetProperty("attempts", out var a) ? a.GetInt32() : 1;
            list.Add(new QueueMessage(leaseId, attempts, ParseBody(item.GetProperty("body"))));
        }

        return list;
    }

    /// <summary>The body arrives as a JSON string or as an inline object depending on the producer.</summary>
    public static JsonElement ParseBody(JsonElement body)
    {
        if (body.ValueKind == JsonValueKind.String)
        {
            try
            {
                using var inner = JsonDocument.Parse(body.GetString()!);
                return inner.RootElement.Clone();
            }
            catch (JsonException)
            {
                return body.Clone();
            }
        }

        return body.Clone();
    }

    public static object BuildAck(IReadOnlyList<SettledMessage> settled) =>
        new
        {
            acks = settled
                .Where(s => s.Outcome is MessageOutcome.Ack)
                .Select(s => new { lease_id = s.Message.LeaseId })
                .ToArray(),
            retries = settled
                .Where(s => s.Outcome is MessageOutcome.Retry)
                .Select(s => new
                {
                    lease_id = s.Message.LeaseId,
                    delay_seconds = ((MessageOutcome.Retry)s.Outcome).DelaySeconds,
                })
                .ToArray(),
        };

    /// <summary>Client errors other than timeouts and throttling will not fix themselves on retry.</summary>
    public static bool IsFatal(System.Net.HttpStatusCode status) =>
        (int)status is >= 400 and < 500
        && status
            is not (
                System.Net.HttpStatusCode.RequestTimeout
                or System.Net.HttpStatusCode.TooManyRequests
            );

    private async Task<JsonDocument> SendAsync(string path, object payload, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, path)
        {
            Content = JsonContent.Create(payload),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", options.ApiToken);

        using var response = await http.SendAsync(request, ct);
        var text = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode)
        {
            var message = $"Cloudflare API {(int)response.StatusCode} on {path}: {text}";
            if (IsFatal(response.StatusCode))
            {
                throw new FatalConfigurationException(
                    $"{message}{Environment.NewLine}Check CLOUDFLARE_ACCOUNT_ID, CLOUDFLARE_API_TOKEN (Queues read/write), QUEUE_ID, and that the queue has an HTTP pull consumer."
                );
            }

            throw new HttpRequestException(message);
        }

        return JsonDocument.Parse(text);
    }
}
