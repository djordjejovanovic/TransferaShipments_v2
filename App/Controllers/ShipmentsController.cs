using Microsoft.AspNetCore.Mvc;
using TransferaShipments_v2.Core.DTOs;
using TransferaShipments_v2.Core.Services;
using TransferaShipments_v2.BlobStorage.Services;
using TransferaShipments_v2.ServiceBus.Services;

namespace TransferaShipments_v2.App.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShipmentsController : ControllerBase
{
    private readonly IShipmentService _shipmentService;
    private readonly IBlobService _blobService;
    private readonly IServiceBusPublisher _busPublisher;
    private readonly IConfiguration _configuration;

    public ShipmentsController(
        IShipmentService shipmentService,
        IBlobService blobService,
        IServiceBusPublisher busPublisher,
        IConfiguration configuration)
    {
        _shipmentService = shipmentService;
        _blobService = blobService;
        _busPublisher = busPublisher;
        _configuration = configuration;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ShipmentCreateDto dto)
    {
        var shipment = await _shipmentService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = shipment.Id }, shipment);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _shipmentService.GetAllAsync();
        return Ok(list);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var shipment = await _shipmentService.GetByIdAsync(id);
        if (shipment == null) return NotFound();
        return Ok(shipment);
    }

    [HttpPost("{id:int}/documents")]
    public async Task<IActionResult> UploadDocument(int id, IFormFile file)
    {
        if (file == null || file.Length == 0) return BadRequest("File is required");

        var existing = await _shipmentService.GetByIdAsync(id);
        if (existing == null) return NotFound();

        // Upload to blob
        var container = _configuration["Azure:BlobContainerName"] ?? "shipments-documents";
        var blobName = $"{id}/{Guid.NewGuid()}_{file.FileName}";
        using var stream = file.OpenReadStream();
        var blobUrl = await _blobService.UploadAsync(container, blobName, stream, file.ContentType);

        // Save metadata & update status & send message
        await _shipmentService.AddDocumentAsync(id, blobName, blobUrl);
        await _busPublisher.PublishDocumentToProcessAsync(id, blobName);

        return Accepted(new { blobName, blobUrl });
    }
}