using TransferaShipments.Domain.Entities;

namespace TransferaShipments.Core.Repositories;

public interface IShipmentRepository
{
    Task<Shipment> AddAsync(Shipment shipment);
    Task<Shipment?> GetByIdAsync(int id);
    // originalna metoda (zadržana radi kompatibilnosti)
    Task<IEnumerable<Shipment>> GetAllAsync();
    // nove metode za paginaciju
    Task<IEnumerable<Shipment>> GetAllAsync(int page, int pageSize);
    Task<int> GetCountAsync();
    Task UpdateAsync(Shipment shipment);
    Task SaveChangesAsync();
}
