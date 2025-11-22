using AppServices.Contracts.Messaging;
using AppServices.Contracts.Repositories;
using AppServices.Contracts.Storage;
using AppServices.UseCases;
using FluentAssertions;
using Moq;
using TransferaShipments.Domain.Entities;
using TransferaShipments.Domain.Enums;

namespace AppServices.Tests.UseCases;

public class UploadDocumentUseCaseTests
{
    private readonly Mock<IShipmentRepository> _shipmentRepositoryMock;
    private readonly Mock<IBlobService> _blobServiceMock;
    private readonly Mock<IServiceBusPublisher> _serviceBusPublisherMock;
    private readonly UploadDocumentUseCase _useCase;

    public UploadDocumentUseCaseTests()
    {
        _shipmentRepositoryMock = new Mock<IShipmentRepository>();
        _blobServiceMock = new Mock<IBlobService>();
        _serviceBusPublisherMock = new Mock<IServiceBusPublisher>();
        _useCase = new UploadDocumentUseCase(
            _shipmentRepositoryMock.Object,
            _blobServiceMock.Object,
            _serviceBusPublisherMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldUploadDocument_WhenAllParametersAreValid()
    {
        // Arrange
        var shipment = new Shipment
        {
            Id = 1,
            ReferenceNumber = "REF001",
            Sender = "Sender A",
            Recipient = "Recipient A",
            Status = ShipmentStatus.Created
        };

        using var fileStream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });
        var request = new UploadDocumentRequest(
            ShipmentId: 1,
            FileStream: fileStream,
            FileName: "document.pdf",
            ContentType: "application/pdf",
            ContainerName: "shipments");
        var cancellationToken = CancellationToken.None;

        var expectedBlobUrl = "https://storage.blob.core.windows.net/shipments/1/guid_document.pdf";

        _shipmentRepositoryMock
            .Setup(x => x.GetByIdAsync(1, cancellationToken))
            .ReturnsAsync(shipment);

        _blobServiceMock
            .Setup(x => x.UploadAsync(
                "shipments",
                It.Is<string>(s => s.StartsWith("1/") && s.EndsWith("_document.pdf")),
                fileStream,
                "application/pdf",
                cancellationToken))
            .ReturnsAsync(expectedBlobUrl);

        _shipmentRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Shipment>(), cancellationToken))
            .Returns(Task.CompletedTask);

        _serviceBusPublisherMock
            .Setup(x => x.PublishDocumentToProcessAsync(1, It.IsAny<string>(), cancellationToken))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _useCase.Handle(request, cancellationToken);

        // Assert
        result.Success.Should().BeTrue();
        result.BlobUrl.Should().Be(expectedBlobUrl);
        result.BlobName.Should().NotBeNullOrEmpty();
        result.BlobName.Should().StartWith("1/");
        result.BlobName.Should().EndWith("_document.pdf");
        result.ErrorMessage.Should().BeNull();

        _blobServiceMock.Verify(
            x => x.UploadAsync("shipments", It.IsAny<string>(), fileStream, "application/pdf", cancellationToken),
            Times.Once);

        _shipmentRepositoryMock.Verify(
            x => x.UpdateAsync(It.Is<Shipment>(s =>
                s.Status == ShipmentStatus.DocumentUploaded &&
                s.LastDocumentBlobName != null &&
                s.LastDocumentUrl == expectedBlobUrl
            ), cancellationToken),
            Times.Once);

        _serviceBusPublisherMock.Verify(
            x => x.PublishDocumentToProcessAsync(1, It.IsAny<string>(), cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnError_WhenFileStreamIsNull()
    {
        // Arrange
        var request = new UploadDocumentRequest(
            ShipmentId: 1,
            FileStream: null!,
            FileName: "document.pdf",
            ContentType: "application/pdf",
            ContainerName: "shipments");
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _useCase.Handle(request, cancellationToken);

        // Assert
        result.Success.Should().BeFalse();
        result.BlobName.Should().BeNull();
        result.BlobUrl.Should().BeNull();
        result.ErrorMessage.Should().Be("File is required");

        _shipmentRepositoryMock.Verify(
            x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _blobServiceMock.Verify(
            x => x.UploadAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnError_WhenShipmentDoesNotExist()
    {
        // Arrange
        using var fileStream = new MemoryStream(new byte[] { 1, 2, 3 });
        var request = new UploadDocumentRequest(
            ShipmentId: 999,
            FileStream: fileStream,
            FileName: "document.pdf",
            ContentType: "application/pdf",
            ContainerName: "shipments");
        var cancellationToken = CancellationToken.None;

        _shipmentRepositoryMock
            .Setup(x => x.GetByIdAsync(999, cancellationToken))
            .ReturnsAsync((Shipment?)null);

        // Act
        var result = await _useCase.Handle(request, cancellationToken);

        // Assert
        result.Success.Should().BeFalse();
        result.BlobName.Should().BeNull();
        result.BlobUrl.Should().BeNull();
        result.ErrorMessage.Should().Be("Shipment not found");

        _blobServiceMock.Verify(
            x => x.UploadAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Handle_ShouldReturnError_WhenFileNameIsInvalid(string? fileName)
    {
        // Arrange
        var shipment = new Shipment { Id = 1, Status = ShipmentStatus.Created };
        using var fileStream = new MemoryStream(new byte[] { 1, 2, 3 });
        var request = new UploadDocumentRequest(
            ShipmentId: 1,
            FileStream: fileStream,
            FileName: fileName!,
            ContentType: "application/pdf",
            ContainerName: "shipments");
        var cancellationToken = CancellationToken.None;

        _shipmentRepositoryMock
            .Setup(x => x.GetByIdAsync(1, cancellationToken))
            .ReturnsAsync(shipment);

        // Act
        var result = await _useCase.Handle(request, cancellationToken);

        // Assert
        result.Success.Should().BeFalse();
        result.BlobName.Should().BeNull();
        result.BlobUrl.Should().BeNull();
        result.ErrorMessage.Should().Be("Invalid file name");

        _blobServiceMock.Verify(
            x => x.UploadAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldSanitizeFileName_RemovingDirectoryPath()
    {
        // Arrange
        var shipment = new Shipment
        {
            Id = 1,
            ReferenceNumber = "REF001",
            Status = ShipmentStatus.Created
        };

        using var fileStream = new MemoryStream(new byte[] { 1, 2, 3 });
        var request = new UploadDocumentRequest(
            ShipmentId: 1,
            FileStream: fileStream,
            FileName: "/path/to/malicious/../../../document.pdf",
            ContentType: "application/pdf",
            ContainerName: "shipments");
        var cancellationToken = CancellationToken.None;

        var expectedBlobUrl = "https://storage.blob.core.windows.net/shipments/1/guid_document.pdf";

        _shipmentRepositoryMock
            .Setup(x => x.GetByIdAsync(1, cancellationToken))
            .ReturnsAsync(shipment);

        _blobServiceMock
            .Setup(x => x.UploadAsync(
                It.IsAny<string>(),
                It.Is<string>(s => s.Contains("document.pdf") && !s.Contains("..") && !s.Contains("/path")),
                It.IsAny<Stream>(),
                It.IsAny<string>(),
                cancellationToken))
            .ReturnsAsync(expectedBlobUrl);

        _shipmentRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Shipment>(), cancellationToken))
            .Returns(Task.CompletedTask);

        _serviceBusPublisherMock
            .Setup(x => x.PublishDocumentToProcessAsync(It.IsAny<int>(), It.IsAny<string>(), cancellationToken))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _useCase.Handle(request, cancellationToken);

        // Assert
        result.Success.Should().BeTrue();
        result.BlobName.Should().EndWith("_document.pdf");
        result.BlobName.Should().NotContain("..");
        result.BlobName.Should().NotContain("/path");
    }

    [Fact]
    public async Task Handle_ShouldUpdateShipmentStatus_ToDocumentUploaded()
    {
        // Arrange
        var shipment = new Shipment
        {
            Id = 1,
            Status = ShipmentStatus.Created
        };

        using var fileStream = new MemoryStream(new byte[] { 1, 2, 3 });
        var request = new UploadDocumentRequest(
            ShipmentId: 1,
            FileStream: fileStream,
            FileName: "document.pdf",
            ContentType: "application/pdf",
            ContainerName: "shipments");
        var cancellationToken = CancellationToken.None;

        _shipmentRepositoryMock
            .Setup(x => x.GetByIdAsync(1, cancellationToken))
            .ReturnsAsync(shipment);

        _blobServiceMock
            .Setup(x => x.UploadAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>(), cancellationToken))
            .ReturnsAsync("https://blob.url");

        Shipment? updatedShipment = null;
        _shipmentRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Shipment>(), cancellationToken))
            .Callback<Shipment, CancellationToken>((s, ct) => updatedShipment = s)
            .Returns(Task.CompletedTask);

        _serviceBusPublisherMock
            .Setup(x => x.PublishDocumentToProcessAsync(It.IsAny<int>(), It.IsAny<string>(), cancellationToken))
            .Returns(Task.CompletedTask);

        // Act
        await _useCase.Handle(request, cancellationToken);

        // Assert
        updatedShipment.Should().NotBeNull();
        updatedShipment!.Status.Should().Be(ShipmentStatus.DocumentUploaded);
    }

    [Fact]
    public async Task Handle_ShouldPublishMessageToServiceBus_AfterSuccessfulUpload()
    {
        // Arrange
        var shipment = new Shipment
        {
            Id = 1,
            Status = ShipmentStatus.Created
        };

        using var fileStream = new MemoryStream(new byte[] { 1, 2, 3 });
        var request = new UploadDocumentRequest(
            ShipmentId: 1,
            FileStream: fileStream,
            FileName: "document.pdf",
            ContentType: "application/pdf",
            ContainerName: "shipments");
        var cancellationToken = CancellationToken.None;

        _shipmentRepositoryMock
            .Setup(x => x.GetByIdAsync(1, cancellationToken))
            .ReturnsAsync(shipment);

        _blobServiceMock
            .Setup(x => x.UploadAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>(), cancellationToken))
            .ReturnsAsync("https://blob.url");

        _shipmentRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Shipment>(), cancellationToken))
            .Returns(Task.CompletedTask);

        string? publishedBlobName = null;
        _serviceBusPublisherMock
            .Setup(x => x.PublishDocumentToProcessAsync(1, It.IsAny<string>(), cancellationToken))
            .Callback<int, string, CancellationToken>((id, blobName, ct) => publishedBlobName = blobName)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _useCase.Handle(request, cancellationToken);

        // Assert
        result.Success.Should().BeTrue();
        publishedBlobName.Should().NotBeNullOrEmpty();
        publishedBlobName.Should().Be(result.BlobName);

        _serviceBusPublisherMock.Verify(
            x => x.PublishDocumentToProcessAsync(1, It.IsAny<string>(), cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldGenerateUniqueBlobName_WithGuid()
    {
        // Arrange
        var shipment = new Shipment { Id = 1, Status = ShipmentStatus.Created };
        using var fileStream = new MemoryStream(new byte[] { 1, 2, 3 });
        var request = new UploadDocumentRequest(
            ShipmentId: 1,
            FileStream: fileStream,
            FileName: "document.pdf",
            ContentType: "application/pdf",
            ContainerName: "shipments");
        var cancellationToken = CancellationToken.None;

        _shipmentRepositoryMock
            .Setup(x => x.GetByIdAsync(1, cancellationToken))
            .ReturnsAsync(shipment);

        string? capturedBlobName = null;
        _blobServiceMock
            .Setup(x => x.UploadAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>(), cancellationToken))
            .Callback<string, string, Stream, string, CancellationToken>((c, bn, s, ct, token) => capturedBlobName = bn)
            .ReturnsAsync("https://blob.url");

        _shipmentRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Shipment>(), cancellationToken))
            .Returns(Task.CompletedTask);

        _serviceBusPublisherMock
            .Setup(x => x.PublishDocumentToProcessAsync(It.IsAny<int>(), It.IsAny<string>(), cancellationToken))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _useCase.Handle(request, cancellationToken);

        // Assert
        capturedBlobName.Should().NotBeNullOrEmpty();
        capturedBlobName.Should().StartWith("1/");
        capturedBlobName.Should().Contain("_");
        capturedBlobName.Should().EndWith("document.pdf");
        
        // Extract the GUID part and verify it's a valid GUID format
        var parts = capturedBlobName!.Split('/');
        parts.Should().HaveCount(2);
        var fileNamePart = parts[1];
        var guidPart = fileNamePart.Substring(0, fileNamePart.IndexOf('_'));
        Guid.TryParse(guidPart, out _).Should().BeTrue("blob name should contain a valid GUID");
    }
}
