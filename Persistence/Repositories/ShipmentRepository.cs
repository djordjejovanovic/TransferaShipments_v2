using Microsoft.EntityFrameworkCore;
using TransferaShipments.Domain.Entities;
using TransferaShipments.Persistence.Data;
using AppServices.Contracts.Repositories;

namespace TransferaShipments.Persistence.Repositories;

public class ShipmentRepository : IShipmentRepository
{
    private readonly AppDbContext _db;
    public ShipmentRepository(AppDbContext db) => _db = db;

    public async Task<Shipment> AddAsync(Shipment shipment, CancellationToken cancellationToken = default)
    {
        var ent = (await _db.Shipments.AddAsync(shipment, cancellationToken)).Entity;

        await _db.SaveChangesAsync(cancellationToken);
  
        return ent;
    }

    public async Task<Shipment?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _db.Shipments.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<Shipment?> GetByReferenceNumberAsync(string referenceNumber, CancellationToken cancellationToken = default)
    {
        return await _db.Shipments.FirstOrDefaultAsync(s => s.ReferenceNumber == referenceNumber, cancellationToken);
    }

    public async Task<IEnumerable<Shipment>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Shipments.OrderByDescending(s => s.CreatedAt).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Shipment>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _db.Shipments
            .AsNoTracking()
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Shipments.CountAsync(cancellationToken);
    }

    public async Task UpdateAsync(Shipment shipment, CancellationToken cancellationToken = default)
    {
        _db.Shipments.Update(shipment);
        
        await _db.SaveChangesAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) => _db.SaveChangesAsync(cancellationToken);
}