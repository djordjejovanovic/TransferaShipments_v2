using Microsoft.EntityFrameworkCore;
using TransferaShipments_v2.Persistence.Data;
using TransferaShipments_v2.Persistence.Repositories;
using TransferaShipments_v2.Core.Services;
using TransferaShipments_v2.BlobStorage.Services;
using TransferaShipments_v2.ServiceBus.Services;
using TransferaShipments_v2.ServiceBus.HostedServices;

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

// Blob Storage
builder.Services.AddSingleton<IBlobService, BlobService>();

// Service Bus
builder.Services.AddSingleton<IServiceBusPublisher, ServiceBusPublisher>();

// Hosted service (consumer)
builder.Services.AddHostedService<DocumentProcessorHostedService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();