namespace MariaDigital.Infrastructure.Options
{
	public sealed class UploadWebhookOptions
	{
		public const string SectionName = "UploadWebhook";
		public const int MinSecretLength = 32;

		/// <summary>Shared with the Cloudflare Worker that signs upload confirmations.</summary>
		public string Secret { get; init; } = string.Empty;

		public static bool IsComplete(UploadWebhookOptions options) =>
			options.Secret.Length >= MinSecretLength;
	}
}
