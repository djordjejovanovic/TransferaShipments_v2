using AppServices.Contracts.Repositories;
using AppServices.UseCases;
using FluentAssertions;
using Moq;
using TransferaShipments.Domain.Entities;
using TransferaShipments.Domain.Enums;

namespace AppServices.Tests.UseCases;

public class GetShipmentByIdUseCaseTests
{
    private readonly Mock<IShipmentRepository> _shipmentRepositoryMock;
    private readonly GetShipmentByIdUseCase _useCase;

    public GetShipmentByIdUseCaseTests()
    {
        _shipmentRepositoryMock = new Mock<IShipmentRepository>();
        _useCase = new GetShipmentByIdUseCase(_shipmentRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnShipment_WhenShipmentExists()
    {
        // Arrange
        var request = new GetShipmentByIdRequest(Id: 1);
        var cancellationToken = CancellationToken.None;

        var expectedShipment = new Shipment
        {
            Id = 1,
            ReferenceNumber = "REF001",
            Sender = "Sender A",
            Recipient = "Recipient A",
            Status = ShipmentStatus.Created,
            CreatedAt = DateTime.UtcNow
        };

        _shipmentRepositoryMock
            .Setup(x => x.GetByIdAsync(1, cancellationToken))
            .ReturnsAsync(expectedShipment);

        // Act
        var result = await _useCase.Handle(request, cancellationToken);

        // Assert
        result.Shipment.Should().NotBeNull();
        result.Shipment.Should().BeEquivalentTo(expectedShipment);

        _shipmentRepositoryMock.Verify(
            x => x.GetByIdAsync(1, cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenShipmentDoesNotExist()
    {
        // Arrange
        var request = new GetShipmentByIdRequest(Id: 999);
        var cancellationToken = CancellationToken.None;

        _shipmentRepositoryMock
            .Setup(x => x.GetByIdAsync(999, cancellationToken))
            .ReturnsAsync((Shipment?)null);

        // Act
        var result = await _useCase.Handle(request, cancellationToken);

        // Assert
        result.Shipment.Should().BeNull();

        _shipmentRepositoryMock.Verify(
            x => x.GetByIdAsync(999, cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnShipmentWithAllProperties_WhenShipmentHasDocuments()
    {
        // Arrange
        var request = new GetShipmentByIdRequest(Id: 2);
        var cancellationToken = CancellationToken.None;

        var expectedShipment = new Shipment
        {
            Id = 2,
            ReferenceNumber = "REF002",
            Sender = "Sender B",
            Recipient = "Recipient B",
            Status = ShipmentStatus.DocumentUploaded,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            LastDocumentBlobName = "2/guid_document.pdf",
            LastDocumentUrl = "https://storage.blob.core.windows.net/container/2/guid_document.pdf"
        };

        _shipmentRepositoryMock
            .Setup(x => x.GetByIdAsync(2, cancellationToken))
            .ReturnsAsync(expectedShipment);

        // Act
        var result = await _useCase.Handle(request, cancellationToken);

        // Assert
        result.Shipment.Should().NotBeNull();
        result.Shipment!.Id.Should().Be(2);
        result.Shipment.ReferenceNumber.Should().Be("REF002");
        result.Shipment.Status.Should().Be(ShipmentStatus.DocumentUploaded);
        result.Shipment.LastDocumentBlobName.Should().Be("2/guid_document.pdf");
        result.Shipment.LastDocumentUrl.Should().Be("https://storage.blob.core.windows.net/container/2/guid_document.pdf");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(999)]
    public async Task Handle_ShouldCallRepository_WithCorrectId(int shipmentId)
    {
        // Arrange
        var request = new GetShipmentByIdRequest(Id: shipmentId);
        var cancellationToken = CancellationToken.None;

        _shipmentRepositoryMock
            .Setup(x => x.GetByIdAsync(shipmentId, cancellationToken))
            .ReturnsAsync((Shipment?)null);

        // Act
        await _useCase.Handle(request, cancellationToken);

        // Assert
        _shipmentRepositoryMock.Verify(
            x => x.GetByIdAsync(shipmentId, cancellationToken),
            Times.Once);
    }
}
