using MediatR;
using TransferaShipments_v2.Domain.Entities;
using TransferaShipments_v2.Domain.Enums;
using TransferaShipments_v2.Persistence.Repositories;

namespace AppServices.UseCases
{
    public record CreateShipmentRequest(string ReferenceNumber, string Sender, string Recipient) : IRequest<CreateShipmentResponse>;

    public record CreateShipmentResponse(int Id);

    public class CreateShipmentUseCase :
        IRequestHandler<CreateShipmentRequest, CreateShipmentResponse>
    {
        private readonly IShipmentRepository _shipmentRepository;

        public CreateShipmentUseCase(IShipmentRepository shipmentRepository)
        {
            _shipmentRepository = shipmentRepository;
        }

        public async Task<CreateShipmentResponse> Handle(CreateShipmentRequest request, CancellationToken cancellationToken)
        {

            var shipment = new Shipment
            {
                ReferenceNumber = request.ReferenceNumber,
                Sender = request.Sender,
                Recipient = request.Recipient,
                CreatedAt = DateTime.UtcNow,
                Status = ShipmentStatus.Created
            };

            var result = await _shipmentRepository.AddAsync(shipment);

            var response = new CreateShipmentResponse(result.Id);

            return response;
        }
    }
}
