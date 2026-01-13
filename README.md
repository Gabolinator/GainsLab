# GainsLab

End-to-end playground for GainsLab’s workout content stack. The repo ships:

- **GainsLab.Domain** — immutable aggregates, identifiers, enums, and cross-layer interfaces.
- **GainsLab.Contracts** — DTOs + sync interfaces consumed by the API, editor, and web layer.
- **GainsLab.Infrastructure** — EF Core contexts, repositories, seeders, sync/outbox services, HTTP providers.
- **GainsLab.Api** — ASP.NET Core sync API (health checks, CRUD endpoints, migrations).
- **Desktop editor** (`desktop/GainsLab.Editor`) — Avalonia app with SQLite caching/offline outbox.
- **Web layer** (`web/GainsLab.WebLayer`) — Blazor Server data-management front end for CRUD workflows.

Everything targets **.NET 9.0** and composes through shared interfaces so components can evolve independently.

---

## Documentation Map

Each project includes a focused README:

- [`backend/GainsLab.Domain/README.md`](backend/GainsLab.Domain/README.md) – domain aggregates, value objects, interfaces.
- [`backend/GainsLab.Contracts/README.md`](backend/GainsLab.Contracts/README.md) – DTO families, sync interfaces, enums.
- [`backend/GainsLab.Infrastructure/README.md`](backend/GainsLab.Infrastructure/README.md) – contexts, repositories, migrations, caches, outbox, sync services.
- [`backend/GainsLab.Api/README.md`](backend/GainsLab.Api/README.md) – API configuration, endpoints, prerequisites.
- [`backend/GainsLab.Application/README.md`](backend/GainsLab.Application/README.md) – shared application-layer DTOs, mappers, factories, result helpers.
- [`web/GainsLab.WebLayer/README.md`](web/GainsLab.WebLayer/README.md) – Blazor-based management UI configuration and feature set.
- `Core-API-Editor-README.txt` – architectural walkthrough spanning core, API, and desktop editor.

Use this root README for a high-level summary and jump into the project-specific docs when you need deeper implementation details.

---

## Repository Layout

| Path | Description |
| --- | --- |
| `backend/GainsLab.Domain` | Core domain primitives, enums, identifiers, and interfaces (see README for aggregates). |
| `backend/GainsLab.Contracts` | Sync/CRUD DTOs, shared interfaces, and enums consumed across projects. |
| `backend/GainsLab.Application` | DTO records, domain mappers, factories, repository/data-management contracts, result helpers. |
| `backend/GainsLab.Infrastructure` | EF Core contexts (Postgres + SQLite), repositories, migrations, caches, sync/outbox services, HTTP providers. |
| `backend/GainsLab.Api` | ASP.NET Core host exposing sync + CRUD endpoints, migrations, health checks. |
| `desktop/GainsLab.Editor` | Avalonia desktop client with SQLite caching, offline outbox, and sync orchestration. |
| `web/GainsLab.WebLayer` | Blazor Server data-management UI (create/edit/delete entities via the API). |
| `docs/` | Additional design notes and supporting documentation. |

---

## Prerequisites

- .NET SDK **9.0.100** or newer.
- PostgreSQL 14+ reachable via `ConnectionStrings:GainsLabDb`.
- SQLite (bundled) for the editor’s local cache.
- Optional: `dotnet-ef` global tool when authoring migrations.
- For the web layer/editor, a running API base URL (defaults to `https://localhost:5001/` or override via `GAINS_SYNC_BASE_URL`).

---

## Quick Start

1. **Restore and build everything**
   ```bash
   dotnet restore GainsLab.sln
   dotnet build GainsLab.sln
   ```

2. **Prepare the database**
   ```bash
   dotnet ef database update \
     --project backend/GainsLab.Infrastructure \
     --startup-project backend/GainsLab.Api \
     --context GainLabPgDBContext
   ```
   The API also migrates on startup, but running this explicitly keeps logs tidy and surfaces schema issues early.

3. **Launch the API**
   ```bash
   dotnet run --project backend/GainsLab.Api/GainsLab.Api.csproj
   ```
   - Defaults to the `Development` environment and loads `ConnectionStrings:GainsLabDb` from appsettings or user secrets.
   - Logs connection info, migration status, and seeding operations via `GainsLabLogger`.

4. **Launch a client**
   - **Desktop editor**
     ```bash
     dotnet run --project desktop/GainsLab.Editor/GainsLab.Editor.csproj
     ```
     Creates a SQLite cache under `%LOCALAPPDATA%/GainsLab/`, hydrates local repositories, and syncs via `HttpDataProvider`.
   - **Web layer**
     ```bash
     dotnet run --project web/GainsLab.WebLayer/GainsLab.WebLayer.csproj
     ```
     Ensure `GAINS_SYNC_BASE_URL` points at the API (falls back to `https://localhost:5001/`). Provides CRUD dashboards for equipment, descriptors, muscles, and movement categories.

---

## Database & Migrations

- **Contexts**
  - `GainLabPgDBContext` (Postgres) — main API database (tables: `descriptors`, `equipments`, `movement_*`, `outbox_changes`, etc.).
  - `GainLabSQLDBContext` (SQLite) — mirrors schema locally for the editor.
- **Authority & audit metadata**
  - `DataAuthority` defaults to `Bidirectional` to gate upstream/downstream writes. Ensure migrations + seeds keep defaults in sync.
- **Common commands**
  - Add migration:
    ```bash
    dotnet ef migrations add <Name> \
      --project backend/GainsLab.Infrastructure \
      --startup-project backend/GainsLab.Api \
      --context GainLabPgDBContext
    ```
  - Update Postgres:
    ```bash
    dotnet ef database update --context GainLabPgDBContext
    ```
  - Update SQLite:
    ```bash
    dotnet ef database update --context GainLabSQLDBContext
    ```
  - See the infrastructure README for more details about seeds, context factories, and migration layout.

---

## Sync & Data Flow Snapshot

1. **Contracts** — All entities implement `ISyncDto` (e.g., `EquipmentSyncDTO`) exposing GUIDs, timestamps, `UpdatedSeq`, tombstone flags, and `DataAuthority`.
2. **API services** — Each entity registers an `ISyncService<T>` that returns ordered `SyncPage<T>` slices and validates push payloads before persisting and enqueueing outbox changes.
3. **Infrastructure** — `HttpDataProvider`, repositories, and caches coordinate remote pulls, local persistence, and outbox dispatch. `SyncState` remembers per-entity cursors.
4. **Clients** — Desktop and web layer resolve providers/gateways to hydrate UI state, trigger saves, and show toast/confirmation messages. Both can seed local state using the shared sync pipeline.

---

## Troubleshooting & Tips

- **`relation "public.equipments" does not exist`** — Run migrations again (Postgres database likely dropped).
- **Authority warnings during migrations** — EF default clashes are safe to ignore because the DTO base sets explicit values; adjust migrations if noise becomes problematic.
- **Duplicate outbox rows** — Ensure the app + database are on matching migrations; the interceptor normalizes payloads but old schemas can still create duplicates.
- **Desktop/Web cannot reach API** — Check `GAINS_SYNC_BASE_URL`, ensure HTTPS dev certificates are trusted, and verify `NetworkChecker` is satisfied (logs appear in console via `GainsLabLogger`).

---

## Contributing

1. Touch the smallest layer that solves the problem—API, contracts, application, domain, infrastructure, and clients all rely on each other.
2. Update XML docs/comments when altering shared interfaces or DTOs; downstream IntelliSense depends on them.
3. Keep Postgres + SQLite migrations in lockstep when schema changes affect both contexts.
4. Extend `Core-API-Editor-README.txt` or the project-specific READMEs when adding new workflows or layers so future contributors can follow along.

Happy hacking!
