using AppServices.UseCases;
using FluentAssertions;
using Moq;
using TransferaShipments.Core.Repositories;
using TransferaShipments.Domain.Entities;
using TransferaShipments.Domain.Enums;
using Xunit;

namespace AppServices.Tests.UseCases;

public class GetAllShipmentsUseCaseTests
{
    private readonly Mock<IShipmentRepository> _mockRepository;
    private readonly GetAllShipmentsUseCase _useCase;

    public GetAllShipmentsUseCaseTests()
    {
        _mockRepository = new Mock<IShipmentRepository>();
        _useCase = new GetAllShipmentsUseCase(_mockRepository.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnShipmentsWithCount()
    {
        // Arrange
        var shipments = new List<Shipment>
        {
            new Shipment
            {
                Id = 1,
                ReferenceNumber = "REF-001",
                Sender = "Sender 1",
                Recipient = "Recipient 1",
                Status = ShipmentStatus.Created,
                CreatedAt = DateTime.UtcNow
            },
            new Shipment
            {
                Id = 2,
                ReferenceNumber = "REF-002",
                Sender = "Sender 2",
                Recipient = "Recipient 2",
                Status = ShipmentStatus.DocumentUploaded,
                CreatedAt = DateTime.UtcNow
            }
        };

        _mockRepository
            .Setup(repo => repo.GetAllAsync(1, 10))
            .ReturnsAsync(shipments);

        _mockRepository
            .Setup(repo => repo.GetCountAsync())
            .ReturnsAsync(2);

        var request = new GetAllShipmentsRequest(Page: 1, PageSize: 10);

        // Act
        var result = await _useCase.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Shipments.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        
        _mockRepository.Verify(
            repo => repo.GetAllAsync(1, 10),
            Times.Once
        );
        
        _mockRepository.Verify(
            repo => repo.GetCountAsync(),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoShipments()
    {
        // Arrange
        var emptyList = new List<Shipment>();

        _mockRepository
            .Setup(repo => repo.GetAllAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(emptyList);

        _mockRepository
            .Setup(repo => repo.GetCountAsync())
            .ReturnsAsync(0);

        var request = new GetAllShipmentsRequest(Page: 1, PageSize: 10);

        // Act
        var result = await _useCase.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Shipments.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Theory]
    [InlineData(1, 10)]
    [InlineData(2, 20)]
    [InlineData(3, 5)]
    public async Task Handle_ShouldCallRepositoryWithCorrectPaginationParameters(int page, int pageSize)
    {
        // Arrange
        _mockRepository
            .Setup(repo => repo.GetAllAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(new List<Shipment>());

        _mockRepository
            .Setup(repo => repo.GetCountAsync())
            .ReturnsAsync(0);

        var request = new GetAllShipmentsRequest(Page: page, PageSize: pageSize);

        // Act
        await _useCase.Handle(request, CancellationToken.None);

        // Assert
        _mockRepository.Verify(
            repo => repo.GetAllAsync(page, pageSize),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectTotalCount_WhenMoreItemsThanPageSize()
    {
        // Arrange
        var shipments = new List<Shipment>
        {
            new Shipment
            {
                Id = 1,
                ReferenceNumber = "REF-001",
                Sender = "Sender",
                Recipient = "Recipient",
                Status = ShipmentStatus.Created,
                CreatedAt = DateTime.UtcNow
            }
        };

        _mockRepository
            .Setup(repo => repo.GetAllAsync(1, 1))
            .ReturnsAsync(shipments);

        _mockRepository
            .Setup(repo => repo.GetCountAsync())
            .ReturnsAsync(100); // Total count is 100, but we only get 1 per page

        var request = new GetAllShipmentsRequest(Page: 1, PageSize: 1);

        // Act
        var result = await _useCase.Handle(request, CancellationToken.None);

        // Assert
        result.Shipments.Should().HaveCount(1);
        result.TotalCount.Should().Be(100);
    }

    [Fact]
    public async Task Handle_ShouldReturnShipmentsWithDifferentStatuses()
    {
        // Arrange
        var shipments = new List<Shipment>
        {
            new Shipment
            {
                Id = 1,
                ReferenceNumber = "REF-001",
                Sender = "Sender",
                Recipient = "Recipient",
                Status = ShipmentStatus.Created,
                CreatedAt = DateTime.UtcNow
            },
            new Shipment
            {
                Id = 2,
                ReferenceNumber = "REF-002",
                Sender = "Sender",
                Recipient = "Recipient",
                Status = ShipmentStatus.DocumentUploaded,
                CreatedAt = DateTime.UtcNow
            },
            new Shipment
            {
                Id = 3,
                ReferenceNumber = "REF-003",
                Sender = "Sender",
                Recipient = "Recipient",
                Status = ShipmentStatus.Processed,
                CreatedAt = DateTime.UtcNow
            }
        };

        _mockRepository
            .Setup(repo => repo.GetAllAsync(1, 10))
            .ReturnsAsync(shipments);

        _mockRepository
            .Setup(repo => repo.GetCountAsync())
            .ReturnsAsync(3);

        var request = new GetAllShipmentsRequest(Page: 1, PageSize: 10);

        // Act
        var result = await _useCase.Handle(request, CancellationToken.None);

        // Assert
        result.Shipments.Should().HaveCount(3);
        result.Shipments.Should().Contain(s => s.Status == ShipmentStatus.Created);
        result.Shipments.Should().Contain(s => s.Status == ShipmentStatus.DocumentUploaded);
        result.Shipments.Should().Contain(s => s.Status == ShipmentStatus.Processed);
    }

    [Fact]
    public async Task Handle_ShouldReturnShipmentsInCorrectOrder()
    {
        // Arrange
        var shipment1 = new Shipment
        {
            Id = 1,
            ReferenceNumber = "REF-001",
            Sender = "Sender",
            Recipient = "Recipient",
            Status = ShipmentStatus.Created,
            CreatedAt = DateTime.UtcNow.AddDays(-2)
        };

        var shipment2 = new Shipment
        {
            Id = 2,
            ReferenceNumber = "REF-002",
            Sender = "Sender",
            Recipient = "Recipient",
            Status = ShipmentStatus.Created,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        var shipment3 = new Shipment
        {
            Id = 3,
            ReferenceNumber = "REF-003",
            Sender = "Sender",
            Recipient = "Recipient",
            Status = ShipmentStatus.Created,
            CreatedAt = DateTime.UtcNow
        };

        var shipments = new List<Shipment> { shipment1, shipment2, shipment3 };

        _mockRepository
            .Setup(repo => repo.GetAllAsync(1, 10))
            .ReturnsAsync(shipments);

        _mockRepository
            .Setup(repo => repo.GetCountAsync())
            .ReturnsAsync(3);

        var request = new GetAllShipmentsRequest(Page: 1, PageSize: 10);

        // Act
        var result = await _useCase.Handle(request, CancellationToken.None);

        // Assert
        result.Shipments.Should().HaveCount(3);
        var shipmentList = result.Shipments.ToList();
        shipmentList[0].Id.Should().Be(1);
        shipmentList[1].Id.Should().Be(2);
        shipmentList[2].Id.Should().Be(3);
    }
}
