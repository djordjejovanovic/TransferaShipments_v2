using AppServices.UseCases;
using Microsoft.EntityFrameworkCore;
using TransferaShipments.App.Filters;
using TransferaShipments.BlobStorage.Services;
using TransferaShipments.Core.Repositories;
using TransferaShipments.Core.Services;
using TransferaShipments.Persistence.Data;
using TransferaShipments.Persistence.Repositories;
using TransferaShipments.Persistence.Services;
using TransferaShipments.ServiceBus.HostedServices;
using TransferaShipments.ServiceBus.Services;

var builder = WebApplication.CreateBuilder(args);

// Configuration
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                     .AddEnvironmentVariables();

// DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer")));

// DI - Persistence / Core
builder.Services.AddScoped<IShipmentRepository, ShipmentRepository>();
builder.Services.AddScoped<IShipmentService, ShipmentService>();

// MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<CreateShipmentUseCase>());

// Blob Storage
builder.Services.AddSingleton<IBlobService, BlobService>();

// Service Bus
var serviceBusConn = builder.Configuration.GetConnectionString("ServiceBus");
var hasValidServiceBus = !string.IsNullOrWhiteSpace(serviceBusConn) 
    && serviceBusConn.StartsWith("Endpoint=sb://", StringComparison.OrdinalIgnoreCase);

if (hasValidServiceBus)
{
    builder.Services.AddSingleton<IServiceBusPublisher, ServiceBusPublisher>();
    builder.Services.AddHostedService<DocumentProcessorHostedService>();
}
else
{
    // nema SB konfiguracije – ne podižemo hosted consumer i koristimo NoOp publisher
    builder.Services.AddSingleton<IServiceBusPublisher, NoOpServiceBusPublisher>();
}

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.OperationFilter<FileUploadOperationFilter>();
});

var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

// Initialize database (with error handling)
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureCreated();
        app.Logger.LogInformation("Database initialized successfully.");
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "Could not connect to database. The application will continue but database operations will fail.");
    }
}

app.Run();