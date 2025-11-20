using TransferaShipments.Domain.Entities;
using TransferaShipments.Core.DTOs;

namespace TransferaShipments.Core.Services;

public interface IShipmentService
{
    Task<Shipment> CreateAsync(ShipmentCreateDto dto);
    Task<Shipment?> GetByIdAsync(int id);
    Task<IEnumerable<Shipment>> GetAllAsync();
    Task AddDocumentAsync(int shipmentId, string blobName, string blobUrl);
    Task MarkProcessedAsync(int shipmentId);
}