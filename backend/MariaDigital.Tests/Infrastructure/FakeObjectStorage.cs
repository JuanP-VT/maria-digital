using MariaDigital.Application;
using MariaDigital.Application.Abstractions.ObjectStorage;

namespace MariaDigital.Tests.Infrastructure;

/// <summary>Records presign requests; tests never reach R2.</summary>
public sealed class FakeObjectStorage : IObjectStorage
{
	public sealed record PresignCall(
		string Key,
		string ContentType,
		long SizeBytes,
		TimeSpan ExpiresIn
	);

	public List<PresignCall> PresignCalls { get; } = [];

	public Task<string> GetPresignedPutAsync(
		string key,
		string contentType,
		long sizeBytes,
		TimeSpan expiresIn,
		CancellationToken cancellationToken = default
	)
	{
		PresignCalls.Add(new PresignCall(key, contentType, sizeBytes, expiresIn));
		return Task.FromResult($"https://storage.test/{key}?signed");
	}

	public Task<StoredObject> UploadAsync(
		Stream content,
		string key,
		string contentType,
		CancellationToken cancellationToken = default
	) => throw new NotImplementedException();

	public Task<Stream> DownloadAsync(string key, CancellationToken cancellationToken = default) =>
		throw new NotImplementedException();

	public Task<PresignedUpload> GetUploadUrlAsync(
		string key,
		string contentType,
		TimeSpan expiresIn,
		CancellationToken cancellationToken = default
	) => throw new NotImplementedException();

	public Task DeleteAsync(string key, CancellationToken cancellationToken = default) =>
		throw new NotImplementedException();

	public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default) =>
		throw new NotImplementedException();
}
