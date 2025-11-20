using TransferaShipments_v2.Domain.Entities;

namespace TransferaShipments_v2.Persistence.Repositories;

public interface IShipmentRepository
{
    Task<Shipment> AddAsync(Shipment shipment);
    Task<Shipment?> GetByIdAsync(int id);
    Task<IEnumerable<Shipment>> GetAllAsync();
    Task UpdateAsync(Shipment shipment);
    Task SaveChangesAsync();
}