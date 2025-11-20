using TransferaShipments_v2.Domain.Entities;
using TransferaShipments_v2.Core.DTOs;

namespace TransferaShipments_v2.Core.Services;

public interface IShipmentService
{
    Task<Shipment> CreateAsync(ShipmentCreateDto dto);
    Task<Shipment?> GetByIdAsync(int id);
    Task<IEnumerable<Shipment>> GetAllAsync();
    Task AddDocumentAsync(int shipmentId, string blobName, string blobUrl);
    Task MarkProcessedAsync(int shipmentId);
}