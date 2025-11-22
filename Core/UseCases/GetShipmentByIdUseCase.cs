using MediatR;
using TransferaShipments.Domain.Entities;
using AppServices.Contracts.Repositories;

namespace AppServices.UseCases
{
    public record GetShipmentByIdRequest(int Id) : IRequest<GetShipmentByIdResponse>;

    public record GetShipmentByIdResponse(Shipment? Shipment);

    public class GetShipmentByIdUseCase : IRequestHandler<GetShipmentByIdRequest, GetShipmentByIdResponse>
    {
        private readonly IShipmentRepository _shipmentRepository;

        public GetShipmentByIdUseCase(IShipmentRepository shipmentRepository)
        {
            _shipmentRepository = shipmentRepository;
        }

        public async Task<GetShipmentByIdResponse> Handle(GetShipmentByIdRequest request, CancellationToken cancellationToken)
        {
            var shipment = await _shipmentRepository.GetByIdAsync(request.Id);
            return new GetShipmentByIdResponse(shipment);
        }
    }
}
