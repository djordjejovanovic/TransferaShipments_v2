using MediatR;
using TransferaShipments.Domain.Entities;
using TransferaShipments.Domain.Enums;

namespace AppServices.UseCases
{
    // ove recorde mozes prebaciti u poseban folder DTOs ako zelis ili boundary moze isto da bude ime foldera. Svejedno je.
    // takodje je ok praksa da stave npr u folder UseCase/CreateShipment/CreateShipmentRequest.cs i  UseCase/CreateShipmentResponse.cs i  UseCase/CreateShipmentUseCase.cs
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