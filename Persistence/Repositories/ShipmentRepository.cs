using Microsoft.EntityFrameworkCore;
using TransferaShipments.Domain.Entities;
using TransferaShipments.Persistence.Data;

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

    public async Task<IEnumerable<Shipment>> GetAllAsync()
    {
        return await _db.Shipments.OrderByDescending(s => s.CreatedAt).ToListAsync();
    }

    public async Task UpdateAsync(Shipment shipment)
    {
        _db.Shipments.Update(shipment);
        await _db.SaveChangesAsync();
    }

    public Task SaveChangesAsync() => _db.SaveChangesAsync();
}