using TransferaShipments.Domain.Entities;

namespace TransferaShipments.Core.Repositories;

public interface IShipmentRepository
{
    Task<Shipment> AddAsync(Shipment shipment);
    Task<Shipment?> GetByIdAsync(int id);
    // Original method (kept for backward compatibility)
    Task<IEnumerable<Shipment>> GetAllAsync();
    Task<IEnumerable<Shipment>> GetAllAsync(int page, int pageSize);
    Task<int> GetCountAsync();
    Task UpdateAsync(Shipment shipment);
    Task SaveChangesAsync();
}
