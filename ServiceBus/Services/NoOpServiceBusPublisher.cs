using Microsoft.Extensions.Logging;
using AppServices.Contracts.Messaging;

namespace TransferaShipments.ServiceBus.Services;

public class NoOpServiceBusPublisher : IServiceBusPublisher
{
    private readonly ILogger<NoOpServiceBusPublisher> _logger;
    public NoOpServiceBusPublisher(ILogger<NoOpServiceBusPublisher> logger) => _logger = logger;

    public Task PublishDocumentToProcessAsync(int shipmentId, string blobName)
    {
        _logger.LogInformation("[NoOpServiceBusPublisher] Would publish ShipmentId={ShipmentId}, BlobName={BlobName}", shipmentId, blobName);
        return Task.CompletedTask;
    }
}