using System.Text.Json;
using MariaDigital.QueuePoller;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace MariaDigital.Tests.QueuePoller;

public sealed class PollLoopTests
{
	private readonly IQueueClient _queue = Substitute.For<IQueueClient>();
	private readonly IUploadConfirmer _confirmer = Substitute.For<IUploadConfirmer>();
	private readonly List<string> _logs = [];

	private PollLoop Create(TimeSpan? interval = null) =>
		new(
			_queue,
			new MessageProcessor(_confirmer, () => 1.0, _logs.Add),
			interval ?? TimeSpan.FromMilliseconds(5),
			_logs.Add
		);

	private static QueueMessage Message(string lease, string key)
	{
		using var doc = JsonDocument.Parse(
			$$$"""{"action":"PutObject","object":{"key":"{{{key}}}","size":1}}"""
		);
		return new QueueMessage(lease, 1, doc.RootElement.Clone());
	}

	[Fact]
	public async Task RunOnce_settles_each_message_and_acks_the_batch()
	{
		_queue
			.PullAsync(Arg.Any<CancellationToken>())
			.Returns([Message("ok", "submissions/ok"), Message("bad", "submissions/bad")]);
		_confirmer.ConfirmAsync("submissions/ok", 1, Arg.Any<CancellationToken>()).Returns(true);
		_confirmer.ConfirmAsync("submissions/bad", 1, Arg.Any<CancellationToken>()).Returns(false);

		var handled = await Create().RunOnceAsync(CancellationToken.None);

		Assert.Equal(2, handled);
		await _queue
			.Received(1)
			.AckAsync(
				Arg.Is<IReadOnlyList<SettledMessage>>(s =>
					s.Count == 2
					&& s[0].Outcome is MessageOutcome.Ack
					&& s[1].Outcome is MessageOutcome.Retry
				),
				Arg.Any<CancellationToken>()
			);
	}

	[Fact]
	public async Task RunOnce_does_not_ack_an_empty_pull()
	{
		_queue.PullAsync(Arg.Any<CancellationToken>()).Returns([]);

		Assert.Equal(0, await Create().RunOnceAsync(CancellationToken.None));
		await _queue.DidNotReceiveWithAnyArgs().AckAsync(default!, default);
	}

	[Fact]
	public async Task Run_stops_on_a_fatal_configuration_error()
	{
		_queue
			.PullAsync(Arg.Any<CancellationToken>())
			.ThrowsAsync(new FatalConfigurationException("bad token"));

		await Assert.ThrowsAsync<FatalConfigurationException>(() =>
			Create().RunAsync(CancellationToken.None)
		);
	}

	[Fact]
	public async Task Run_survives_a_failed_poll_and_stops_on_cancellation()
	{
		using var cts = new CancellationTokenSource();
		var calls = 0;
		_queue
			.PullAsync(Arg.Any<CancellationToken>())
			.Returns(_ =>
			{
				if (++calls == 1)
					throw new HttpRequestException("offline");
				cts.Cancel();
				return Task.FromResult<IReadOnlyList<QueueMessage>>([]);
			});

		await Create().RunAsync(cts.Token);

		Assert.Equal(2, calls);
		Assert.Contains(_logs, l => l.Contains("offline"));
	}
}
