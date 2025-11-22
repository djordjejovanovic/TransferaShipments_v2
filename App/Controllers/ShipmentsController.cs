using Microsoft.AspNetCore.Mvc;
using TransferaShipments.Core.DTOs;
using TransferaShipments.BlobStorage.Services;
using TransferaShipments.ServiceBus.Services;
using MediatR;
using AppServices.UseCases;

namespace TransferaShipments.App.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShipmentsController : ControllerBase
{
    private readonly IBlobService _blobService;
    private readonly IServiceBusPublisher _busPublisher;
    private readonly IConfiguration _configuration;
    private readonly IMediator _mediator;
    private readonly ILogger<ShipmentsController> _logger;

    public ShipmentsController(
        IBlobService blobService,
        IServiceBusPublisher busPublisher,
        IConfiguration configuration,
        IMediator mediator,
        ILogger<ShipmentsController> logger)
    {
        _blobService = blobService;
        _busPublisher = busPublisher;
        _configuration = configuration;
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost("Create")]
    public async Task<IActionResult> Create([FromBody] ShipmentCreateDto dto)
    {
        if(dto == null)
        {
            return BadRequest("Empty request");
        }

        var request = new CreateShipmentRequest(dto.ReferenceNumber, dto.Sender, dto.Recipient);

        var response = await _mediator.Send(request);

        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    // Pagination: page and pageSize as query parameters
    [HttpGet("GetAll")]
    public async Task<IActionResult> GetAll([FromQuery] int page, [FromQuery] int pageSize)
    {
        if (page <= 0)
        {
            page = 1;
        }

        var maxPageSize = 100;

        if (pageSize <= 0)
        {
            pageSize = 5;
        }

        pageSize = Math.Min(pageSize, maxPageSize);

        var request = new GetAllShipmentsRequest(page, pageSize);

        var response = await _mediator.Send(request);

        if (response?.Shipments == null)
        {
            return NotFound();
        }

        Response.Headers["X-Total-Count"] = response.TotalCount.ToString();

        var result = new
        {
            Items = response.Shipments,
            TotalCount = response.TotalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(response.TotalCount / (double)pageSize)
        };

        return Ok(result);
    }

    [HttpGet("GetById/{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var request = new GetShipmentByIdRequest(id);

        var response = await _mediator.Send(request);

        if (response?.Shipment == null)
        {
            return NotFound();
        }

        return Ok(response.Shipment);
    }

    [HttpPost("UploadDocument/{id:int}")]
    public async Task<IActionResult> UploadDocument(int id, [FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("File is required");
        }

        var checkRequest = new GetShipmentByIdRequest(id);

        var checkResponse = await _mediator.Send(checkRequest);

        if (checkResponse?.Shipment == null)
        {
            return NotFound();
        }

        try
        {
            // Upload to blob
            var container = _configuration["Azure:BlobContainerName"] ?? "shipments-documents";
            var blobName = $"{id}/{Guid.NewGuid()}_{file.FileName}";
            
            string blobUrl;
            using (var stream = file.OpenReadStream())
            {
                blobUrl = await _blobService.UploadAsync(container, blobName, stream, file.ContentType);
            }

            // Save metadata & update status using MediatR
            var uploadRequest = new UploadDocumentRequest(id, blobName, blobUrl);
            var uploadResponse = await _mediator.Send(uploadRequest);

            if (!uploadResponse.Success)
            {
                return NotFound();
            }

            // Send message to service bus
            await _busPublisher.PublishDocumentToProcessAsync(id, blobName);

            return Accepted(new { blobName, blobUrl });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload document for shipment {ShipmentId}", id);
            return StatusCode(500, new { error = "Failed to upload document. Please ensure the storage service is running and try again." });
        }
    }
}