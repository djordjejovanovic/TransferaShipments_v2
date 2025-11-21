using MediatR;
using TransferaShipments.Domain.Entities;
using TransferaShipments.Core.Repositories;

namespace AppServices.UseCases
{
    public record GetAllShipmentsRequest(int Page, int PageSize) : IRequest<GetAllShipmentsResponse>;

    public record GetAllShipmentsResponse(IEnumerable<Shipment> Shipments, int TotalCount);

    public class GetAllShipmentsUseCase : IRequestHandler<GetAllShipmentsRequest, GetAllShipmentsResponse>
    {
        private readonly IShipmentRepository _shipmentRepository;

        public GetAllShipmentsUseCase(IShipmentRepository shipmentRepository)
        {
            _shipmentRepository = shipmentRepository;
        }

        public async Task<GetAllShipmentsResponse> Handle(GetAllShipmentsRequest request, CancellationToken cancellationToken)
        {
            var shipments = await _shipmentRepository.GetAllAsync(request.Page, request.PageSize);
            var total = await _shipmentRepository.GetCountAsync();
            return new GetAllShipmentsResponse(shipments, total);
        }
    }
}
