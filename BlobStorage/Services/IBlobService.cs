namespace TransferaShipments_v2.BlobStorage.Services;

public interface IBlobService
{
    /// <summary>
    /// Uploads stream to container and returns accessible URL (or blob name if not available).
    /// </summary>
    Task<string> UploadAsync(string containerName, string blobName, Stream data, string contentType);
    Task<Stream> DownloadAsync(string containerName, string blobName);
}