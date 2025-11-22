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

    private BlobServiceClient CreateClient(string conn)
    {
        // Support the simple development flag (recommended for local Azurite)
        if (!string.IsNullOrEmpty(conn) && conn.Trim().Equals("UseDevelopmentStorage=true", StringComparison.OrdinalIgnoreCase))
        {
            // Construct the full Azurite connection string (Azurite uses the well-known devstoreaccount1)
            // This value works for default Azurite setup (when started on localhost:10000).
            var azuriteConn =
                "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ5OUb9Q/2qjmreU+oiMTT+j6HjmQSlAvHBSoD6+MdVfn+BOvyFQA9QvwjkHQAUAicK5xCdvQ==;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;";
            return new BlobServiceClient(azuriteConn);
        }

        return new BlobServiceClient(conn);
    }

    private BlobContainerClient GetContainer(string container)
    {
        var conn = _configuration.GetConnectionString("AzureBlob");
        var client = CreateClient(conn ?? "");
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