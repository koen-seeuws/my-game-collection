# Game Vault: retro game & console collection tracker

A self-hosted app for keeping track of your (retro) games and consoles. Built with **.NET 10**, **Blazor (Interactive Server)**, **MudBlazor** and **PostgreSQL**, and set up to run on a home server with **Docker**. It can be installed as a **PWA**.

## Features

- Games and consoles, each tied to a platform. About 45 retro and modern platforms are added on first start, and you can manage them under *Platforms*.
- Filters for text search (title, barcode, notes, storage location, platform), type, platform, status (owned, wishlist, for sale, sold), region, minimum condition, CIB only, and with or without photos. You can sort the results and switch between a grid and a table. Filters are kept while you navigate.
- **Barcode scanning**:
  - Live camera scanning (EAN-13/8, UPC-A/E, Code 128/39) with [html5-qrcode](https://github.com/mebjas/html5-qrcode). The library is bundled with the app, so no CDN is needed.
  - A *From photo* fallback for when the page isn't served over HTTPS.
  - Manual entry, or a USB/Bluetooth barcode scanner: it types the code into the field and presses Enter.
- The form is filled in automatically from free APIs:
  1. **Your own collection**. If you already own an item with that barcode, its data is reused and you get a duplicate warning.
  2. **[UPCitemdb](https://www.upcitemdb.com/)** free trial endpoint. No key needed, about 100 lookups per day.
  3. **[Open Products Facts](https://world.openproductsfacts.org/)** as a fallback. Open data, no key.
  4. **[RAWG](https://rawg.io/apidocs)** (optional, free key) adds release year, developer, publisher, genre and artwork. It also powers the *Find details by title* button, which helps for old games without a barcode.

  The raw product title is cleaned up ("Super Mario 64 - Nintendo 64 (PAL)" becomes "Super Mario 64"). The platform is matched by name and aliases, games are told apart from hardware, and the region is guessed from the title or from the barcode's GS1 country prefix.
- **Photos of your own copy**:
  - The browser resizes each photo to at most 2048 px and also makes a 480 px thumbnail. The server stores both files.
  - Files are saved on disk at `/data/photos/{itemId}/…`, which is a host folder mounted into the container.
  - You can pick which photo is the cover, delete photos, and view them full-screen.
- A dashboard with counts, money spent and estimated value, your top platforms, and recently added items.
- Dark and light theme.

## Architecture (Clean Architecture + vertical slices)

```
src/
  Domain/
    MyGameCollection.Domain                     Entities (CollectionItem → Game / GameConsole, Platform, ItemPhoto), enums.
                                                Persistence-ignorant: no EF attributes or navigation properties.
  Application/
    MyGameCollection.Application.Abstractions   The application's public contract:
                                                - Features/<Feature>/: queries & commands + the models they carry
                                                  (ItemEditModel, ItemDetailsDto, PhotoUpload, BarcodeLookupResult, ...)
                                                - Repositories/ + IPhotoStorage, IBarcodeLookupProvider, IGameMetadataProvider
                                                - ReadModels/ (ItemFilter, ItemListDto, PagedResult, ...)
                                                Depends only on Domain and MediatR.Contracts (Apache-2.0). No EF.
    MyGameCollection.Application                Handlers (one per request), domain mapping of the input models,
                                                barcode parsing, pipeline behaviors (logging, DataAnnotations validation).
  Infrastructure/
    MyGameCollection.Infrastructure.Persistence Data models (ItemRecord, PlatformRecord, ItemPhotoRecord), their EF config,
                                                mappers domain ⇄ data model, repositories, migrations, platform seed.
    MyGameCollection.Infrastructure.Storage     Local-disk photo storage.
    MyGameCollection.Infrastructure.Barcodes    UPCitemdb + Open Products Facts clients.
    MyGameCollection.Infrastructure.Metadata    RAWG client.
  Presentation/
    MyGameCollection.Web                        Blazor Server + MudBlazor UI. Pages only talk to ISender.
tests/
  MyGameCollection.Application.Tests            Unit tests + full-pipeline feature tests (InMemory) + optional Postgres test.
```

**Slices.** Each use case has a request in `Application.Abstractions/Features/<Feature>/` and its handler at the same path in `Application/Features/<Feature>/`. For example, `CreateItemCommand.cs` pairs with `CreateItemHandler.cs`.
- **What's public:** callers (the Web project, tests) only see the contracts. Handlers are `internal` and are discovered by MediatR.
- **Slices never call each other.** Shared logic lives in small internal services instead, such as `GameMetadataEnricher`.

| Feature | Requests |
|---|---|
| Items | `SearchItemsQuery`, `FindItemsByBarcodeQuery`, `GetItemDetailsQuery`, `GetItemForEditQuery`, `CreateItemCommand`, `UpdateItemCommand`, `DeleteItemCommand` |
| Photos | `AddPhotoCommand`, `SetPrimaryPhotoCommand`, `DeletePhotoCommand`, `GetPhotoFileQuery` |
| Platforms | `GetPlatformsQuery`, `CreatePlatformCommand`, `UpdatePlatformCommand`, `DeletePlatformCommand` |
| Barcodes | `LookupBarcodeQuery`, `SearchGameByTitleQuery`, `GetLookupCapabilitiesQuery` |
| Dashboard | `GetDashboardQuery` |

**Domain model and data model.** EF Core only knows the `*Record` classes in Persistence. The repositories load records and map them to domain entities with `ItemMapper` and `PlatformMapper`. On save they map back again, and they work out which photos were added, removed or changed. Read-only screens use projections straight to read models, so they skip the domain model.

**DbContext lifetime.** Each repository call creates its own short-lived `DbContext` from `IDbContextFactory`. Blazor Server keeps DI scopes alive for the whole circuit, so one shared context would run into concurrent-use errors and stale tracked entities.

**Dependencies.** All dependencies point inward:
- Infrastructure projects reference only `Application.Abstractions`.
- `Application` references `Abstractions` and `Domain`. It's the only project that needs the (licensed) MediatR runtime package. `Abstractions` only needs `MediatR.Contracts`.
- `Web` is the only place that wires everything together: `AddApplication`, `AddPersistence`, `AddLocalPhotoStorage`, `AddBarcodeLookup` and `AddRawgMetadata`.

**MediatR licensing.** MediatR 13+ is published by Lucky Penny Software under RPL-1.5 or a commercial license. Its Community license is free for individuals and small companies. Set `MEDIATR_LICENSE_KEY` in `.env` (or `MediatR:LicenseKey` in configuration). Without a key, MediatR still works but logs a warning at startup.

## Running on the home server (Docker)

```bash
cp .env.example .env          # set at least POSTGRES_PASSWORD (and RAWG_API_KEY if you want)
mkdir -p data/photos data/keys
sudo chown -R 1654:1654 data  # the container runs as the non-root "app" user (UID 1654)
docker compose up -d --build
```

The app runs at `http://<server>:8080`. Database migrations and the platform seed run automatically when it starts.

| What | Where |
|---|---|
| Photos | `${DATA_PATH}/photos` on the host (`./data/photos` by default) |
| Data-protection keys | `${DATA_PATH}/keys` |
| Database | Docker volume `pgdata` |

**Backups:** copy the `data/` folder, and dump the database with `docker compose exec db pg_dump -U gamevault mygamecollection > backup.sql`.

### HTTPS (needed for live camera scanning and installing the PWA)

Browsers only allow camera access and service workers on `https://` or `localhost`. Without HTTPS everything still works except live scanning, and the *From photo* option covers that. To get HTTPS you have these options:

- **Existing reverse proxy** (Nginx Proxy Manager, Traefik, Caddy). Point it at port 8080 and enable WebSockets, which Blazor needs. Forwarded headers are already handled.
- **Bundled Caddy**:
  1. Set `SITE_ADDRESS` in `.env` to a hostname that resolves to your server, for example through your router or Pi-hole DNS.
  2. Run `docker compose --profile https up -d`.
  3. With `TLS_MODE=internal`, Caddy uses its own certificate authority. Trust its root certificate on your phone: `docker compose cp caddy:/data/caddy/pki/authorities/local/root.crt .`
  4. For a public domain, set `TLS_MODE` to your e-mail address to get a Let's Encrypt certificate instead.
- **Tailscale** with `tailscale serve` also gives you a valid certificate.

Then open the site on your phone and choose *Add to Home Screen* or *Install app*.

## Development

```bash
docker run -d --name gamevault-dev-db -p 5432:5432 -e POSTGRES_PASSWORD=postgres postgres:17-alpine
dotnet run --project src/MyGameCollection.Web            # uses appsettings.json connection string
dotnet user-secrets set Rawg:ApiKey <key> --project src/MyGameCollection.Web   # optional
```

Migrations:

```bash
dotnet ef migrations add <Name> -p src/MyGameCollection.Infrastructure.Persistence -s src/MyGameCollection.Infrastructure.Persistence -o Migrations
```

Tests (the Postgres test only runs when `MGC_TEST_POSTGRES` is set):

```bash
dotnet test
docker run --rm -d -p 55432:5432 -e POSTGRES_PASSWORD=test postgres:17-alpine
MGC_TEST_POSTGRES="Host=localhost;Port=55432;Username=postgres;Password=test;Database=mgc_test" dotnet test
```

## Notes

- There is **no login**. The app is meant for a trusted home network or behind a VPN. Don't expose it to the internet without adding authentication, for example through your reverse proxy.
- Free barcode databases have patchy coverage for older games, especially PAL releases. If a lookup finds nothing, the barcode is still saved, and the region is guessed from the barcode prefix.
