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
            var page = request.Page;
            if(request.Page <= 0)
            {
                page = 1;
            }
            
            var pageSize = Math.Min(request.PageSize, 100);
            if(request.PageSize <= 0)
            {
                pageSize = 20;
            }   
            
            var shipments = await _shipmentRepository.GetAllAsync(page, pageSize, cancellationToken);

            var total = await _shipmentRepository.GetCountAsync(cancellationToken);
            
            return new PaginatedResponse<Shipment>(shipments, total, page, pageSize);
        }
    }
}
