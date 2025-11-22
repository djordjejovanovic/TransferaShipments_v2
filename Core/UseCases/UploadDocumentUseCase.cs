using MediatR;
using AppServices.Contracts.Repositories;
using AppServices.Contracts.Storage;
using AppServices.Contracts.Messaging;

namespace AppServices.UseCases
{
    public record UploadDocumentRequest(
        int ShipmentId, 
        Stream FileStream,
        string FileName,
        string ContentType,
        string ContainerName) : IRequest<UploadDocumentResponse>;

    public record UploadDocumentResponse(bool Success, string? BlobName, string? BlobUrl, string? ErrorMessage);

    public class UploadDocumentUseCase : IRequestHandler<UploadDocumentRequest, UploadDocumentResponse>
    {
        private readonly IShipmentRepository _shipmentRepository;
        private readonly IBlobService _blobService;
        private readonly IServiceBusPublisher _serviceBusPublisher;

        public UploadDocumentUseCase(
            IShipmentRepository shipmentRepository,
            IBlobService blobService,
            IServiceBusPublisher serviceBusPublisher)
        {
            _shipmentRepository = shipmentRepository;
            _blobService = blobService;
            _serviceBusPublisher = serviceBusPublisher;
        }

        public async Task<UploadDocumentResponse> Handle(UploadDocumentRequest request, CancellationToken cancellationToken)
        {
            // Validate stream
            if (request.FileStream == null)
            {
                return new UploadDocumentResponse(false, null, null, "File is required");
            }

            // Check stream length if possible (some streams don't support Length property)
            if (request.FileStream.CanSeek && request.FileStream.Length == 0)
            {
                return new UploadDocumentResponse(false, null, null, "File is empty");
            }

            // Check if shipment exists
            var shipment = await _shipmentRepository.GetByIdAsync(request.ShipmentId, cancellationToken);
            if (shipment == null)
            {
                return new UploadDocumentResponse(false, null, null, "Shipment not found");
            }

            // Sanitize file name to prevent path traversal attacks
            var sanitizedFileName = Path.GetFileName(request.FileName);
            if (string.IsNullOrWhiteSpace(sanitizedFileName))
            {
                return new UploadDocumentResponse(false, null, null, "Invalid file name");
            }

            // Generate blob name and upload to storage
            var blobName = $"{request.ShipmentId}/{Guid.NewGuid()}_{sanitizedFileName}";
            
            var blobUrl = await _blobService.UploadAsync(
                request.ContainerName, 
                blobName, 
                request.FileStream, 
                request.ContentType,
                cancellationToken);

            // Update shipment metadata and status
            shipment.LastDocumentBlobName = blobName;
            shipment.LastDocumentUrl = blobUrl;
            shipment.Status = TransferaShipments.Domain.Enums.ShipmentStatus.DocumentUploaded;

            await _shipmentRepository.UpdateAsync(shipment, cancellationToken);

            // Publish message to service bus for further processing
            await _serviceBusPublisher.PublishDocumentToProcessAsync(request.ShipmentId, blobName, cancellationToken);

            return new UploadDocumentResponse(true, blobName, blobUrl, null);
        }
    }
}
