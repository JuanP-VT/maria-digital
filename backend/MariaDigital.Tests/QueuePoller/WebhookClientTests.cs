using System.Net;
using System.Text.Json;
using MariaDigital.Application.Services;
using MariaDigital.QueuePoller;

namespace MariaDigital.Tests.QueuePoller;

public sealed class WebhookClientTests
{
	private const string Secret = "0123456789abcdef0123456789abcdef";
	private static readonly DateTimeOffset Now = new(2026, 10, 30, 12, 0, 0, TimeSpan.Zero);

	private sealed class CapturingHandler(HttpStatusCode status) : HttpMessageHandler
	{
		public HttpRequestMessage? Request { get; private set; }
		public string? Body { get; private set; }

		protected override async Task<HttpResponseMessage> SendAsync(
			HttpRequestMessage request,
			CancellationToken cancellationToken
		)
		{
			Request = request;
			Body = await request.Content!.ReadAsStringAsync(cancellationToken);
			return new HttpResponseMessage(status);
		}
	}

	private static WebhookClient Create(CapturingHandler handler) =>
		new(new HttpClient(handler), "http://api.test", Secret, new FixedTimeProvider(Now));

	[Fact]
	public async Task Posts_the_signed_key_and_size()
	{
		var handler = new CapturingHandler(HttpStatusCode.NoContent);

		var ok = await Create(handler)
			.ConfirmAsync("submissions/a/b/c", 42, CancellationToken.None);

		Assert.True(ok);
		Assert.Equal(
			"http://api.test/api/internal/uploads/confirmed",
			handler.Request!.RequestUri!.ToString()
		);
		Assert.Equal("""{"key":"submissions/a/b/c","size":42}""", handler.Body);
		var timestamp = Now.ToUnixTimeSeconds();
		Assert.Equal(
			timestamp.ToString(),
			handler.Request.Headers.GetValues("X-Timestamp").Single()
		);
		Assert.Equal(
			WebhookSignature.Sign(Secret, timestamp, handler.Body!),
			handler.Request.Headers.GetValues("X-Signature").Single()
		);
	}

	[Theory]
	[InlineData(HttpStatusCode.Unauthorized)]
	[InlineData(HttpStatusCode.NotFound)]
	[InlineData(HttpStatusCode.InternalServerError)]
	public async Task Non_2xx_is_a_failure(HttpStatusCode status)
	{
		var ok = await Create(new CapturingHandler(status))
			.ConfirmAsync("submissions/a", 1, CancellationToken.None);

		Assert.False(ok);
	}

	[Fact]
	public async Task The_body_round_trips_as_json()
	{
		var handler = new CapturingHandler(HttpStatusCode.NoContent);
		await Create(handler).ConfirmAsync("submissions/ñ\"x", 1, CancellationToken.None);

		using var doc = JsonDocument.Parse(handler.Body!);
		Assert.Equal("submissions/ñ\"x", doc.RootElement.GetProperty("key").GetString());
	}

	private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
	{
		public override DateTimeOffset GetUtcNow() => now;
	}
}
