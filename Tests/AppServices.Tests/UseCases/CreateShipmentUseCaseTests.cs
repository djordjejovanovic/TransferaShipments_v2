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
    public async Task ShouldCreateShipment()
    {
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

        var result = await _useCase.Handle(request, cancellationToken);

        result.Success.Should().BeTrue();
        result.Id.Should().Be(1);
        result.ErrorMessage.Should().BeNull();
    }
}
