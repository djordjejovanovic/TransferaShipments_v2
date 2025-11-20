namespace TransferaShipments.ServiceBus.Services;

public interface IServiceBusPublisher
{
    Task PublishDocumentToProcessAsync(int shipmentId, string blobName);
}