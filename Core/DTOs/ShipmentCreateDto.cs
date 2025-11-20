using System.ComponentModel.DataAnnotations;

namespace TransferaShipments.Core.DTOs;

public class ShipmentCreateDto
{
    [Required]
    public string ReferenceNumber { get; set; } = null!;
    [Required]
    public string Sender { get; set; } = null!;
    [Required]
    public string Recipient { get; set; } = null!;
}