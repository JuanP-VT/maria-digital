using MariaDigital.Application.Services;

namespace MariaDigital.Tests.Application.Services;

public sealed class WebhookSignatureTests
{
	private const string Secret = "0123456789abcdef0123456789abcdef";
	private const string Body = """{"key":"submissions/a/registration/b","size":5000}""";
	private static readonly DateTimeOffset Now = new(2026, 10, 29, 12, 0, 0, TimeSpan.Zero);

	private static string Sign(DateTimeOffset at, string body = Body, string secret = Secret) =>
		WebhookSignature.Sign(secret, at.ToUnixTimeSeconds(), body);

	private static bool Check(
		DateTimeOffset at,
		string? signature,
		string body = Body,
		string secret = Secret
	) => WebhookSignature.IsValid(secret, at.ToUnixTimeSeconds().ToString(), signature, body, Now);

	[Fact]
	public void A_fresh_correct_signature_is_valid()
	{
		Assert.True(Check(Now, Sign(Now)));
	}

	[Fact]
	public void The_signature_is_the_known_hmac_of_timestamp_dot_body()
	{
		// Pinned so the Worker (which must produce the same value) cannot drift from the backend.
		Assert.Equal(
			"417adf36b901772f658597a76d329694e335e65fc0849d7715c3ca33406309b5",
			WebhookSignature.Sign(Secret, 1_700_000_000, "{}")
		);
	}

	[Fact]
	public void The_signature_may_be_uppercase_hex()
	{
		Assert.True(Check(Now, Sign(Now).ToUpperInvariant()));
	}

	[Fact]
	public void A_tampered_body_is_rejected()
	{
		Assert.False(Check(Now, Sign(Now), body: Body.Replace("5000", "9")));
	}

	[Fact]
	public void A_different_secret_is_rejected()
	{
		Assert.False(Check(Now, Sign(Now, secret: "another-secret-another-secret-0000")));
	}

	[Fact]
	public void A_tampered_timestamp_is_rejected()
	{
		var signature = Sign(Now);
		Assert.False(Check(Now.AddSeconds(1), signature));
	}

	[Theory]
	[InlineData(-299, true)]
	[InlineData(-301, false)]
	[InlineData(299, true)]
	[InlineData(301, false)]
	public void Only_timestamps_within_the_tolerance_are_accepted(int offsetSeconds, bool expected)
	{
		var at = Now.AddSeconds(offsetSeconds);
		Assert.Equal(expected, Check(at, Sign(at)));
	}

	[Theory]
	[InlineData(null, "abc")]
	[InlineData("", "abc")]
	[InlineData("not-a-number", "abc")]
	[InlineData("-5", "abc")]
	[InlineData("99999999999999999999", "abc")]
	[InlineData("1793275200", null)]
	[InlineData("1793275200", "")]
	public void Missing_or_malformed_headers_are_rejected(string? timestamp, string? signature)
	{
		Assert.False(WebhookSignature.IsValid(Secret, timestamp, signature, Body, Now));
	}

	[Fact]
	public void A_signature_of_the_wrong_length_is_rejected()
	{
		Assert.False(Check(Now, Sign(Now)[..10]));
	}
}
