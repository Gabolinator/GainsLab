# GainsLab Sync API

ASP.NET Core host that powers GainsLab’s synchronization surface for editor clients. The API fronts the shared infrastructure project, executes EF Core migrations against PostgreSQL on boot, and exposes both coarse-grained sync endpoints and CRUD endpoints for descriptors/equipment.

## Features
- Targets .NET **9.0**, wired through the shared GainsLab Core/Application/Infrastructure layers.
- PostgreSQL-backed `GainLabPgDBContext` with automatic migrations on startup.
- Entity-agnostic sync pipeline (`ISyncService<TSyncDto>`) that currently supports descriptors, equipment, movement categories, muscles, and movements.
- Conventional REST controllers for descriptors and equipment in addition to `/sync` bulk endpoints.
- Swagger UI + minimal `/healthz` endpoint enabled whenever the environment is `Development`.

## Prerequisites
- .NET SDK **9.0.100** or newer.
- PostgreSQL 14+ with a database the API user can migrate/seeds.
- The connection string stored under `ConnectionStrings:GainsLabDb`.

## Configuration
`Program.cs` forces `ASPNETCORE_ENVIRONMENT=Development` for now so that Swagger and relaxed HTTPS policies are always enabled locally. Provide a PostgreSQL connection string via `appsettings.Development.json` or user secrets:

```json
{
  "ConnectionStrings": {
    "GainsLabDb": "Host=localhost;Port=5432;Database=gainslab;Username=postgres;Password=postgres;SSL Mode=Disable"
  }
}
```

User secret example (from the repo root):

```bash
dotnet user-secrets set "ConnectionStrings:GainsLabDb" "Host=localhost;Port=5432;Database=gainslab;Username=postgres;Password=postgres;SSL Mode=Disable" \
  --project backend/GainsLab.Api/GainsLab.Api.csproj
```

## Build & Run
1. Restore/build the whole solution so the API and its sibling projects compile:
   ```bash
   dotnet restore GainsLab.sln
   dotnet build GainsLab.sln
   ```
2. Apply pending migrations (the API also does this on boot, but running it explicitly keeps logs quiet and fails early):
   ```bash
   dotnet ef database update \
     --project backend/GainsLab.Infrastructure \
     --startup-project backend/GainsLab.Api \
     --context GainLabPgDBContext
   ```
3. Run the API:
   ```bash
   dotnet run --project backend/GainsLab.Api/GainsLab.Api.csproj
   ```
   Startup logs will include the connection string hash, migration status, and connectivity checks produced by `GainsLabLogger`.

## Database & Seeding
- `AppExtension.RunApplicationAsync` resolves `GainLabPgDBContext`, applies migrations, and logs pending/applied migration names.
- The same startup scope resolves `IEntitySeedResolver` and `DBDataInitializer`. The initializer can populate baseline descriptor/equipment data; the `CreateBaseEntities` call is currently commented out while seeding is being migrated to the desktop “data management console.”
- All repositories (`DescriptorRepository`, `EquipmentRepository`, etc.) live in `backend/GainsLab.Infrastructure` and are registered via `ConfigureServicesPostDBContext`.

When adding new entities:
1. Add sync DTOs + EF handlers in `GainsLab.Infrastructure`.
2. Register the repository/service pair in `DIExtensions`.
3. Create or update controllers if the entity needs direct CRUD endpoints.

## HTTP Surface
All routes are relative to the API root (defaults to `https://localhost:5001` when using Kestrel):

| Method | Route | Description |
| --- | --- | --- |
| GET | `/healthz` | Simple readiness probe returning `{ ok: true }`. |
| GET | `/swagger` | Swagger UI/JSON thanks to `AddSwaggerGen` when env is Development. |
| GET | `/sync/{entity}?ts=<iso8601>&seq=<long>&take=<1-500>` | Streams a `SyncPage<T>` for the requested `EntityType` (`Equipment`, `Descriptor`, `MovementCategory`, `Movement`, `Muscle`). Cursor defaults to the beginning. |
| POST | `/sync/{entity}` | Pushes a batch of DTO envelopes for the entity. Accepts a `SyncPushEnvelope` whose `items` are deserialized to the target `ISyncDto` subtype before invoking the registered `ISyncService`. |
| GET | `/descriptions/sync` | Convenience endpoint that proxies to the descriptor sync service when clients don’t need the entity switchboard. |
| GET | `/descriptions/{id}` | Reads a descriptor by id via `IDescriptorRepository`. Returns 404 when missing, 400 when the id is empty. |
| POST | `/descriptions` | Creates a descriptor from `DescriptorPostDTO`. Responds with the created resource location via `APIResultValidation`. |
| PUT | `/descriptions/{id}` | Replaces a descriptor with a `DescriptorPutDTO`. |
| PATCH | `/descriptions/{id}` | Applies partial updates with `DescriptorUpdateDTO`; returns an `DescriptorUpdateOutcome`. |
| GET | `/equipments/sync` | Same as the descriptor sync endpoint but for equipment DTOs. |
| GET | `/equipments/{id}` | Fetches a single equipment row. |
| POST | `/equipments` | Creates equipment from `EquipmentPostDTO`. |
| PUT | `/equipments/{id}` | Full replace with `EquipmentPutDTO`. |
| PATCH | `/equipments/{id}` | Partial patch using `EquipmentUpdateDTO`. |
| DELETE | `/equipments/{id}` | Deletes an equipment row using the repository. |

`SyncController` automatically validates the `entity` segment against `EntityType` and clamps `take` between 1 and 500, so malformed requests fail with `404` or `400` before hitting EF Core.

## Development Tips
- `DIExtensions.ConfigureServicesPreDBContext` registers `ILogger`, `IClock`, MVC controllers, and Swagger. Anything required before `GainLabPgDBContext` is available should be placed here.
- `ConfigureServicesPostDBContext` is where repositories and infrastructure singletons belong. Keep PostgreSQL/SQLite-specific services in the infrastructure project.
- Use `curl` or `httpie` to verify sync responses. Example:
  ```bash
  curl "https://localhost:5001/sync/Equipment?take=50"
  ```
- When authoring new migrations, continue using `backend/GainsLab.Infrastructure` as the migration project with `backend/GainsLab.Api` as the startup project so the Postgres context is discovered.

With those basics in place you can extend the sync surface, wire new controllers, or host the API behind your preferred reverse proxy/container platform.
