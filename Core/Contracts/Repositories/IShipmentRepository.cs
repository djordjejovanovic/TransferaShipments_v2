using TransferaShipments.Domain.Entities;

namespace AppServices.Contracts.Repositories;

/// <summary>
/// Repository interface for Shipment entity operations
/// </summary>
public interface IShipmentRepository
{
    Task<Shipment> AddAsync(Shipment shipment, CancellationToken cancellationToken = default);
    Task<Shipment?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Shipment>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Shipment>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> GetCountAsync(CancellationToken cancellationToken = default);
    Task UpdateAsync(Shipment shipment, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
