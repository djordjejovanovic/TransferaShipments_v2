using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using AppServices.Contracts.Storage;

namespace TransferaShipments.BlobStorage.Services;

public class BlobService : IBlobService
{
    private readonly IConfiguration _configuration;

    public BlobService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    private BlobContainerClient GetContainer(string container)
    {
        var conn = _configuration.GetConnectionString("AzureBlob");
        var client = new BlobServiceClient(conn);
        var containerClient = client.GetBlobContainerClient(container);
        containerClient.CreateIfNotExists(PublicAccessType.None);
        return containerClient;
    }

    public async Task<string> UploadAsync(string containerName, string blobName, Stream data, string contentType, CancellationToken cancellationToken = default)
    {
        var containerClient = GetContainer(containerName);
        var blobClient = containerClient.GetBlobClient(blobName);
        var headers = new BlobHttpHeaders { ContentType = contentType };

        var uploadOptions = new BlobUploadOptions
        {
            HttpHeaders = headers
        };

        await blobClient.UploadAsync(data, uploadOptions, cancellationToken);
        // Return URI (may be local emulator URI if using Azurite)
        return blobClient.Uri.ToString();
    }

    public async Task<Stream> DownloadAsync(string containerName, string blobName, CancellationToken cancellationToken = default)
    {
        var containerClient = GetContainer(containerName);
        var blobClient = containerClient.GetBlobClient(blobName);
        var ms = new MemoryStream();
        await blobClient.DownloadToAsync(ms, cancellationToken);
        ms.Position = 0;
        return ms;
    }
}