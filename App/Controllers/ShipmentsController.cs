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

    public ShipmentsController(
        IBlobService blobService,
        IServiceBusPublisher busPublisher,
        IConfiguration configuration,
        IMediator mediator)
    {
        _blobService = blobService;
        _busPublisher = busPublisher;
        _configuration = configuration;
        _mediator = mediator;
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

        var defaultPageSize = 5;
        var maxPageSize = 100;

        if (pageSize <= 0)
        {
            pageSize = defaultPageSize;
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
    public async Task<IActionResult> UploadDocument(int id, IFormFile file)
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

        // Upload to blob
        var container = _configuration["Azure:BlobContainerName"] ?? "shipments-documents";
        var blobName = $"{id}/{Guid.NewGuid()}_{file.FileName}";
        var stream = file.OpenReadStream();
        var blobUrl = await _blobService.UploadAsync(container, blobName, stream, file.ContentType);

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
}