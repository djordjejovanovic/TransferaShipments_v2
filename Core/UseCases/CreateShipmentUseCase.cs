using MediatR;
using TransferaShipments.Domain.Entities;
using TransferaShipments.Domain.Enums;
using AppServices.Contracts.Repositories;

namespace AppServices.UseCases
{
    public record CreateShipmentRequest(string ReferenceNumber, string Sender, string Recipient) : IRequest<CreateShipmentResponse>;

    public record CreateShipmentResponse(bool Success, int? Id = null, string? ErrorMessage = null);

    public class CreateShipmentUseCase : IRequestHandler<CreateShipmentRequest, CreateShipmentResponse>
    {
        private readonly IShipmentRepository _shipmentRepository;

        public CreateShipmentUseCase(IShipmentRepository shipmentRepository)
        {
            _shipmentRepository = shipmentRepository;
        }

        public async Task<CreateShipmentResponse> Handle(CreateShipmentRequest request, CancellationToken cancellationToken)
        {
            var existingShipment = await _shipmentRepository.GetByReferenceNumberAsync(request.ReferenceNumber, cancellationToken);
            
            if (existingShipment != null)
            {
                return new CreateShipmentResponse(
                    Success: false,
                    Id: null,
                    ErrorMessage: $"Shipment with ReferenceNumber '{request.ReferenceNumber}' already exists."
                );
            }

            var shipment = new Shipment
            {
                ReferenceNumber = request.ReferenceNumber,
                Sender = request.Sender,
                Recipient = request.Recipient,
                CreatedAt = DateTime.UtcNow,
                Status = ShipmentStatus.Created
            };

            var result = await _shipmentRepository.AddAsync(shipment, cancellationToken);

            return new CreateShipmentResponse(Success: true, Id: result.Id);
        }
    }
}