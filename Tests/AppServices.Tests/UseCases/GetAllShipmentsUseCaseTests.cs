using AppServices.Common.Models;
using AppServices.Contracts.Repositories;
using AppServices.UseCases;
using FluentAssertions;
using Moq;
using TransferaShipments.Domain.Entities;
using TransferaShipments.Domain.Enums;

namespace AppServices.Tests.UseCases;

public class GetAllShipmentsUseCaseTests
{
    private readonly Mock<IShipmentRepository> _shipmentRepositoryMock;
    private readonly GetAllShipmentsUseCase _useCase;

    public GetAllShipmentsUseCaseTests()
    {
        _shipmentRepositoryMock = new Mock<IShipmentRepository>();
        _useCase = new GetAllShipmentsUseCase(_shipmentRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnPaginatedShipments_WithValidParameters()
    {
        // Arrange
        var request = new GetAllShipmentsRequest(Page: 1, PageSize: 10);
        var cancellationToken = CancellationToken.None;

        var shipments = new List<Shipment>
        {
            new Shipment { Id = 1, ReferenceNumber = "REF001", Sender = "Sender A", Recipient = "Recipient A", Status = ShipmentStatus.Created },
            new Shipment { Id = 2, ReferenceNumber = "REF002", Sender = "Sender B", Recipient = "Recipient B", Status = ShipmentStatus.Created }
        };

        _shipmentRepositoryMock
            .Setup(x => x.GetAllAsync(1, 10, cancellationToken))
            .ReturnsAsync(shipments);

        _shipmentRepositoryMock
            .Setup(x => x.GetCountAsync(cancellationToken))
            .ReturnsAsync(25);

        // Act
        var result = await _useCase.Handle(request, cancellationToken);

        // Assert
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(25);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.TotalPages.Should().Be(3);

        _shipmentRepositoryMock.Verify(
            x => x.GetAllAsync(1, 10, cancellationToken),
            Times.Once);

        _shipmentRepositoryMock.Verify(
            x => x.GetCountAsync(cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldDefaultToPage1_WhenPageIsZero()
    {
        // Arrange
        var request = new GetAllShipmentsRequest(Page: 0, PageSize: 10);
        var cancellationToken = CancellationToken.None;

        _shipmentRepositoryMock
            .Setup(x => x.GetAllAsync(1, 10, cancellationToken))
            .ReturnsAsync(new List<Shipment>());

        _shipmentRepositoryMock
            .Setup(x => x.GetCountAsync(cancellationToken))
            .ReturnsAsync(0);

        // Act
        var result = await _useCase.Handle(request, cancellationToken);

        // Assert
        result.Page.Should().Be(1);
        _shipmentRepositoryMock.Verify(
            x => x.GetAllAsync(1, 10, cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldDefaultToPage1_WhenPageIsNegative()
    {
        // Arrange
        var request = new GetAllShipmentsRequest(Page: -5, PageSize: 10);
        var cancellationToken = CancellationToken.None;

        _shipmentRepositoryMock
            .Setup(x => x.GetAllAsync(1, 10, cancellationToken))
            .ReturnsAsync(new List<Shipment>());

        _shipmentRepositoryMock
            .Setup(x => x.GetCountAsync(cancellationToken))
            .ReturnsAsync(0);

        // Act
        var result = await _useCase.Handle(request, cancellationToken);

        // Assert
        result.Page.Should().Be(1);
        _shipmentRepositoryMock.Verify(
            x => x.GetAllAsync(1, 10, cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldDefaultToPageSize20_WhenPageSizeIsZero()
    {
        // Arrange
        var request = new GetAllShipmentsRequest(Page: 1, PageSize: 0);
        var cancellationToken = CancellationToken.None;

        _shipmentRepositoryMock
            .Setup(x => x.GetAllAsync(1, 20, cancellationToken))
            .ReturnsAsync(new List<Shipment>());

        _shipmentRepositoryMock
            .Setup(x => x.GetCountAsync(cancellationToken))
            .ReturnsAsync(0);

        // Act
        var result = await _useCase.Handle(request, cancellationToken);

        // Assert
        result.PageSize.Should().Be(20);
        _shipmentRepositoryMock.Verify(
            x => x.GetAllAsync(1, 20, cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldDefaultToPageSize20_WhenPageSizeIsNegative()
    {
        // Arrange
        var request = new GetAllShipmentsRequest(Page: 1, PageSize: -10);
        var cancellationToken = CancellationToken.None;

        _shipmentRepositoryMock
            .Setup(x => x.GetAllAsync(1, 20, cancellationToken))
            .ReturnsAsync(new List<Shipment>());

        _shipmentRepositoryMock
            .Setup(x => x.GetCountAsync(cancellationToken))
            .ReturnsAsync(0);

        // Act
        var result = await _useCase.Handle(request, cancellationToken);

        // Assert
        result.PageSize.Should().Be(20);
        _shipmentRepositoryMock.Verify(
            x => x.GetAllAsync(1, 20, cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldLimitPageSizeTo100_WhenPageSizeExceeds100()
    {
        // Arrange
        var request = new GetAllShipmentsRequest(Page: 1, PageSize: 150);
        var cancellationToken = CancellationToken.None;

        _shipmentRepositoryMock
            .Setup(x => x.GetAllAsync(1, 100, cancellationToken))
            .ReturnsAsync(new List<Shipment>());

        _shipmentRepositoryMock
            .Setup(x => x.GetCountAsync(cancellationToken))
            .ReturnsAsync(0);

        // Act
        var result = await _useCase.Handle(request, cancellationToken);

        // Assert
        result.PageSize.Should().Be(100);
        _shipmentRepositoryMock.Verify(
            x => x.GetAllAsync(1, 100, cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoShipmentsExist()
    {
        // Arrange
        var request = new GetAllShipmentsRequest(Page: 1, PageSize: 10);
        var cancellationToken = CancellationToken.None;

        _shipmentRepositoryMock
            .Setup(x => x.GetAllAsync(1, 10, cancellationToken))
            .ReturnsAsync(new List<Shipment>());

        _shipmentRepositoryMock
            .Setup(x => x.GetCountAsync(cancellationToken))
            .ReturnsAsync(0);

        // Act
        var result = await _useCase.Handle(request, cancellationToken);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldCalculateTotalPagesCorrectly()
    {
        // Arrange
        var request = new GetAllShipmentsRequest(Page: 1, PageSize: 10);
        var cancellationToken = CancellationToken.None;

        _shipmentRepositoryMock
            .Setup(x => x.GetAllAsync(1, 10, cancellationToken))
            .ReturnsAsync(new List<Shipment>());

        _shipmentRepositoryMock
            .Setup(x => x.GetCountAsync(cancellationToken))
            .ReturnsAsync(95);

        // Act
        var result = await _useCase.Handle(request, cancellationToken);

        // Assert
        result.TotalPages.Should().Be(10); // Math.Ceiling(95 / 10.0) = 10
    }
}
