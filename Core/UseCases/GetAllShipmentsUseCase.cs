using MediatR;
using TransferaShipments.Domain.Entities;
using TransferaShipments.Core.Repositories;

namespace AppServices.UseCases
{
    public record GetAllShipmentsRequest() : IRequest<GetAllShipmentsResponse>;

    public record GetAllShipmentsResponse(IEnumerable<Shipment> Shipments);

    public class GetAllShipmentsUseCase : IRequestHandler<GetAllShipmentsRequest, GetAllShipmentsResponse>
    {
        private readonly IShipmentRepository _shipmentRepository;

        public GetAllShipmentsUseCase(IShipmentRepository shipmentRepository)
        {
            _shipmentRepository = shipmentRepository;
        }

        public async Task<GetAllShipmentsResponse> Handle(GetAllShipmentsRequest request, CancellationToken cancellationToken)
        {
            var shipments = await _shipmentRepository.GetAllAsync();
            return new GetAllShipmentsResponse(shipments);
        }
    }
}
