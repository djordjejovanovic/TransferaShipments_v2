using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using AppServices.Contracts.Messaging;

namespace TransferaShipments.ServiceBus.Services;

public class ServiceBusPublisher : IServiceBusPublisher
{
    private readonly IConfiguration _configuration;
    private readonly string _queueName;
    private readonly ServiceBusClient _client;

    public ServiceBusPublisher(IConfiguration configuration)
    {
        _configuration = configuration;
        var conn = _configuration.GetConnectionString("ServiceBus");
        _queueName = _configuration["Azure:ServiceBusQueueName"] ?? "documents-toprocess";
        _client = new ServiceBusClient(conn);
    }

    public async Task PublishDocumentToProcessAsync(int shipmentId, string blobName, CancellationToken cancellationToken = default)
    {
        var sender = _client.CreateSender(_queueName);

        var payload = new
        {
            ShipmentId = shipmentId,
            BlobName = blobName
        };

        var msgBody = JsonSerializer.Serialize(payload);

        var msg = new ServiceBusMessage(msgBody)
        {
            Subject = "DocumentToProcess",
        };

        await sender.SendMessageAsync(msg, cancellationToken);
    }
}