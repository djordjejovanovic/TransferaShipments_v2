namespace TransferaShipments.BlobStorage.Services;

public interface IBlobService
{
    //prebaci interface u appServices projekat
    // AppServices i use case unuar njega je orkestrator svega

    /// <summary>
    /// Uploads stream to container and returns accessible URL (or blob name if not available).
    /// </summary>
    Task<string> UploadAsync(string containerName, string blobName, Stream data, string contentType);
    Task<Stream> DownloadAsync(string containerName, string blobName);
}