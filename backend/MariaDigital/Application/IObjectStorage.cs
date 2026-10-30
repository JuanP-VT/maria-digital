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
		/// <summary>
		/// URL that accepts exactly one PUT of <paramref name="sizeBytes"/> bytes with this content type.
		/// The signature covers both, so storage rejects any other type or length.
		/// </summary>
		Task<string> GetPresignedPutAsync(
			string key,
			string contentType,
			long sizeBytes,
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
