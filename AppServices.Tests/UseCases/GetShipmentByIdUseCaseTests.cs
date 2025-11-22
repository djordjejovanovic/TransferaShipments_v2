using AppServices.UseCases;
using FluentAssertions;
using Moq;
using TransferaShipments.Core.Repositories;
using TransferaShipments.Domain.Entities;
using TransferaShipments.Domain.Enums;
using Xunit;

namespace AppServices.Tests.UseCases;

public class GetShipmentByIdUseCaseTests
{
    private readonly Mock<IShipmentRepository> _mockRepository;
    private readonly GetShipmentByIdUseCase _useCase;

    public GetShipmentByIdUseCaseTests()
    {
        _mockRepository = new Mock<IShipmentRepository>();
        _useCase = new GetShipmentByIdUseCase(_mockRepository.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnShipment_WhenShipmentExists()
    {
        // Arrange
        var shipmentId = 1;
        var expectedShipment = new Shipment
        {
            Id = shipmentId,
            ReferenceNumber = "REF-12345",
            Sender = "John Doe",
            Recipient = "Jane Smith",
            Status = ShipmentStatus.Created,
            CreatedAt = DateTime.UtcNow
        };

        _mockRepository
            .Setup(repo => repo.GetByIdAsync(shipmentId))
            .ReturnsAsync(expectedShipment);

        var request = new GetShipmentByIdRequest(shipmentId);

        // Act
        var result = await _useCase.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Shipment.Should().NotBeNull();
        result.Shipment!.Id.Should().Be(shipmentId);
        result.Shipment.ReferenceNumber.Should().Be("REF-12345");
        result.Shipment.Sender.Should().Be("John Doe");
        result.Shipment.Recipient.Should().Be("Jane Smith");
        
        _mockRepository.Verify(
            repo => repo.GetByIdAsync(shipmentId),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenShipmentDoesNotExist()
    {
        // Arrange
        var shipmentId = 999;
        
        _mockRepository
            .Setup(repo => repo.GetByIdAsync(shipmentId))
            .ReturnsAsync((Shipment?)null);

        var request = new GetShipmentByIdRequest(shipmentId);

        // Act
        var result = await _useCase.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Shipment.Should().BeNull();
        
        _mockRepository.Verify(
            repo => repo.GetByIdAsync(shipmentId),
            Times.Once
        );
    }

    [Theory]
    [InlineData(1)]
    [InlineData(42)]
    [InlineData(100)]
    public async Task Handle_ShouldCallRepositoryWithCorrectId(int shipmentId)
    {
        // Arrange
        _mockRepository
            .Setup(repo => repo.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Shipment?)null);

        var request = new GetShipmentByIdRequest(shipmentId);

        // Act
        await _useCase.Handle(request, CancellationToken.None);

        // Assert
        _mockRepository.Verify(
            repo => repo.GetByIdAsync(shipmentId),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnShipmentWithAllStatuses()
    {
        // Arrange
        var shipmentId = 5;
        var testStatuses = new[] 
        { 
            ShipmentStatus.Created, 
            ShipmentStatus.DocumentUploaded, 
            ShipmentStatus.Processed 
        };

        foreach (var status in testStatuses)
        {
            var shipment = new Shipment
            {
                Id = shipmentId,
                ReferenceNumber = "REF-STATUS-TEST",
                Sender = "Test Sender",
                Recipient = "Test Recipient",
                Status = status,
                CreatedAt = DateTime.UtcNow
            };

            _mockRepository
                .Setup(repo => repo.GetByIdAsync(shipmentId))
                .ReturnsAsync(shipment);

            var request = new GetShipmentByIdRequest(shipmentId);

            // Act
            var result = await _useCase.Handle(request, CancellationToken.None);

            // Assert
            result.Shipment.Should().NotBeNull();
            result.Shipment!.Status.Should().Be(status);
        }
    }

    [Fact]
    public async Task Handle_ShouldReturnShipmentWithDocumentInfo_WhenAvailable()
    {
        // Arrange
        var shipmentId = 10;
        var shipment = new Shipment
        {
            Id = shipmentId,
            ReferenceNumber = "REF-DOC-TEST",
            Sender = "Sender",
            Recipient = "Recipient",
            Status = ShipmentStatus.DocumentUploaded,
            CreatedAt = DateTime.UtcNow,
            LastDocumentBlobName = "document.pdf",
            LastDocumentUrl = "https://example.com/document.pdf"
        };

        _mockRepository
            .Setup(repo => repo.GetByIdAsync(shipmentId))
            .ReturnsAsync(shipment);

        var request = new GetShipmentByIdRequest(shipmentId);

        // Act
        var result = await _useCase.Handle(request, CancellationToken.None);

        // Assert
        result.Shipment.Should().NotBeNull();
        result.Shipment!.LastDocumentBlobName.Should().Be("document.pdf");
        result.Shipment.LastDocumentUrl.Should().Be("https://example.com/document.pdf");
    }
}
