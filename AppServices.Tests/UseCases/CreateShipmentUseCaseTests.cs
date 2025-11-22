using AppServices.UseCases;
using FluentAssertions;
using Moq;
using TransferaShipments.Core.Repositories;
using TransferaShipments.Domain.Entities;
using TransferaShipments.Domain.Enums;
using Xunit;

namespace AppServices.Tests.UseCases;

public class CreateShipmentUseCaseTests
{
    private readonly Mock<IShipmentRepository> _mockRepository;
    private readonly CreateShipmentUseCase _useCase;

    public CreateShipmentUseCaseTests()
    {
        _mockRepository = new Mock<IShipmentRepository>();
        _useCase = new CreateShipmentUseCase(_mockRepository.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateShipmentSuccessfully()
    {
        // Arrange
        var request = new CreateShipmentRequest(
            ReferenceNumber: "REF-12345",
            Sender: "John Doe",
            Recipient: "Jane Smith"
        );

        var expectedShipment = new Shipment
        {
            Id = 1,
            ReferenceNumber = request.ReferenceNumber,
            Sender = request.Sender,
            Recipient = request.Recipient,
            Status = ShipmentStatus.Created,
            CreatedAt = DateTime.UtcNow
        };

        _mockRepository
            .Setup(repo => repo.AddAsync(It.IsAny<Shipment>()))
            .ReturnsAsync(expectedShipment);

        // Act
        var result = await _useCase.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        
        _mockRepository.Verify(
            repo => repo.AddAsync(It.Is<Shipment>(s =>
                s.ReferenceNumber == request.ReferenceNumber &&
                s.Sender == request.Sender &&
                s.Recipient == request.Recipient &&
                s.Status == ShipmentStatus.Created
            )),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldSetCorrectShipmentStatus()
    {
        // Arrange
        var request = new CreateShipmentRequest(
            ReferenceNumber: "REF-99999",
            Sender: "Sender Name",
            Recipient: "Recipient Name"
        );

        Shipment? capturedShipment = null;
        _mockRepository
            .Setup(repo => repo.AddAsync(It.IsAny<Shipment>()))
            .Callback<Shipment>(s => capturedShipment = s)
            .ReturnsAsync((Shipment s) => { s.Id = 42; return s; });

        // Act
        await _useCase.Handle(request, CancellationToken.None);

        // Assert
        capturedShipment.Should().NotBeNull();
        capturedShipment!.Status.Should().Be(ShipmentStatus.Created);
    }

    [Fact]
    public async Task Handle_ShouldSetCreatedAtTimestamp()
    {
        // Arrange
        var request = new CreateShipmentRequest(
            ReferenceNumber: "REF-00001",
            Sender: "Test Sender",
            Recipient: "Test Recipient"
        );

        var beforeTest = DateTime.UtcNow.AddSeconds(-1);
        
        Shipment? capturedShipment = null;
        _mockRepository
            .Setup(repo => repo.AddAsync(It.IsAny<Shipment>()))
            .Callback<Shipment>(s => capturedShipment = s)
            .ReturnsAsync((Shipment s) => { s.Id = 1; return s; });

        // Act
        await _useCase.Handle(request, CancellationToken.None);

        var afterTest = DateTime.UtcNow.AddSeconds(1);

        // Assert
        capturedShipment.Should().NotBeNull();
        capturedShipment!.CreatedAt.Should().BeAfter(beforeTest);
        capturedShipment.CreatedAt.Should().BeBefore(afterTest);
    }

    [Fact]
    public async Task Handle_ShouldReturnResponseWithCorrectId()
    {
        // Arrange
        var request = new CreateShipmentRequest(
            ReferenceNumber: "REF-54321",
            Sender: "Alice",
            Recipient: "Bob"
        );

        var shipment = new Shipment { Id = 123 };
        _mockRepository
            .Setup(repo => repo.AddAsync(It.IsAny<Shipment>()))
            .ReturnsAsync(shipment);

        // Act
        var result = await _useCase.Handle(request, CancellationToken.None);

        // Assert
        result.Id.Should().Be(123);
    }

    [Theory]
    [InlineData("REF-001", "Sender1", "Recipient1")]
    [InlineData("REF-002", "Company A", "Company B")]
    [InlineData("SHIP-12345", "John Smith", "Mary Johnson")]
    public async Task Handle_ShouldHandleVariousInputs(string refNumber, string sender, string recipient)
    {
        // Arrange
        var request = new CreateShipmentRequest(refNumber, sender, recipient);
        
        _mockRepository
            .Setup(repo => repo.AddAsync(It.IsAny<Shipment>()))
            .ReturnsAsync((Shipment s) => { s.Id = 1; return s; });

        // Act
        var result = await _useCase.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        
        _mockRepository.Verify(
            repo => repo.AddAsync(It.Is<Shipment>(s =>
                s.ReferenceNumber == refNumber &&
                s.Sender == sender &&
                s.Recipient == recipient
            )),
            Times.Once
        );
    }
}
