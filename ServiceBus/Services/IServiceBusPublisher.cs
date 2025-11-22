namespace TransferaShipments.ServiceBus.Services;

//isto bi trebalo da zavrsi u app services projekatu. Interface posle se poziva iz use case-eva a implementacija je ovde i o je to.
public interface IServiceBusPublisher
{
    Task PublishDocumentToProcessAsync(int shipmentId, string blobName);
}