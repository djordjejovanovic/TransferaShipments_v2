# TransferaShipments_v2 - skeleton

Ovaj repozitorijum sadrži skelet rešenja za:
- ASP.NET Core Web API (App)
- Core (DTOs, interfejsi)
- Domain (entiteti)
- Persistence (EF Core, repository, service)
- BlobStorage (Azure.Storage.Blobs wrapper)
- ServiceBus (Azure.Messaging.ServiceBus sender + hosted consumer)

## Kako pokrenuti (lokalno)

### Preduslovi
- .NET 8.0 SDK
- Node.js (za Azurite)

### Brzo pokretanje

1. **Klonirajte repozitorijum**
   ```bash
   git clone https://github.com/djordjejovanovic/TransferaShipments_v2.git
   cd TransferaShipments_v2
   ```

2. **Instalirajte Azurite (za lokalni Blob Storage)**
   ```bash
   npm install -g azurite
   ```

3. **Pokrenite Azurite**
   ```bash
   azurite --silent --location ./azurite_workspace
   ```

4. **Pokrenite aplikaciju**
   ```bash
   cd App
   dotnet run
   ```

5. **Otvorite Swagger UI**
   - Navigirajte na: http://localhost:52752/swagger
   - Ili HTTPS: https://localhost:52751/swagger

### Connection Strings

Podrazumevane vrednosti u `appsettings.json`:

- **AzureBlob**: `UseDevelopmentStorage=true` (koristi Azurite)
- **ServiceBus**: `""` (prazno - koristi NoOp publisher)
- **SqlServer**: `Server=localhost;Database=TransferaShipments;...`

**Napomena**: Aplikacija će se pokrenuti i bez SQL Server baze - samo će prikazati upozorenje. Blob Storage će raditi sa Azurite-om.

### Service Bus

Ako nemate Azure Service Bus, aplikacija će automatski koristiti **NoOp publisher** koji samo loguje poruke umesto da ih šalje. Ovo omogućava testiranje bez aktivnog Service Bus-a.

Za korišćenje pravog Service Bus-a, podesite connection string:
```json
"ServiceBus": "Endpoint=sb://your-namespace.servicebus.windows.net/;SharedAccessKeyName=...;SharedAccessKey=..."
```

### Testiranje API-ja

#### Kreiranje pošiljke (Shipment)
```bash
curl -X POST http://localhost:52752/api/Shipments \
  -H "Content-Type: application/json" \
  -d '{"referenceNumber":"REF001","sender":"Pošiljalac","recipient":"Primalac"}'
```

#### Upload dokumenta
```bash
curl -X POST http://localhost:52752/api/Shipments/1/documents \
  -F "file=@/path/to/file.pdf"
```

## Arhitektura

- **App**: ASP.NET Core Web API sa Swagger dokumentacijom
- **Core**: Application services, DTOs, interfejsi (MediatR use cases)
- **Domain**: Domain entiteti
- **Persistence**: EF Core, repositories
- **BlobStorage**: Azure Blob Storage wrapper (podržava Azurite)
- **ServiceBus**: Azure Service Bus publisher i consumer (sa NoOp implementacijom)

## Napomene

- Ovo je skeleton projekat - cilj je prikazati arhitekturu i integraciju sa Azure servisima
- Aplikacija podržava lokalni development sa Azurite-om bez potrebe za Azure nalogom
- NoOp Service Bus publisher omogućava testiranje bez aktivnog Service Bus-a
- Dodajte autentikaciju, validaciju, logging, unit testove i detaljnije error handling po potrebi
