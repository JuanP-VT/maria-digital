using System.Text.Json;
using MariaDigital.QueuePoller;

namespace MariaDigital.Tests.QueuePoller;

public sealed class MessageParserTests
{
	private static ParsedMessage Parse(string json)
	{
		using var doc = JsonDocument.Parse(json);
		return MessageParser.Parse(doc.RootElement);
	}

	private static string Event(string action, string key, string size = "123") =>
		$$$"""{"action":"{{{action}}}","bucket":"guadalupe-archive","object":{"key":"{{{key}}}","size":{{{size}}}}}""";

	[Theory]
	[InlineData("PutObject")]
	[InlineData("CopyObject")]
	[InlineData("CompleteMultipartUpload")]
	public void Create_actions_under_the_prefix_are_confirmed(string action)
	{
		var parsed = Parse(Event(action, "submissions/a/registration/b"));

		Assert.Equal(MessageKind.Confirm, parsed.Kind);
		Assert.Equal("submissions/a/registration/b", parsed.Key);
		Assert.Equal(123, parsed.Size);
	}

	[Fact]
	public void Delete_actions_are_ignored()
	{
		Assert.Equal(MessageKind.Ignore, Parse(Event("DeleteObject", "submissions/a/b/c")).Kind);
	}

	[Fact]
	public void Keys_outside_the_prefix_are_ignored()
	{
		Assert.Equal(MessageKind.Ignore, Parse(Event("PutObject", "other/a")).Kind);
	}

	[Theory]
	[InlineData("[]")]
	[InlineData("\"text\"")]
	[InlineData("""{"object":{"key":"submissions/a","size":1}}""")]
	[InlineData("""{"action":1,"object":{"key":"submissions/a","size":1}}""")]
	[InlineData("""{"action":"PutObject"}""")]
	[InlineData("""{"action":"PutObject","object":{"key":"","size":1}}""")]
	[InlineData("""{"action":"PutObject","object":{"key":"submissions/a"}}""")]
	[InlineData("""{"action":"PutObject","object":{"key":"submissions/a","size":-1}}""")]
	[InlineData("""{"action":"PutObject","object":{"key":"submissions/a","size":1.5}}""")]
	[InlineData("""{"action":"PutObject","object":{"key":"submissions/a","size":"1"}}""")]
	public void Malformed_bodies_are_invalid(string json)
	{
		Assert.Equal(MessageKind.Invalid, Parse(json).Kind);
	}
}
