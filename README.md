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

## Povezivanje sa Microsoft Azure Storage Explorer

Microsoft Azure Storage Explorer omogućava pregledanje i upravljanje sadržajem u Azure Storage računu, kao i lokalnom Azurite emulatoru.

> 📘 **Detaljno uputstvo**: Za potpuna uputstva sa rešavanjem problema, pogledajte [AZURE_STORAGE_EXPLORER.md](./AZURE_STORAGE_EXPLORER.md)

### Preuzimanje Azure Storage Explorer

Preuzmite i instalirajte Azure Storage Explorer sa zvanične stranice:
- **Link za preuzimanje**: https://azure.microsoft.com/en-us/products/storage/storage-explorer/
- Dostupno za Windows, macOS i Linux

### Povezivanje sa lokalnim Azurite emulatorom

Ako koristite Azurite za lokalni development (što je podrazumevano u ovom projektu):

1. **Pokrenite Azurite** (ako već nije pokrenut):
   ```bash
   azurite --silent --location ./azurite_workspace
   ```

2. **Otvorite Azure Storage Explorer**

3. **Povežite se na Azurite**:
   - Kliknite na ikonu **"Connect"** (plavi konektor u levom gornjem uglu)
   - Ili idite na **Edit → Connect to Azure Storage**

4. **Izaberite metod povezivanja**:
   - Odaberite **"Local storage emulator"** ili **"Attach to a local emulator"**
   - Ako nema ove opcije, odaberite **"Storage account or service"** → **"Connection string"**

5. **Unesite connection string za Azurite**:
   ```
   DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUb9Q/2qjmreU+oiMTT+j6HjmQSlAvHBSoD6+MdVfn+BOvyFQA9QvwjkHQAUAicK5xCdvQ==;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;
   ```
   - **Display name** (opcionalno): "TransferaShipments Azurite"

6. **Kliknite "Next"** i zatim **"Connect"**

7. **Pregledajte sadržaj**:
   - U levom panelu videćete **(Emulator - Default Ports) (Key)**
   - Proširite **Blob Containers** da vidite kontejnere
   - Tražite kontejner **"shipments-documents"** koji koristi aplikacija

### Povezivanje sa pravim Azure Storage računom

Ako koristite pravi Azure Storage nalog umesto Azurite-a:

1. **Pribavite connection string iz Azure Portal-a**:
   - Idite na https://portal.azure.com
   - Otvorite vaš Storage Account
   - U meniju izaberite **"Access keys"**
   - Kopirajte **"Connection string"** (Key1 ili Key2)

2. **U Azure Storage Explorer-u**:
   - Kliknite na ikonu **"Connect"**
   - Odaberite **"Storage account or service"** → **"Connection string"**
   - Nalepite vaš connection string
   - Unesite **Display name** (npr. "TransferaShipments Production")
   - Kliknite **"Next"** i zatim **"Connect"**

3. **Ažurirajte appsettings.json**:
   - Zamenite vrednost `"AzureBlob"` sa vašim production connection string-om:
   ```json
   "ConnectionStrings": {
     "AzureBlob": "DefaultEndpointsProtocol=https;AccountName=youraccountname;AccountKey=youraccountkey;EndpointSuffix=core.windows.net"
   }
   ```

### Korišćenje Azure Storage Explorer-a

Nakon povezivanja možete:

- **Pregledati blob-ove**: Dvoklikom na kontejner vidite sve fajlove
- **Upload fajlova**: Drag & drop ili desni klik → Upload
- **Download fajlova**: Desni klik na fajl → Download
- **Brisanje fajlova**: Desni klik → Delete
- **Kreiranje kontejnera**: Desni klik na "Blob Containers" → Create Blob Container
- **Pregledanje svojstava**: Desni klik → Properties

### Napomene

- **Azurite mora biti pokrenut** pre povezivanja sa Storage Explorer-om
- **Standardni Azurite portovi**:
  - Blob Service: `http://127.0.0.1:10000`
  - Queue Service: `http://127.0.0.1:10001`
  - Table Service: `http://127.0.0.1:10002`
- Ako koristite nestandardne portove za Azurite, prilagodite BlobEndpoint u connection string-u
- Connection string je osetljiv na bezbednost - **nikad ne commit-ujte pravi production connection string u Git**

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
