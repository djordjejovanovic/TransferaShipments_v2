# Vodič za povezivanje sa Microsoft Azure Storage Explorer

Ovaj dokument pruža detaljna uputstva za povezivanje TransferaShipments_v2 projekta sa Microsoft Azure Storage Explorer aplikacijom.

## Sadržaj

1. [Šta je Azure Storage Explorer?](#šta-je-azure-storage-explorer)
2. [Instalacija Azure Storage Explorer](#instalacija-azure-storage-explorer)
3. [Povezivanje sa Azurite (Lokalni Emulator)](#povezivanje-sa-azurite-lokalni-emulator)
4. [Povezivanje sa Azure Storage nalogom](#povezivanje-sa-azure-storage-nalogom)
5. [Korišćenje Azure Storage Explorer](#korišćenje-azure-storage-explorer)
6. [Rešavanje problema](#rešavanje-problema)

## Šta je Azure Storage Explorer?

Microsoft Azure Storage Explorer je samostalna desktop aplikacija koja omogućava:
- Pregledanje i upravljanje Azure Storage podacima
- Rad sa Blob, Queue, Table i File Storage servisima
- Povezivanje sa lokalnim emulatorima (Azurite)
- Upload, download i upravljanje fajlovima
- Besplatna je i multiplatformska (Windows, macOS, Linux)

## Instalacija Azure Storage Explorer

### Preuzmite aplikaciju:

- **Zvanična stranica**: https://azure.microsoft.com/en-us/products/storage/storage-explorer/
- **Direktan link**: https://go.microsoft.com/fwlink/?LinkId=708343

### Sistemski zahtevi:

- **Windows**: Windows 10/11 (64-bit)
- **macOS**: macOS 10.14 ili noviji
- **Linux**: Ubuntu 20.04+, Fedora, Debian

### Instalacija:

1. Preuzmite instalacioni fajl za vaš operativni sistem
2. Pokrenite instalaciju i pratite uputstva
3. Pokrenite aplikaciju nakon instalacije

## Povezivanje sa Azurite (Lokalni Emulator)

Azurite je zvanični Microsoft emulator za Azure Storage koji se koristi za lokalni development.

### Korak 1: Proverite da li je Azurite instaliran

```bash
azurite --version
```

Ako nije instaliran:
```bash
npm install -g azurite
```

### Korak 2: Pokrenite Azurite

Iz glavnog direktorijuma projekta:

```bash
azurite --silent --location ./azurite_workspace
```

**Napomena**: Ostavite ovaj terminal otvoren dok koristite aplikaciju i Storage Explorer.

### Korak 3: Otvorite Azure Storage Explorer

Pokrenite aplikaciju Azure Storage Explorer.

### Korak 4: Dodajte vezu na Azurite

#### Opcija A: Korišćenje Local Emulator opcije (preporučeno)

1. Kliknite na ikonu **"Connect to Azure Storage"** (plava ikona sa konektorom u levom gornjem uglu)
2. Izaberite **"Local storage emulator"** ili **"Attach to a local emulator"**
3. Aplikacija će automatski detektovati Azurite na standardnim portovima
4. Unesite **Display name**: "TransferaShipments Local"
5. Kliknite **"Next"** i zatim **"Connect"**

#### Opcija B: Korišćenje Connection String-a

1. Kliknite na ikonu **"Connect to Azure Storage"**
2. Izaberite **"Storage account or service"**
3. Izaberite **"Connection string"**
4. Nalepite sledeći connection string:

```
DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUb9Q/2qjmreU+oiMTT+j6HjmQSlAvHBSoD6+MdVfn+BOvyFQA9QvwjkHQAUAicK5xCdvQ==;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;
```

5. Unesite **Display name**: "TransferaShipments Azurite"
6. Kliknite **"Next"** i zatim **"Connect"**

### Korak 5: Pregledajte kontejnere

1. U levom panelu, proširite vašu novu konekciju
2. Proširite **"Blob Containers"**
3. Videćete kontejner **"shipments-documents"** nakon što aplikacija kreira prvi dokument

## Povezivanje sa Azure Storage nalogom

Ako želite da koristite pravi Azure Storage nalog umesto lokalnog emulatora:

### Korak 1: Kreirajte Azure Storage Account (ako već nemate)

1. Idite na [Azure Portal](https://portal.azure.com)
2. Kliknite **"Create a resource"**
3. Potražite **"Storage account"** i kliknite **"Create"**
4. Popunite potrebne informacije:
   - **Subscription**: Izaberite vašu pretplatu
   - **Resource group**: Kreirajte novu ili koristite postojeću
   - **Storage account name**: npr. `transferashipments`
   - **Region**: Izaberite region blizu vas
   - **Performance**: Standard
   - **Redundancy**: LRS (Locally-redundant storage) je dovoljno za development
5. Kliknite **"Review + create"** i zatim **"Create"**

### Korak 2: Pribavite Connection String

1. Otvorite vaš Storage Account u Azure Portal-u
2. U levom meniju, pod **"Security + networking"**, kliknite **"Access keys"**
3. Kliknite **"Show keys"** dugme
4. Kopirajte **"Connection string"** (možete koristiti key1 ili key2)

**Connection string izgleda ovako:**
```
DefaultEndpointsProtocol=https;AccountName=transferashipments;AccountKey=XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX==;EndpointSuffix=core.windows.net
```

### Korak 3: Dodajte vezu u Azure Storage Explorer

1. Otvorite Azure Storage Explorer
2. Kliknite na ikonu **"Connect to Azure Storage"**
3. Izaberite **"Storage account or service"**
4. Izaberite **"Connection string"**
5. Nalepite vaš connection string
6. Unesite **Display name**: "TransferaShipments Production"
7. Kliknite **"Next"** i zatim **"Connect"**

### Korak 4: Ažurirajte konfiguraciju aplikacije

Ažurirajte `appsettings.json` ili koristite User Secrets:

```json
{
  "ConnectionStrings": {
    "AzureBlob": "DefaultEndpointsProtocol=https;AccountName=transferashipments;AccountKey=XXXXXXX;EndpointSuffix=core.windows.net"
  }
}
```

**VAŽNO**: Nikad ne commit-ujte production connection string u Git! Koristite:
- User Secrets za development
- Environment variables za production
- Azure Key Vault za production secrets

### Korak 5: Kreirajte potreban kontejner

U Azure Storage Explorer-u:

1. Proširite vašu konekciju
2. Desni klik na **"Blob Containers"**
3. Kliknite **"Create Blob Container"**
4. Unesite ime: `shipments-documents`
5. Podesite **Public access level**: "Private (no anonymous access)"

Ili, aplikacija će automatski kreirati kontejner pri prvom upload-u.

## Korišćenje Azure Storage Explorer

### Osnovne operacije

#### Pregledanje blob-ova
- Dvoklikom otvorite kontejner da vidite sve fajlove
- Fajlovi se prikazuju sa imenom, veličinom, tipom i datumom modifikacije

#### Upload fajlova
- **Metod 1**: Drag & drop fajlove direktno u kontejner
- **Metod 2**: Desni klik u kontejneru → **"Upload"** → **"Upload Files"**
- Možete upload-ovati i cele foldere: **"Upload Folder"**

#### Download fajlova
- Desni klik na fajl → **"Download"**
- Ili drag & drop fajl iz Storage Explorer-a na vaš desktop

#### Brisanje fajlova
- Desni klik na fajl → **"Delete"**
- Za brisanje više fajlova: Ctrl+Click ili Shift+Click za selekciju, zatim Delete

#### Kopiranje URL-a
- Desni klik na fajl → **"Copy URL"**
- Kopirani URL možete koristiti za direktan pristup (ako je kontejner public)

#### Pregled svojstava
- Desni klik na fajl → **"Properties"**
- Videćete metadata, Content-Type, veličinu, i druge detalje

### Napredne operacije

#### Postavljanje metadata
- Desni klik na fajl → **"Properties"** → **"Metadata"** tab
- Dodajte custom key-value parove

#### Kreiranje Shared Access Signature (SAS)
- Desni klik na kontejner ili fajl → **"Get Shared Access Signature"**
- Podesite permissions i expiry time
- Kopirajte generirani SAS URL za bezbedno deljenje

#### Snapshot-ovi
- Desni klik na kontejner → **"Create Snapshot"**
- Snapshot čuva trenutno stanje kontejnera

## Rešavanje problema

### Problem: Ne mogu da se povežem na Azurite

**Rešenje:**
1. Proverite da li je Azurite pokrenut:
   ```bash
   # Pokrenite Azurite
   azurite --silent --location ./azurite_workspace
   ```
2. Proverite da li su portovi slobodni (10000, 10001, 10002)
3. Proverite firewall postavke

### Problem: "Server not found" greška

**Rešenje:**
- Proverite da li je connection string tačan
- Za Azurite, proverite da li koristite `http://127.0.0.1:10000` (ne `localhost`)
- Proverite da li je Azurite pokrenut na standardnim portovima

### Problem: "Authentication failed" za Azure Storage

**Rešenje:**
- Proverite da li je connection string validan i nije istekao
- Regenerišite Access Key u Azure Portal-u ako je potrebno
- Proverite da li imate potrebne permissions na Storage Account-u

### Problem: Ne vidim kontejner "shipments-documents"

**Rešenje:**
- Kontejner se automatski kreira pri prvom upload-u dokumenta kroz API
- Možete ga ručno kreirati u Storage Explorer-u (desni klik na "Blob Containers" → "Create Blob Container")
- Proverite da li aplikacija radi i da li ste upload-ovali neki dokument

### Problem: Azurite se gasi odmah nakon pokretanja

**Rešenje:**
1. Proverite da li je folder `azurite_workspace` kreiran
2. Pokrenite sa debug opcijom da vidite greške:
   ```bash
   azurite --location ./azurite_workspace --debug ./azurite_debug.log
   ```
3. Proverite log fajl za detalje o greškama

### Problem: Storage Explorer pokazuje stari sadržaj

**Rešenje:**
- Kliknite **Refresh** dugme (ili pritisnite F5)
- Ili desni klik na kontejner → **"Refresh"**

## Dodatni resursi

- **Azure Storage Explorer dokumentacija**: https://learn.microsoft.com/en-us/azure/vs-azure-tools-storage-manage-with-storage-explorer
- **Azurite dokumentacija**: https://learn.microsoft.com/en-us/azure/storage/common/storage-use-azurite
- **Azure Storage dokumentacija**: https://learn.microsoft.com/en-us/azure/storage/

## Bezbednosne napomene

- **Nikad ne delite** production connection string-ove javno
- **Ne commit-ujte** connection string-ove u Git
- **Koristite** User Secrets ili Environment Variables za osetljive podatke
- **Regenerišite** Access Keys redovno u Azure Portal-u
- **Koristite** Shared Access Signatures (SAS) za privremeni pristup umesto deljenja Access Keys
- **Podesite** firewall rules u Azure da ograničite pristup samo sa poznatih IP adresa

---

Za dodatna pitanja ili pomoć, pogledajte glavni [README.md](./README.md) ili otvorite issue u repozitorijumu.
