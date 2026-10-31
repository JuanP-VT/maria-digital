using System.Net;
using MariaDigital.LocalScanner;

namespace MariaDigital.Tests.LocalScanner;

public class OllamaBootstrapperTests
{
	private const string Tags = """{"models":[{"name":"qwen3.5:9b"}]}""";
	private static readonly OllamaOptions Options = new("http://ollama.test", "qwen3.5:9b");

	private sealed class StubHandler(Func<int, HttpResponseMessage> respond) : HttpMessageHandler
	{
		private int _calls;

		protected override Task<HttpResponseMessage> SendAsync(
			HttpRequestMessage request,
			CancellationToken cancellationToken
		) => Task.FromResult(respond(_calls++));
	}

	private sealed class FakeServer : IDisposable
	{
		public int Starts { get; set; }
		public bool Stopped { get; private set; }

		public void Dispose() => Stopped = true;
	}

	private static HttpResponseMessage Ok() =>
		new(HttpStatusCode.OK) { Content = new StringContent(Tags) };

	private static OllamaBootstrapper Create(StubHandler handler, FakeServer server) =>
		new(
			new HttpClient(handler),
			() =>
			{
				server.Starts++;
				return server;
			},
			TimeSpan.FromMilliseconds(200),
			TimeSpan.FromMilliseconds(10)
		);

	[Fact]
	public async Task AlreadyRunning_DoesNotStartServer_AndDoesNotStopItOnDispose()
	{
		var server = new FakeServer();
		var bootstrapper = Create(new StubHandler(_ => Ok()), server);

		var handle = await bootstrapper.EnsureReadyAsync(Options, CancellationToken.None);
		handle.Dispose();

		Assert.Equal(0, server.Starts);
		Assert.False(server.Stopped);
	}

	[Fact]
	public async Task NotRunning_StartsServer_WaitsUntilReady_AndStopsItOnDispose()
	{
		var server = new FakeServer();
		var handler = new StubHandler(call =>
			call < 3 ? throw new HttpRequestException("refused") : Ok()
		);
		var bootstrapper = Create(handler, server);

		var handle = await bootstrapper.EnsureReadyAsync(Options, CancellationToken.None);

		Assert.Equal(1, server.Starts);
		Assert.False(server.Stopped);
		handle.Dispose();
		Assert.True(server.Stopped);
	}

	[Fact]
	public async Task NeverBecomesReady_StopsServerAndThrows()
	{
		var server = new FakeServer();
		var handler = new StubHandler(_ => throw new HttpRequestException("refused"));
		var bootstrapper = Create(handler, server);

		await Assert.ThrowsAsync<InvalidOperationException>(() =>
			bootstrapper.EnsureReadyAsync(Options, CancellationToken.None)
		);

		Assert.True(server.Stopped);
	}

	[Fact]
	public async Task ModelMissing_ThrowsWithPullHint()
	{
		var server = new FakeServer();
		var bootstrapper = Create(new StubHandler(_ => Ok()), server);

		var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
			bootstrapper.EnsureReadyAsync(
				Options with
				{
					Model = "other:1b",
				},
				CancellationToken.None
			)
		);

		Assert.Contains("ollama pull other:1b", ex.Message);
	}

	[Fact]
	public async Task ModelMissingAfterStartingServer_StopsServer()
	{
		var server = new FakeServer();
		var handler = new StubHandler(call =>
			call == 0 ? throw new HttpRequestException("refused") : Ok()
		);
		var bootstrapper = Create(handler, server);

		await Assert.ThrowsAsync<InvalidOperationException>(() =>
			bootstrapper.EnsureReadyAsync(
				Options with
				{
					Model = "other:1b",
				},
				CancellationToken.None
			)
		);

		Assert.True(server.Stopped);
	}

	[Fact]
	public async Task RequestTimeout_IsTreatedAsNotRunning()
	{
		var server = new FakeServer();
		var handler = new StubHandler(call =>
			call == 0 ? throw new TaskCanceledException("timeout") : Ok()
		);
		var bootstrapper = Create(handler, server);

		await bootstrapper.EnsureReadyAsync(Options, CancellationToken.None);

		Assert.Equal(1, server.Starts);
	}

	[Fact]
	public async Task ErrorStatus_IsTreatedAsNotRunning()
	{
		var server = new FakeServer();
		var handler = new StubHandler(call =>
			call == 0 ? new HttpResponseMessage(HttpStatusCode.BadGateway) : Ok()
		);
		var bootstrapper = Create(handler, server);

		await bootstrapper.EnsureReadyAsync(Options, CancellationToken.None);

		Assert.Equal(1, server.Starts);
	}
}
