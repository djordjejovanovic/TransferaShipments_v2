using TransferaShipments_v2.Domain.Enums;

namespace TransferaShipments_v2.Domain.Entities;

public class Shipment
{
    public int Id { get; set; }
    public string ReferenceNumber { get; set; } = null!;
    public string Sender { get; set; } = null!;
    public string Recipient { get; set; } = null!;
    public ShipmentStatus Status { get; set; } = ShipmentStatus.Created;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Optional: store last document info
    public string? LastDocumentBlobName { get; set; }
    public string? LastDocumentUrl { get; set; }
}