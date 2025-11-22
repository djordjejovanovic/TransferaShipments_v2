using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using AppServices.Contracts.Storage;
using AppServices.Contracts.Repositories;
using TransferaShipments.Domain.Enums;

namespace TransferaShipments.ServiceBus.HostedServices;

public class DocumentProcessorHostedService : IHostedService, IAsyncDisposable
{
    private readonly IConfiguration _configuration;
    private readonly IBlobService _blobService;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DocumentProcessorHostedService> _logger;
    private ServiceBusClient? _client;
    private ServiceBusProcessor? _processor;

    public DocumentProcessorHostedService(IConfiguration configuration, IBlobService blobService, IServiceProvider serviceProvider, ILogger<DocumentProcessorHostedService> logger)
    {
        _configuration = configuration;
        _blobService = blobService;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var conn = _configuration.GetConnectionString("ServiceBus");

        if (string.IsNullOrEmpty(conn))
        {
            _logger.LogInformation("ServiceBus connection string not configured - DocumentProcessorHostedService will not start.");
            return Task.CompletedTask;
        }

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

        _logger.LogInformation("Starting ServiceBus processor for queue {Queue}", queue);

        return _processor.StartProcessingAsync(cancellationToken);
    }

    private async Task MessageHandler(ProcessMessageEventArgs args)
    {
        if (string.IsNullOrEmpty(args?.Message?.Body?.ToString()))
        {
            throw new InvalidOperationException("Message body is null");
        }

        var body = args.Message.Body.ToString();
        DocumentMessage? doc = null;

        try
        {

            doc = JsonSerializer.Deserialize<DocumentMessage>(body);

            if (doc == null)
            {
                throw new InvalidOperationException("Invalid message payload");
            }

            var container = _configuration["Azure:BlobContainerName"] ?? "shipments-documents";

            // Download blob
            var stream = await _blobService.DownloadAsync(container, doc.BlobName);

            // Simulate processing
            await Task.Delay(TimeSpan.FromSeconds(2));

            // Create scope for scoped services (Repository, DbContext)
            using var scope = _serviceProvider.CreateScope();

            var shipmentRepository = scope.ServiceProvider.GetRequiredService<IShipmentRepository>();

            var shipment = await shipmentRepository.GetByIdAsync(doc.ShipmentId);
            if (shipment != null)
            {
                shipment.Status = ShipmentStatus.Processed;
                await shipmentRepository.UpdateAsync(shipment);
            }

            await args.CompleteMessageAsync(args.Message);
            _logger.LogInformation("Processed message for ShipmentId={ShipmentId}, BlobName={BlobName}", doc.ShipmentId, doc.BlobName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message: {Body}", body);

            try
            {
                await args.AbandonMessageAsync(args.Message);
            }
            catch (Exception abandonEx)
            {
                _logger.LogError(abandonEx, "Failed to abandon message");
            }
        }
    }

    private Task ErrorHandler(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception, "ServiceBus processor error: {ErrorSource}", args.ErrorSource);
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_processor != null)
        {
            await _processor.StopProcessingAsync(cancellationToken);
            _logger.LogInformation("Stopped ServiceBus processor");
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_processor != null) await _processor.DisposeAsync();
        if (_client != null) await _client.DisposeAsync();
    }

    private record DocumentMessage(int ShipmentId, string BlobName);
}