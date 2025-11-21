using Microsoft.EntityFrameworkCore;
using TransferaShipments.Domain.Entities;
using TransferaShipments.Persistence.Data;
using TransferaShipments.Core.Repositories;

namespace TransferaShipments.Persistence.Repositories;

public class ShipmentRepository : IShipmentRepository
{
    private readonly AppDbContext _db;
    public ShipmentRepository(AppDbContext db) => _db = db;

    public async Task<Shipment> AddAsync(Shipment shipment)
    {
        var ent = (await _db.Shipments.AddAsync(shipment)).Entity;

        await _db.SaveChangesAsync();
  
        return ent;
    }

    public async Task<Shipment?> GetByIdAsync(int id)
    {
        return await _db.Shipments.FirstOrDefaultAsync(s => s.Id == id);
    }

    // Original method (returns all) - kept for backward compatibility
    public async Task<IEnumerable<Shipment>> GetAllAsync()
    {
        return await _db.Shipments.OrderByDescending(s => s.CreatedAt).ToListAsync();
    }

    public async Task<IEnumerable<Shipment>> GetAllAsync(int page, int pageSize)
    {
        return await _db.Shipments
            .AsNoTracking()
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetCountAsync()
    {
        return await _db.Shipments.CountAsync();
    }

    public async Task UpdateAsync(Shipment shipment)
    {
        _db.Shipments.Update(shipment);
        
        await _db.SaveChangesAsync();
    }

    public Task SaveChangesAsync() => _db.SaveChangesAsync();
}