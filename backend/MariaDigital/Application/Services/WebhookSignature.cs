using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace MariaDigital.Application.Services;

/// <summary>
/// HMAC-SHA256 over "{unix timestamp}.{raw body}", hex encoded. The timestamp bounds how long a
/// captured request can be replayed; replays inside the window are harmless because confirming an
/// upload is idempotent.
/// </summary>
public static class WebhookSignature
{
	public static readonly TimeSpan Tolerance = TimeSpan.FromMinutes(5);

	public static string Sign(string secret, long timestamp, string body)
	{
		var payload = Encoding.UTF8.GetBytes(
			$"{timestamp.ToString(CultureInfo.InvariantCulture)}.{body}"
		);
		return Convert.ToHexStringLower(
			HMACSHA256.HashData(Encoding.UTF8.GetBytes(secret), payload)
		);
	}

	public static bool IsValid(
		string secret,
		string? timestampHeader,
		string? signatureHeader,
		string body,
		DateTimeOffset now
	)
	{
		if (
			string.IsNullOrEmpty(signatureHeader)
			|| !long.TryParse(
				timestampHeader,
				NumberStyles.None,
				CultureInfo.InvariantCulture,
				out var timestamp
			)
		)
			return false;

		// Out-of-range values would throw below, so treat them as a failed check.
		if (timestamp > DateTimeOffset.MaxValue.ToUnixTimeSeconds())
			return false;

		var age = now - DateTimeOffset.FromUnixTimeSeconds(timestamp);
		if (age.Duration() > Tolerance)
			return false;

		var expected = Encoding.ASCII.GetBytes(Sign(secret, timestamp, body));
		var actual = Encoding.ASCII.GetBytes(signatureHeader.ToLowerInvariant());
		return CryptographicOperations.FixedTimeEquals(expected, actual);
	}
}
