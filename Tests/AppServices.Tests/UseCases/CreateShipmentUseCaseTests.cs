using AppServices.Contracts.Repositories;
using AppServices.UseCases;
using FluentAssertions;
using Moq;
using TransferaShipments.Domain.Entities;
using TransferaShipments.Domain.Enums;

namespace AppServices.Tests.UseCases;

public class CreateShipmentUseCaseTests
{
    private readonly Mock<IShipmentRepository> _shipmentRepositoryMock;
    private readonly CreateShipmentUseCase _useCase;

    public CreateShipmentUseCaseTests()
    {
        _shipmentRepositoryMock = new Mock<IShipmentRepository>();
        _useCase = new CreateShipmentUseCase(_shipmentRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateShipment_WhenReferenceNumberIsUnique()
    {
        // Arrange
        var request = new CreateShipmentRequest("REF001", "Sender A", "Recipient B");
        var cancellationToken = CancellationToken.None;

        _shipmentRepositoryMock
            .Setup(x => x.GetByReferenceNumberAsync(request.ReferenceNumber, cancellationToken))
            .ReturnsAsync((Shipment?)null);

        var createdShipment = new Shipment
        {
            Id = 1,
            ReferenceNumber = request.ReferenceNumber,
            Sender = request.Sender,
            Recipient = request.Recipient,
            Status = ShipmentStatus.Created
        };

        _shipmentRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Shipment>(), cancellationToken))
            .ReturnsAsync(createdShipment);

        // Act
        var result = await _useCase.Handle(request, cancellationToken);

        // Assert
        result.Success.Should().BeTrue();
        result.Id.Should().Be(1);
        result.ErrorMessage.Should().BeNull();

        _shipmentRepositoryMock.Verify(
            x => x.GetByReferenceNumberAsync(request.ReferenceNumber, cancellationToken),
            Times.Once);

        _shipmentRepositoryMock.Verify(
            x => x.AddAsync(It.Is<Shipment>(s =>
                s.ReferenceNumber == request.ReferenceNumber &&
                s.Sender == request.Sender &&
                s.Recipient == request.Recipient &&
                s.Status == ShipmentStatus.Created
            ), cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnError_WhenReferenceNumberAlreadyExists()
    {
        // Arrange
        var request = new CreateShipmentRequest("REF001", "Sender A", "Recipient B");
        var cancellationToken = CancellationToken.None;

        var existingShipment = new Shipment
        {
            Id = 1,
            ReferenceNumber = request.ReferenceNumber,
            Sender = "Existing Sender",
            Recipient = "Existing Recipient"
        };

        _shipmentRepositoryMock
            .Setup(x => x.GetByReferenceNumberAsync(request.ReferenceNumber, cancellationToken))
            .ReturnsAsync(existingShipment);

        // Act
        var result = await _useCase.Handle(request, cancellationToken);

        // Assert
        result.Success.Should().BeFalse();
        result.Id.Should().BeNull();
        result.ErrorMessage.Should().Be($"Shipment with ReferenceNumber '{request.ReferenceNumber}' already exists.");

        _shipmentRepositoryMock.Verify(
            x => x.GetByReferenceNumberAsync(request.ReferenceNumber, cancellationToken),
            Times.Once);

        _shipmentRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Shipment>(), cancellationToken),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldSetCreatedAtToUtcNow()
    {
        // Arrange
        var request = new CreateShipmentRequest("REF002", "Sender C", "Recipient D");
        var cancellationToken = CancellationToken.None;
        var beforeTime = DateTime.UtcNow;

        _shipmentRepositoryMock
            .Setup(x => x.GetByReferenceNumberAsync(request.ReferenceNumber, cancellationToken))
            .ReturnsAsync((Shipment?)null);

        Shipment? capturedShipment = null;
        _shipmentRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Shipment>(), cancellationToken))
            .Callback<Shipment, CancellationToken>((s, ct) => capturedShipment = s)
            .ReturnsAsync((Shipment s, CancellationToken ct) => { s.Id = 2; return s; });

        // Act
        await _useCase.Handle(request, cancellationToken);
        var afterTime = DateTime.UtcNow;

        // Assert
        capturedShipment.Should().NotBeNull();
        capturedShipment!.CreatedAt.Should().BeOnOrAfter(beforeTime);
        capturedShipment.CreatedAt.Should().BeOnOrBefore(afterTime);
    }

    [Fact]
    public async Task Handle_ShouldSetInitialStatusToCreated()
    {
        // Arrange
        var request = new CreateShipmentRequest("REF003", "Sender E", "Recipient F");
        var cancellationToken = CancellationToken.None;

        _shipmentRepositoryMock
            .Setup(x => x.GetByReferenceNumberAsync(request.ReferenceNumber, cancellationToken))
            .ReturnsAsync((Shipment?)null);

        Shipment? capturedShipment = null;
        _shipmentRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Shipment>(), cancellationToken))
            .Callback<Shipment, CancellationToken>((s, ct) => capturedShipment = s)
            .ReturnsAsync((Shipment s, CancellationToken ct) => { s.Id = 3; return s; });

        // Act
        await _useCase.Handle(request, cancellationToken);

        // Assert
        capturedShipment.Should().NotBeNull();
        capturedShipment!.Status.Should().Be(ShipmentStatus.Created);
    }
}
