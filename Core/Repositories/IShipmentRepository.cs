using TransferaShipments.Domain.Entities;

namespace TransferaShipments.Core.Repositories;

// mozes sve interfejse da gruises u jedan folder pa unuar njega da pravis dalje podele
// npr. contracts.repositories, contracts.services, contracts.storage itd.

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
