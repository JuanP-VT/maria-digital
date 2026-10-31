using System.Globalization;
using System.Text;
using System.Text.Json;
using MariaDigital.Application.Services;

namespace MariaDigital.QueuePoller;

public interface IUploadConfirmer
{
    /// <summary>Sends the signed confirmation; true when the backend answered 2xx.</summary>
    Task<bool> ConfirmAsync(string key, long size, CancellationToken ct);
}

/// <summary>Same signed POST the worker sends (worker/src/process.ts).</summary>
public sealed class WebhookClient(
    HttpClient http,
    string apiBaseUrl,
    string secret,
    TimeProvider time
) : IUploadConfirmer
{
    public const string Path = "/api/internal/uploads/confirmed";
    public static readonly TimeSpan Timeout = TimeSpan.FromSeconds(30);

    public async Task<bool> ConfirmAsync(string key, long size, CancellationToken ct)
    {
        var body = JsonSerializer.Serialize(new { key, size });
        var timestamp = time.GetUtcNow().ToUnixTimeSeconds();

        using var request = new HttpRequestMessage(HttpMethod.Post, apiBaseUrl + Path)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json"),
        };
        request.Headers.Add("X-Timestamp", timestamp.ToString(CultureInfo.InvariantCulture));
        request.Headers.Add("X-Signature", WebhookSignature.Sign(secret, timestamp, body));

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeout.CancelAfter(Timeout);
        using var response = await http.SendAsync(request, timeout.Token);
        return response.IsSuccessStatusCode;
    }
}
