using Microsoft.EntityFrameworkCore;
using TransferaShipments.Persistence.Data;
using TransferaShipments.Persistence.Repositories;
using TransferaShipments.Core.Services;
using TransferaShipments.BlobStorage.Services;
using TransferaShipments.ServiceBus.Services;
using TransferaShipments.ServiceBus.HostedServices;
using TransferaShipments.Persistence.Services;

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

using (var scope = app.Services.CreateScope()) { var db = scope.ServiceProvider.GetRequiredService<AppDbContext>(); db.Database.EnsureCreated(); }
app.Run();