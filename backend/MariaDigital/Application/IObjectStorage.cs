using MariaDigital.Application.Abstractions.ObjectStorage;

namespace MariaDigital.Application
{
	public interface IObjectStorage
	{
		// Direct Stream
		Task<StoredObject> UploadAsync(
			Stream content,
			string key,
			string contentType,
			CancellationToken cancellationToken = default
		);
		Task<Stream> DownloadAsync(string key, CancellationToken cancellationToken = default);

		// Presigned Url
		Task<string> GetPresignedPutAsync(
			string key,
			string contentType,
			TimeSpan expiresIn,
			CancellationToken cancellationToken = default
		);
		Task<PresignedUpload> GetUploadUrlAsync(
			string key,
			string contentType,
			TimeSpan expiresIn,
			CancellationToken cancellationToken = default
		);

		// Utility
		Task DeleteAsync(string key, CancellationToken cancellationToken = default);
		Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
	}
}
