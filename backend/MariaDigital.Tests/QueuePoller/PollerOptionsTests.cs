using MariaDigital.QueuePoller;

namespace MariaDigital.Tests.QueuePoller;

public sealed class PollerOptionsTests
{
	private const string Secret = "0123456789abcdef0123456789abcdef";

	private static Func<string, string?> Env(params (string Name, string Value)[] values) =>
		name => values.FirstOrDefault(v => v.Name == name).Value;

	private static (string, string)[] Required() =>
		[
			("CLOUDFLARE_ACCOUNT_ID", "acc"),
			("CLOUDFLARE_API_TOKEN", "tok"),
			("QUEUE_ID", "q1"),
			("UploadWebhook__Secret", Secret),
		];

	[Fact]
	public void Resolve_UsesDefaults()
	{
		var options = PollerOptions.Resolve(Env(Required()));

		Assert.Equal("https://localhost:7028", options.ApiBaseUrl);
		Assert.Equal(TimeSpan.FromSeconds(3), options.PollInterval);
		Assert.Equal(10, options.BatchSize);
		Assert.Equal(TimeSpan.FromSeconds(30), options.VisibilityTimeout);
	}

	[Fact]
	public void Resolve_ReadsOverridesAndTrimsTrailingSlash()
	{
		var options = PollerOptions.Resolve(
			Env([.. Required(), ("API_BASE_URL", "http://x:5000/"), ("BATCH_SIZE", "5")])
		);

		Assert.Equal("http://x:5000", options.ApiBaseUrl);
		Assert.Equal(5, options.BatchSize);
	}

	[Fact]
	public void Resolve_ListsEveryMissingVariable()
	{
		var ex = Assert.Throws<ArgumentException>(() => PollerOptions.Resolve(Env()));

		Assert.Contains("CLOUDFLARE_ACCOUNT_ID", ex.Message);
		Assert.Contains("CLOUDFLARE_API_TOKEN", ex.Message);
		Assert.Contains("QUEUE_ID", ex.Message);
		Assert.Contains("UploadWebhook__Secret", ex.Message);
	}

	[Fact]
	public void Resolve_ReportsAllProblemsAtOnce()
	{
		var ex = Assert.Throws<ArgumentException>(() =>
			PollerOptions.Resolve(Env(("UploadWebhook__Secret", "short"), ("BATCH_SIZE", "0")))
		);

		Assert.Contains("CLOUDFLARE_ACCOUNT_ID is not set", ex.Message);
		Assert.Contains("QUEUE_ID is not set", ex.Message);
		Assert.Contains("at least 32 characters", ex.Message);
		Assert.Contains("BATCH_SIZE must be an integer", ex.Message);
	}

	[Theory]
	[InlineData("localhost:7028")]
	[InlineData("ftp://x")]
	[InlineData("not a url")]
	public void Resolve_RejectsInvalidApiBaseUrl(string url)
	{
		var ex = Assert.Throws<ArgumentException>(() =>
			PollerOptions.Resolve(Env([.. Required(), ("API_BASE_URL", url)]))
		);

		Assert.Contains("API_BASE_URL", ex.Message);
	}

	[Fact]
	public void Resolve_TreatsBlankValuesAsMissing()
	{
		var vars = Required().Where(v => v.Item1 != "QUEUE_ID").ToList();
		vars.Add(("QUEUE_ID", "   "));

		var ex = Assert.Throws<ArgumentException>(() => PollerOptions.Resolve(Env([.. vars])));

		Assert.Contains("QUEUE_ID is not set", ex.Message);
	}

	[Fact]
	public void Resolve_RejectsShortSecret()
	{
		var vars = Required().Where(v => v.Item1 != "UploadWebhook__Secret").ToList();
		vars.Add(("UploadWebhook__Secret", "short"));

		Assert.Throws<ArgumentException>(() => PollerOptions.Resolve(Env([.. vars])));
	}

	[Theory]
	[InlineData("BATCH_SIZE", "0")]
	[InlineData("BATCH_SIZE", "101")]
	[InlineData("BATCH_SIZE", "abc")]
	[InlineData("POLL_INTERVAL_SECONDS", "-1")]
	public void Resolve_RejectsOutOfRangeNumbers(string name, string value)
	{
		Assert.Throws<ArgumentException>(() =>
			PollerOptions.Resolve(Env([.. Required(), (name, value)]))
		);
	}
}
