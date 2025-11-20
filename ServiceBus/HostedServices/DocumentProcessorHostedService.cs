using System.Text.Json;
using Azure.Messaging.ServiceBus;
using TransferaShipments_v2.BlobStorage.Services;
using TransferaShipments_v2.Persistence.Repositories;
using TransferaShipments_v2.Core.Services;

namespace TransferaShipments_v2.ServiceBus.HostedServices;

public class DocumentProcessorHostedService : IHostedService, IAsyncDisposable
{
    private readonly IConfiguration _configuration;
    private readonly IBlobService _blobService;
    private readonly IShipmentService _shipmentService;
    private ServiceBusClient? _client;
    private ServiceBusProcessor? _processor;

    public DocumentProcessorHostedService(IConfiguration configuration, IBlobService blobService, IShipmentService shipmentService)
    {
        _configuration = configuration;
        _blobService = blobService;
        _shipmentService = shipmentService;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var conn = _configuration.GetConnectionString("ServiceBus");
        var queue = _configuration["Azure:ServiceBusQueueName"] ?? "documents-toprocess";
        _client = new ServiceBusClient(conn);

        var options = new ServiceBusProcessorOptions
        {
            AutoCompleteMessages = false,
            MaxConcurrentCalls = 2
        };

        _processor = _client.CreateProcessor(queue, options);
        _processor.ProcessMessageAsync += MessageHandler;
        _processor.ProcessErrorAsync += ErrorHandler;

        return _processor.StartProcessingAsync(cancellationToken);
    }

    private async Task MessageHandler(ProcessMessageEventArgs args)
    {
        var body = args.Message.Body.ToString();
        try
        {
            var doc = JsonSerializer.Deserialize<DocumentMessage>(body);
            if (doc == null) throw new InvalidOperationException("Invalid message");

            var container = _configuration["Azure:BlobContainerName"] ?? "shipments-documents";
            var stream = await _blobService.DownloadAsync(container, doc.BlobName);

            // Simulate processing
            await Task.Delay(TimeSpan.FromSeconds(2));

            // Update shipment status
            await _shipmentService.MarkProcessedAsync(doc.ShipmentId);

            await args.CompleteMessageAsync(args.Message);
        }
        catch (Exception ex)
        {
            // log error (for now write to console); abandon or dead-letter depending on policy
            Console.WriteLine($"Error processing message: {ex.Message}");
            await args.AbandonMessageAsync(args.Message);
        }
    }

    private Task ErrorHandler(ProcessErrorEventArgs args)
    {
        Console.WriteLine($"ServiceBus Error: {args.Exception}");
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_processor != null)
            await _processor.StopProcessingAsync(cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        if (_processor != null)
        {
            await _processor.DisposeAsync();
        }

        if (_client != null)
        {
            await _client.DisposeAsync();
        }
    }

    private record DocumentMessage(int ShipmentId, string BlobName);
}