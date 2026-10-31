using System.Text.Json;
using MariaDigital.QueuePoller;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace MariaDigital.Tests.QueuePoller;

public sealed class MessageProcessorTests
{
	private readonly IUploadConfirmer _confirmer = Substitute.For<IUploadConfirmer>();
	private readonly List<string> _logs = [];

	private MessageProcessor Create() => new(_confirmer, () => 1.0, _logs.Add);

	private static QueueMessage Message(string json, int attempts = 1)
	{
		using var doc = JsonDocument.Parse(json);
		return new QueueMessage("lease", attempts, doc.RootElement.Clone());
	}

	private const string Put =
		"""{"action":"PutObject","object":{"key":"submissions/a/b/c","size":9}}""";

	[Fact]
	public async Task A_2xx_confirmation_is_acked()
	{
		_confirmer.ConfirmAsync("submissions/a/b/c", 9, Arg.Any<CancellationToken>()).Returns(true);

		var outcome = await Create().ProcessAsync(Message(Put), CancellationToken.None);

		Assert.IsType<MessageOutcome.Ack>(outcome);
	}

	[Fact]
	public async Task A_rejected_confirmation_is_retried_with_backoff()
	{
		_confirmer
			.ConfirmAsync(Arg.Any<string>(), Arg.Any<long>(), Arg.Any<CancellationToken>())
			.Returns(false);

		var outcome = await Create()
			.ProcessAsync(Message(Put, attempts: 2), CancellationToken.None);

		Assert.Equal(new MessageOutcome.Retry(15), outcome);
	}

	[Fact]
	public async Task A_network_error_is_retried()
	{
		_confirmer
			.ConfirmAsync(Arg.Any<string>(), Arg.Any<long>(), Arg.Any<CancellationToken>())
			.ThrowsAsync(new HttpRequestException("boom"));

		var outcome = await Create().ProcessAsync(Message(Put), CancellationToken.None);

		Assert.Equal(new MessageOutcome.Retry(5), outcome);
		Assert.Contains(_logs, l => l.Contains("boom"));
	}

	[Fact]
	public async Task A_timeout_is_retried()
	{
		_confirmer
			.ConfirmAsync(Arg.Any<string>(), Arg.Any<long>(), Arg.Any<CancellationToken>())
			.ThrowsAsync(new TaskCanceledException());

		var outcome = await Create().ProcessAsync(Message(Put), CancellationToken.None);

		Assert.IsType<MessageOutcome.Retry>(outcome);
	}

	[Fact]
	public async Task Shutdown_cancellation_propagates()
	{
		using var cts = new CancellationTokenSource();
		await cts.CancelAsync();
		_confirmer
			.ConfirmAsync(Arg.Any<string>(), Arg.Any<long>(), Arg.Any<CancellationToken>())
			.ThrowsAsync(new OperationCanceledException());

		await Assert.ThrowsAsync<OperationCanceledException>(() =>
			Create().ProcessAsync(Message(Put), cts.Token)
		);
	}

	[Fact]
	public async Task Ignored_messages_are_acked_without_calling_the_backend()
	{
		var outcome = await Create()
			.ProcessAsync(
				Message("""{"action":"DeleteObject","object":{"key":"submissions/a","size":1}}"""),
				CancellationToken.None
			);

		Assert.IsType<MessageOutcome.Ack>(outcome);
		await _confirmer.DidNotReceiveWithAnyArgs().ConfirmAsync(default!, default, default);
	}

	[Fact]
	public async Task Invalid_messages_are_logged_and_acked()
	{
		var outcome = await Create().ProcessAsync(Message("[]"), CancellationToken.None);

		Assert.IsType<MessageOutcome.Ack>(outcome);
		Assert.Contains(_logs, l => l.StartsWith("Invalid message"));
	}
}
