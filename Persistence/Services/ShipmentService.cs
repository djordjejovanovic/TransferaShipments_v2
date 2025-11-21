using TransferaShipments.Domain.Entities;
using TransferaShipments.Domain.Enums;
using TransferaShipments.Core.Repositories;
using TransferaShipments.Core.Services;
using TransferaShipments.Core.DTOs;

namespace TransferaShipments.Persistence.Services;

public class ShipmentService : IShipmentService
{
    private readonly IShipmentRepository _repo;

    public ShipmentService(IShipmentRepository repo)
    {
        _repo = repo;
    }

    public async Task<Shipment> CreateAsync(ShipmentCreateDto dto)
    {
        var shipment = new Shipment
        {
            ReferenceNumber = dto.ReferenceNumber,
            Sender = dto.Sender,
            Recipient = dto.Recipient,
            CreatedAt = DateTime.UtcNow,
            Status = ShipmentStatus.Created
        };
        return await _repo.AddAsync(shipment);
    }

    public async Task<IEnumerable<Shipment>> GetAllAsync() => await _repo.GetAllAsync();

    public async Task<Shipment?> GetByIdAsync(int id) => await _repo.GetByIdAsync(id);

    public async Task AddDocumentAsync(int shipmentId, string blobName, string blobUrl)
    {
        var shipment = await _repo.GetByIdAsync(shipmentId);

        if (shipment == null)
        {
            throw new InvalidOperationException("Shipment not found");
        }

        shipment.LastDocumentBlobName = blobName;
        shipment.LastDocumentUrl = blobUrl;
        shipment.Status = ShipmentStatus.DocumentUploaded;

        await _repo.UpdateAsync(shipment);
    }

    public async Task MarkProcessedAsync(int shipmentId)
    {
        var shipment = await _repo.GetByIdAsync(shipmentId);

        if (shipment == null)
        {
            throw new InvalidOperationException("Shipment not found");
        }

        shipment.Status = ShipmentStatus.Processed;

        await _repo.UpdateAsync(shipment);
    }
}