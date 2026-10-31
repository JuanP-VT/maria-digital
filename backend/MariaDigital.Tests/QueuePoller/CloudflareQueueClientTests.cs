using System.Net;
using System.Text;
using System.Text.Json;
using MariaDigital.QueuePoller;

namespace MariaDigital.Tests.QueuePoller;

public sealed class CloudflareQueueClientTests
{
	private static readonly PollerOptions Options = new(
		"acc",
		"tok",
		"q1",
		"http://api.test",
		"0123456789abcdef0123456789abcdef",
		TimeSpan.FromSeconds(3),
		7,
		TimeSpan.FromSeconds(30)
	);

	private sealed class StubHandler(HttpStatusCode status, string response) : HttpMessageHandler
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
			return new HttpResponseMessage(status)
			{
				Content = new StringContent(response, Encoding.UTF8, "application/json"),
			};
		}
	}

	private static CloudflareQueueClient Create(StubHandler handler) =>
		new(new HttpClient(handler) { BaseAddress = CloudflareQueueClient.BaseAddress }, Options);

	[Fact]
	public async Task Pull_sends_batch_settings_and_parses_string_and_object_bodies()
	{
		var handler = new StubHandler(
			HttpStatusCode.OK,
			"""
			{"success":true,"result":{"messages":[
			  {"lease_id":"l1","attempts":2,"body":"{\"action\":\"PutObject\"}"},
			  {"lease_id":"l2","body":{"action":"DeleteObject"}},
			  {"lease_id":"l3","attempts":1,"body":"not json"}
			]}}
			"""
		);

		var messages = await Create(handler).PullAsync(CancellationToken.None);

		Assert.Equal(
			"https://api.cloudflare.com/client/v4/accounts/acc/queues/q1/messages/pull",
			handler.Request!.RequestUri!.ToString()
		);
		Assert.Equal("Bearer tok", handler.Request.Headers.Authorization!.ToString());
		using var sent = JsonDocument.Parse(handler.Body!);
		Assert.Equal(7, sent.RootElement.GetProperty("batch_size").GetInt32());
		Assert.Equal(30000, sent.RootElement.GetProperty("visibility_timeout_ms").GetInt32());

		Assert.Equal(3, messages.Count);
		Assert.Equal(2, messages[0].Attempts);
		Assert.Equal("PutObject", messages[0].Body.GetProperty("action").GetString());
		Assert.Equal(1, messages[1].Attempts);
		Assert.Equal("DeleteObject", messages[1].Body.GetProperty("action").GetString());
		Assert.Equal(JsonValueKind.String, messages[2].Body.ValueKind);
	}

	[Fact]
	public async Task Pull_returns_nothing_for_an_empty_queue()
	{
		var handler = new StubHandler(
			HttpStatusCode.OK,
			"""{"success":true,"result":{"messages":[]}}"""
		);

		Assert.Empty(await Create(handler).PullAsync(CancellationToken.None));
	}

	[Fact]
	public async Task Ack_sends_acks_and_retries_with_delay()
	{
		var handler = new StubHandler(HttpStatusCode.OK, """{"success":true,"result":{}}""");
		using var body = JsonDocument.Parse("{}");
		var settled = new[]
		{
			new SettledMessage(
				new QueueMessage("l1", 1, body.RootElement),
				new MessageOutcome.Ack()
			),
			new SettledMessage(
				new QueueMessage("l2", 1, body.RootElement),
				new MessageOutcome.Retry(15)
			),
		};

		await Create(handler).AckAsync(settled, CancellationToken.None);

		Assert.EndsWith("/queues/q1/messages/ack", handler.Request!.RequestUri!.ToString());
		using var sent = JsonDocument.Parse(handler.Body!);
		Assert.Equal(
			"l1",
			sent.RootElement.GetProperty("acks")[0].GetProperty("lease_id").GetString()
		);
		var retry = sent.RootElement.GetProperty("retries")[0];
		Assert.Equal("l2", retry.GetProperty("lease_id").GetString());
		Assert.Equal(15, retry.GetProperty("delay_seconds").GetInt32());
	}

	[Fact]
	public async Task Ack_with_nothing_to_settle_makes_no_request()
	{
		var handler = new StubHandler(HttpStatusCode.OK, "{}");

		await Create(handler).AckAsync([], CancellationToken.None);

		Assert.Null(handler.Request);
	}

	[Fact]
	public async Task A_server_error_is_transient_and_surfaces_status_and_body()
	{
		var handler = new StubHandler(
			HttpStatusCode.BadGateway,
			"""{"errors":[{"message":"nope"}]}"""
		);

		var ex = await Assert.ThrowsAsync<HttpRequestException>(() =>
			Create(handler).PullAsync(CancellationToken.None)
		);

		Assert.Contains("502", ex.Message);
		Assert.Contains("nope", ex.Message);
	}

	[Theory]
	[InlineData(HttpStatusCode.BadRequest)]
	[InlineData(HttpStatusCode.Unauthorized)]
	[InlineData(HttpStatusCode.Forbidden)]
	[InlineData(HttpStatusCode.NotFound)]
	public async Task A_client_error_is_fatal_and_names_the_settings_to_check(HttpStatusCode status)
	{
		var handler = new StubHandler(status, """{"errors":[{"message":"nope"}]}""");

		var ex = await Assert.ThrowsAsync<FatalConfigurationException>(() =>
			Create(handler).PullAsync(CancellationToken.None)
		);

		Assert.Contains(((int)status).ToString(), ex.Message);
		Assert.Contains("QUEUE_ID", ex.Message);
	}

	[Theory]
	[InlineData(HttpStatusCode.RequestTimeout)]
	[InlineData(HttpStatusCode.TooManyRequests)]
	public async Task Timeouts_and_throttling_are_transient(HttpStatusCode status)
	{
		var handler = new StubHandler(status, "{}");

		await Assert.ThrowsAsync<HttpRequestException>(() =>
			Create(handler).PullAsync(CancellationToken.None)
		);
	}
}
