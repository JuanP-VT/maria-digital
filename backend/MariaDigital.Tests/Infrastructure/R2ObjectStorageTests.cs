using Amazon.Runtime;
using Amazon.S3;
using MariaDigital.Infrastructure.ObjectStorage;
using MariaDigital.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace MariaDigital.Tests.Infrastructure;

// Presigning is computed locally (no network), so the real AWS client runs with dummy credentials.
public sealed class R2ObjectStorageTests : IDisposable
{
	private readonly AmazonS3Client _s3 = new(
		new BasicAWSCredentials("test-key", "test-secret"),
		new AmazonS3Config
		{
			ServiceURL = "https://acct.r2.cloudflarestorage.com",
			ForcePathStyle = true,
			AuthenticationRegion = "auto",
			RequestChecksumCalculation = RequestChecksumCalculation.WHEN_REQUIRED,
		}
	);

	public void Dispose() => _s3.Dispose();

	private R2ObjectStorage Storage() =>
		new(
			_s3,
			Microsoft.Extensions.Options.Options.Create(
				new R2Options
				{
					AccountId = "acct",
					AccessKeyId = "test-key",
					SecretAccessKey = "test-secret",
					BucketName = "bucket",
				}
			)
		);

	[Fact]
	public async Task Presigned_put_points_at_the_bucket_key_and_expires_soon()
	{
		var url = await Storage()
			.GetPresignedPutAsync("a/b", "image/jpeg", 123, TimeSpan.FromMinutes(5));

		var uri = new Uri(url);
		Assert.Equal("acct.r2.cloudflarestorage.com", uri.Host);
		Assert.Equal("/bucket/a/b", uri.AbsolutePath);
		Assert.Contains("X-Amz-Expires=300", url);
	}

	[Fact]
	public async Task Presigned_put_signs_content_type_and_content_length()
	{
		var url = await Storage()
			.GetPresignedPutAsync("a/b", "image/jpeg", 123, TimeSpan.FromMinutes(5));

		var signedHeaders = System.Web.HttpUtility.ParseQueryString(new Uri(url).Query)[
			"X-Amz-SignedHeaders"
		]!.Split(';');
		Assert.Contains("content-type", signedHeaders);
		Assert.Contains("content-length", signedHeaders);
	}
}
