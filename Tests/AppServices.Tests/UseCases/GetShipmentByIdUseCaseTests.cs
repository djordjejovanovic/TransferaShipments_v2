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
    public async Task ShouldReturnShipmentById()
    {
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

        var result = await _useCase.Handle(request, cancellationToken);

        result.Shipment.Should().NotBeNull();
        result.Shipment.Should().BeEquivalentTo(expectedShipment);
    }
}
