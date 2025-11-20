using Microsoft.AspNetCore.Mvc;
using TransferaShipments.Core.DTOs;
using TransferaShipments.Core.Services;
using TransferaShipments.BlobStorage.Services;
using TransferaShipments.ServiceBus.Services;
using MediatR;
using AppServices.UseCases;

namespace TransferaShipments.App.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShipmentsController : ControllerBase
{
    private readonly IShipmentService _shipmentService;
    private readonly IBlobService _blobService;
    private readonly IServiceBusPublisher _busPublisher;
    private readonly IConfiguration _configuration;
    private readonly IMediator _mediator;

    public ShipmentsController(
        IShipmentService shipmentService,
        IBlobService blobService,
        IServiceBusPublisher busPublisher,
        IConfiguration configuration,
        IMediator mediator)
    {
        _shipmentService = shipmentService;
        _blobService = blobService;
        _busPublisher = busPublisher;
        _configuration = configuration;
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ShipmentCreateDto dto)
    {
        //TODO : Zameniti sve sa _shipmentService sa drugim servisom koji koristi MediatR
        var request = new CreateShipmentRequest(dto.ReferenceNumber, dto.Sender, dto.Recipient);
        var response = await _mediator.Send(request);

        return CreatedAtAction(nameof(GetById), response);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        //TODO : Koristiti MediatR umesto direktnog poziva servisa
        var list = await _shipmentService.GetAllAsync();
        return Ok(list);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        //TODO : Koristiti MediatR umesto direktnog poziva servisa
        var shipment = await _shipmentService.GetByIdAsync(id);
        if (shipment == null) return NotFound();
        return Ok(shipment);
    }

    [HttpPost("{id:int}/documents")]
    public async Task<IActionResult> UploadDocument(int id, IFormFile file)
    {
        if (file == null || file.Length == 0) return BadRequest("File is required");

        //TODO : Koristiti MediatR umesto direktnog poziva servisa
        var existing = await _shipmentService.GetByIdAsync(id);
        if (existing == null) return NotFound();

        // Upload to blob
        var container = _configuration["Azure:BlobContainerName"] ?? "shipments-documents";
        var blobName = $"{id}/{Guid.NewGuid()}_{file.FileName}";
        using var stream = file.OpenReadStream();
        var blobUrl = await _blobService.UploadAsync(container, blobName, stream, file.ContentType);

        // Save metadata & update status & send message
        //TODO : Koristiti MediatR umesto direktnog poziva servisa
        await _shipmentService.AddDocumentAsync(id, blobName, blobUrl);
        await _busPublisher.PublishDocumentToProcessAsync(id, blobName);

        return Accepted(new { blobName, blobUrl });
    }
}