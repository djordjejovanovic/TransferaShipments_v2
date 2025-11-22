using TransferaShipments.Domain.Entities;

namespace AppServices.Contracts.Repositories;

/// <summary>
/// Repository interface for Shipment entity operations
/// </summary>
public interface IShipmentRepository
{
    Task<Shipment> AddAsync(Shipment shipment);
    Task<Shipment?> GetByIdAsync(int id);
    Task<IEnumerable<Shipment>> GetAllAsync();
    Task<IEnumerable<Shipment>> GetAllAsync(int page, int pageSize);
    Task<int> GetCountAsync();
    Task UpdateAsync(Shipment shipment);
    Task SaveChangesAsync();
}
