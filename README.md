# GainsLab

End-to-end playground for GainsLab’s workout content stack. The repo hosts:

- a PostgreSQL-backed sync API,
- shared domain/contract libraries,
- an Avalonia desktop editor with local SQLite caching and an offline outbox.

Everything targets **.NET 9.0** and is wired together through shared interfaces so either side can evolve independently.

---

## Repository Layout

| Path | Description |
| --- | --- |
| `backend/GainsLab.Core` | Domain primitives, identifiers, logging utilities, and shared interfaces. |
| `backend/GainsLab.Contracts` | Sync DTOs, HTTP providers, outbox interceptor, API-facing sync services. |
| `backend/GainsLab.Infrastructure` | EF Core contexts (Postgres + SQLite), DTOs, migrations, seeds, repositories. |
| `backend/GainsLab.Api` | ASP.NET Core host exposing sync endpoints and health checks. |
| `desktop/GainsLab.Editor` | Avalonia desktop client, sync orchestrator, local caches, UI shell. |
| `Core-API-Editor-README.txt` | Deep dive into how the layers fit together. |

---

## Prerequisites

- .NET SDK **9.0.100** or newer.
- PostgreSQL 14+ reachable via `ConnectionStrings:GainsLabDb`.
- SQLite (bundled with .NET) for the editor’s local cache.
- Optional: `dotnet-ef` global tool for managing migrations.

---

## Quick Start

1. **Restore and build**
   ```bash
   dotnet restore GainsLab.sln
   dotnet build GainsLab.sln
   ```

2. **Run database migrations**
   ```bash
   dotnet ef database update \
     --project backend/GainsLab.Infrastructure \
     --startup-project backend/GainsLab.Api \
     --context GainLabPgDBContext
   ```
   The API automatically runs migrations on startup as well, but applying them ahead of time keeps logs clean.

3. **Launch the API**
   ```bash
   dotnet run --project backend/GainsLab.Api/GainsLab.Api.csproj
   ```
   - Environment defaults to `Development`.
   - Connection string is read from `appsettings.Development.json` or user secrets.
   - On boot the app logs migration status and seeds baseline data through `DBDataInitializer`.

4. **Launch the editor (optional)**
   ```bash
   dotnet run --project desktop/GainsLab.Editor/GainsLab.Editor.csproj
   ```
   - The editor seeds a local SQLite DB under `%LOCALAPPDATA%/GainsLab/`.
   - Configure the API base URL via DI or environment (`GAINS_SYNC_BASE_URL`).

---

## Database & Migrations

- **Contexts**
  - `GainLabPgDBContext` (Postgres) — tables `descriptors`, `equipments`, `outbox_changes`, etc.
  - `GainLabSQLDBContext` (SQLite) — mirrors the schema as closely as possible for offline editing.
- **DataAuthority**
  - Both DTOs now expose an `Authority` enum (`Upstream`, `Downstream`, `Bidirectional`) so the server can reject edits originating from the wrong tier.
  - Defaults to `Bidirectional` through code and database defaults; reseed after schema changes.
- **Commands**
  - Add migration: `dotnet ef migrations add <Name> --project backend/GainsLab.Infrastructure --startup-project backend/GainsLab.Api`.
  - Update DB: `dotnet ef database update --context GainLabPgDBContext` (or `GainLabSQLDBContext` for local).

---

## Sync Pipeline Overview

1. Entities implement `ISyncDto` (e.g., `EquipmentSyncDTO`) with metadata such as `UpdatedAtUtc`, `UpdatedSeq`, `Authority`, and tombstone flags.
2. The API registers `ISyncService<T>` for each entity. Each service:
   - Pulls: returns ordered `SyncPage<T>` batches based on a cursor.
   - Pushes: validates payloads, enforces `DataAuthority`, stamps server metadata, persists via EF Core, and writes to the outbox.
3. The desktop editor hosts:
   - `HttpDataProvider` (from `GainsLab.Contracts`) for remote calls.
   - `ISyncEntityProcessor` implementations that materialize DTOs into SQLite entities.
   - An outbox interceptor that normalizes payloads (ignoring timestamps/row versions) to prevent duplicates.

---

## Troubleshooting

- **`relation "public.equipments" does not exist`** — the database was dropped without rerunning migrations. Apply migrations again or delete/recreate the database entirely before restarting the API.
- **Authority warnings during migrations** — EF complains because enum defaults clash with database defaults. Safe to ignore since `BaseDto` sets the value explicitly; suppress by configuring a sentinel or making the column nullable if desired.
- **Duplicate outbox rows** — ensure the database is on the latest schema; the interceptor now normalizes payloads before insert.

---

## Contributing

1. Keep XML documentation up to date when touching shared contracts or domain types.
2. Add/verify migrations for both Postgres and SQLite when changing DTOs.
3. Extend the architecture doc (`Core-API-Editor-README.txt`) when adding new layers or workflows.
4. Prefer functional, isolated changes—API, contracts, editor, and infrastructure all ship together.

Happy hacking!
