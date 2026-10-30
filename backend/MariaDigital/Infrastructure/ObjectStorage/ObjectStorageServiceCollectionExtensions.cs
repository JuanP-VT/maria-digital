using Amazon.Runtime;
using Amazon.S3;
using MariaDigital.Application;
using MariaDigital.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace MariaDigital.Infrastructure.ObjectStorage;

public static class ObjectStorageServiceCollectionExtensions
{
	/// <summary>
	/// Registers Cloudflare R2. Options are validated on first use rather than at startup, so the
	/// app still boots (and tests run) without R2 credentials; credentials come from environment
	/// variables or user-secrets, never appsettings.json.
	/// </summary>
	public static IServiceCollection AddR2ObjectStorage(
		this IServiceCollection services,
		IConfiguration configuration
	)
	{
		services
			.AddOptions<R2Options>()
			.Bind(configuration.GetSection(R2Options.SectionName))
			.Validate(
				R2Options.IsComplete,
				"R2 is not configured (AccountId, keys and BucketName)."
			);

		services.AddSingleton<IAmazonS3>(provider =>
		{
			var options = provider.GetRequiredService<IOptions<R2Options>>().Value;
			return new AmazonS3Client(
				new BasicAWSCredentials(options.AccessKeyId, options.SecretAccessKey),
				new AmazonS3Config
				{
					ServiceURL = $"https://{options.AccountId}.r2.cloudflarestorage.com",
					ForcePathStyle = true,
					AuthenticationRegion = "auto",
					// R2 rejects the optional checksum headers newer SDKs add by default.
					RequestChecksumCalculation = RequestChecksumCalculation.WHEN_REQUIRED,
					ResponseChecksumValidation = ResponseChecksumValidation.WHEN_REQUIRED,
				}
			);
		});
		services.AddScoped<IObjectStorage, R2ObjectStorage>();

		return services;
	}
}
