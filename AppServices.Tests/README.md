# AppServices.Tests

Unit tests for the TransferaShipments_v2 AppServices layer.

## Overview

This project contains comprehensive unit tests for the use cases in the AppServices layer. The tests use xUnit as the testing framework, Moq for mocking dependencies, and FluentAssertions for readable assertions.

## Test Structure

```
AppServices.Tests/
└── UseCases/
    ├── CreateShipmentUseCaseTests.cs
    ├── GetShipmentByIdUseCaseTests.cs
    └── GetAllShipmentsUseCaseTests.cs
```

## Test Coverage

### CreateShipmentUseCaseTests (8 tests)
Tests for creating new shipments:
- Creating shipments with valid data
- Verifying shipment status is set correctly
- Ensuring CreatedAt timestamp is set
- Handling various input combinations
- Verifying repository interactions

### GetShipmentByIdUseCaseTests (8 tests)
Tests for retrieving shipments by ID:
- Retrieving existing shipments
- Handling non-existent shipments (returns null)
- Testing all shipment statuses (Created, DocumentUploaded, Processed)
- Verifying document information is returned
- Testing with various IDs

### GetAllShipmentsUseCaseTests (6 tests)
Tests for retrieving paginated list of shipments:
- Returning shipments with correct pagination
- Handling empty results
- Verifying total count accuracy
- Testing different pagination parameters
- Ensuring shipments with various statuses are returned

## Running the Tests

### Run all tests:
```bash
dotnet test AppServices.Tests/AppServices.Tests.csproj
```

### Run with detailed output:
```bash
dotnet test AppServices.Tests/AppServices.Tests.csproj --logger "console;verbosity=detailed"
```

### Run specific test class:
```bash
dotnet test AppServices.Tests/AppServices.Tests.csproj --filter FullyQualifiedName~CreateShipmentUseCaseTests
```

## Test Results

✓ Total tests: 22  
✓ All tests passing  
✓ No security vulnerabilities  

## Dependencies

- **xUnit** (v3.1.4) - Testing framework
- **Moq** (v4.20.72) - Mocking library
- **FluentAssertions** (v8.8.0) - Fluent assertion library
- **Microsoft.NET.Test.Sdk** - Test SDK

## Best Practices

The tests follow these best practices:
- **AAA Pattern**: Arrange, Act, Assert
- **Clear naming**: Test names describe what is being tested
- **Mocking**: Dependencies are mocked using Moq
- **Isolation**: Each test is independent and isolated
- **Readable assertions**: FluentAssertions for expressive test assertions
- **Theory tests**: Using `[Theory]` with `[InlineData]` for parameterized tests
