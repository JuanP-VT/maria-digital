using MariaDigital.LocalScanner;

namespace MariaDigital.Tests.LocalScanner;

public class SampleLocatorTests
{
	[Fact]
	public void FindUpwards_FindsFileInAParentDirectory()
	{
		var root = Path.Combine(Path.GetTempPath(), "repo");
		var start = Path.Combine(root, "backend", "app");
		var expected = Path.Combine(root, "Test.jpeg");

		var found = SampleLocator.FindUpwards(start, "Test.jpeg", path => path == expected);

		Assert.Equal(expected, found);
	}

	[Fact]
	public void FindUpwards_PrefersTheClosestDirectory()
	{
		var root = Path.Combine(Path.GetTempPath(), "repo");
		var start = Path.Combine(root, "backend");

		var found = SampleLocator.FindUpwards(start, "Test.jpeg", _ => true);

		Assert.Equal(Path.Combine(start, "Test.jpeg"), found);
	}

	[Fact]
	public void FindUpwards_ReturnsNullWhenMissing()
	{
		var start = Path.Combine(Path.GetTempPath(), "repo", "backend");

		Assert.Null(SampleLocator.FindUpwards(start, "Test.jpeg", _ => false));
	}
}
