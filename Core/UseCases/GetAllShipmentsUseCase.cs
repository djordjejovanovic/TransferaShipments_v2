using MediatR;
using TransferaShipments.Domain.Entities;
using AppServices.Contracts.Repositories;
using AppServices.Common.Models;

namespace AppServices.UseCases
{
    public record GetAllShipmentsRequest(int Page, int PageSize) : IRequest<PaginatedResponse<Shipment>>;

    public class GetAllShipmentsUseCase : IRequestHandler<GetAllShipmentsRequest, PaginatedResponse<Shipment>>
    {
        private readonly IShipmentRepository _shipmentRepository;

        public GetAllShipmentsUseCase(IShipmentRepository shipmentRepository)
        {
            _shipmentRepository = shipmentRepository;
        }

        public async Task<PaginatedResponse<Shipment>> Handle(GetAllShipmentsRequest request, CancellationToken cancellationToken)
        {
            // Validate and normalize pagination parameters
            var page = request.Page <= 0 ? 1 : request.Page;
            var pageSize = request.PageSize <= 0 ? 20 : Math.Min(request.PageSize, 100);

            var shipments = await _shipmentRepository.GetAllAsync(page, pageSize);
            var total = await _shipmentRepository.GetCountAsync();
            
            return new PaginatedResponse<Shipment>(shipments, total, page, pageSize);
        }
    }
}
