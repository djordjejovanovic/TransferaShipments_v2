````markdown
```markdown
# TransferaShipments_v2 - skeleton

Ovaj repozitorijum sadrži skelet rešenja za:
- ASP.NET Core Web API (App)
- Core (DTOs, interfejsi)
- Domain (entiteti)
- Persistence (EF Core, repository, service)
- BlobStorage (Azure.Storage.Blobs wrapper)
- ServiceBus (Azure.Messaging.ServiceBus sender + hosted consumer)

Kako pokrenuti (lokalno):
1. Otvorite solution u Visual Studio/VS Code ili napravite .sln i dodajte projekte.
2. Podesite connection strings u App/appsettings.json ili preko env var:
   - ConnectionStrings:SqlServer
   - ConnectionStrings:AzureBlob
   - ConnectionStrings:ServiceBus
3. Ako nemate Azure, možete koristiti:
   - Azurite za Blob Storage (AzureBlob konekcioni string: UseDevelopmentStorage=true)
   - Azure Service Bus emulator nije zvanično dostupan; poruke možete testirati integraciono ili podesiti ServiceBus string ako imate nalog.
4. Pokrenite migracije / kreirajte bazu (EF migrations nije uključen, možete koristiti EnsureCreated u DbContext ako brzo testirate).
5. Pokrenite projekat App i isprobajte Swagger: /swagger

Napomene:
- Ovo je skeleton: cilj je arhitektura i primena Azure SDK-ova, DI i hosted servisa.
- Dodajte autentikaciju, validaciju, logging, unit testove i detaljnije error handling po potrebi.
````