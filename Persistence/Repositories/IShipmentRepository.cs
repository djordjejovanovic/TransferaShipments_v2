using TransferaShipments.Domain.Entities;

namespace TransferaShipments.Persistence.Repositories;

public interface IShipmentRepository
{
    Task<Shipment> AddAsync(Shipment shipment);
    Task<Shipment?> GetByIdAsync(int id);
    Task<IEnumerable<Shipment>> GetAllAsync();
    Task UpdateAsync(Shipment shipment);
    Task SaveChangesAsync();
}