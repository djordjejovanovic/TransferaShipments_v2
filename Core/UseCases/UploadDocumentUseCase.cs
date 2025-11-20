using MediatR;
using TransferaShipments.Core.Repositories;

namespace AppServices.UseCases
{
    public record UploadDocumentRequest(int ShipmentId, string BlobName, string BlobUrl) : IRequest<UploadDocumentResponse>;

    public record UploadDocumentResponse(bool Success);

    public class UploadDocumentUseCase : IRequestHandler<UploadDocumentRequest, UploadDocumentResponse>
    {
        private readonly IShipmentRepository _shipmentRepository;

        public UploadDocumentUseCase(IShipmentRepository shipmentRepository)
        {
            _shipmentRepository = shipmentRepository;
        }

        public async Task<UploadDocumentResponse> Handle(UploadDocumentRequest request, CancellationToken cancellationToken)
        {
            var shipment = await _shipmentRepository.GetByIdAsync(request.ShipmentId);
            if (shipment == null)
            {
                return new UploadDocumentResponse(false);
            }

            shipment.LastDocumentBlobName = request.BlobName;
            shipment.LastDocumentUrl = request.BlobUrl;
            shipment.Status = TransferaShipments.Domain.Enums.ShipmentStatus.DocumentUploaded;

            await _shipmentRepository.UpdateAsync(shipment);

            return new UploadDocumentResponse(true);
        }
    }
}
