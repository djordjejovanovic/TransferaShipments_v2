namespace TransferaShipments_v2.ServiceBus.Services;

public interface IServiceBusPublisher
{
    Task PublishDocumentToProcessAsync(int shipmentId, string blobName);
}