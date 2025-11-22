namespace AppServices.Contracts.Messaging;

/// <summary>
/// Interface for Service Bus message publishing
/// </summary>
public interface IServiceBusPublisher
{
    Task PublishDocumentToProcessAsync(int shipmentId, string blobName);
}
