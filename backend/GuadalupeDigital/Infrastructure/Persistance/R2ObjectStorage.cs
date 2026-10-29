using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using GuadalupeDigital.Application;
using GuadalupeDigital.Application.Abstractions.ObjectStorage;
using GuadalupeDigital.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace GuadalupeDigital.Infrastructure.ObjectStorage;

public sealed class R2ObjectStorage : IObjectStorage
{
	private readonly IAmazonS3 _s3;
	private readonly R2Options _options;

	public R2ObjectStorage(IAmazonS3 s3, IOptions<R2Options> options)
	{
		_s3 = s3;
		_options = options.Value;
	}

	public Task DeleteAsync(string key, CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}

	public Task<Stream> DownloadAsync(string key, CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}

	public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}

	public Task<PresignedUpload> GetPresignedPutAsync(
		string key,
		long fileSize,
		string contentType,
		TimeSpan expiresIn,
		CancellationToken cancellationToken = default
	)
	{
		throw new NotImplementedException();
	}

	public Task<string> GetPresignedPutAsync(
		string key,
		string contentType,
		TimeSpan expiresIn,
		CancellationToken cancellationToken = default
	)
	{
		var expiresAt = DateTimeOffset.UtcNow.Add(expiresIn);

		var request = new GetPreSignedUrlRequest
		{
			BucketName = _options.BucketName,
			Key = key,
			Verb = HttpVerb.PUT,
			Expires = expiresAt.UtcDateTime,
			ContentType = contentType,
		};

		var url = _s3.GetPreSignedURLAsync(request);

		return url;
	}

	public Task<PresignedUpload> GetUploadUrlAsync(
		string key,
		string contentType,
		TimeSpan expiresIn,
		CancellationToken cancellationToken = default
	)
	{
		throw new NotImplementedException();
	}

	public Task<StoredObject> UploadAsync(
		Stream content,
		string key,
		string contentType,
		CancellationToken cancellationToken = default
	)
	{
		throw new NotImplementedException();
	}
}
