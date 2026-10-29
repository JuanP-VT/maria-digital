namespace MariaDigital.Infrastructure.Options
{
	public sealed class R2Options
	{
		public const string SectionName = "R2";
		public required string AccountId { get; init; }
		public required string AccessKeyId { get; init; }
		public required string SecretAccessKey { get; init; }
		public required string BucketName { get; init; }
	}
}
