using MariaDigital.QueuePoller;

namespace MariaDigital.Tests.QueuePoller;

public sealed class RetryPolicyTests
{
	[Theory]
	[InlineData(1, 5)]
	[InlineData(2, 15)]
	[InlineData(3, 45)]
	[InlineData(4, 135)]
	public void Without_jitter_the_delay_triples_each_attempt(int attempts, int expected)
	{
		Assert.Equal(expected, RetryPolicy.DelaySeconds(attempts, () => 1.0));
	}

	[Fact]
	public void The_delay_is_capped()
	{
		Assert.Equal(600, RetryPolicy.DelaySeconds(20, () => 1.0));
	}

	[Fact]
	public void Jitter_scales_between_half_and_full()
	{
		Assert.Equal(23, RetryPolicy.DelaySeconds(3, () => 0.0));
		Assert.Equal(45, RetryPolicy.DelaySeconds(3, () => 1.0));
	}

	[Theory]
	[InlineData(0)]
	[InlineData(-3)]
	public void Non_positive_attempts_use_the_first_step(int attempts)
	{
		Assert.Equal(5, RetryPolicy.DelaySeconds(attempts, () => 1.0));
	}

	[Fact]
	public void The_delay_is_never_below_one_second()
	{
		Assert.True(RetryPolicy.DelaySeconds(1, () => 0.0) >= 1);
	}
}
