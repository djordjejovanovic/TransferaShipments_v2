namespace AppServices.Contracts.Storage;

/// <summary>
/// Interface for blob storage operations
/// </summary>
public interface IBlobService
{
    /// <summary>
    /// Uploads stream to container and returns accessible URL (or blob name if not available).
    /// </summary>
    Task<string> UploadAsync(string containerName, string blobName, Stream data, string contentType);
    
    /// <summary>
    /// Downloads a blob from the specified container
    /// </summary>
    Task<Stream> DownloadAsync(string containerName, string blobName);
}
