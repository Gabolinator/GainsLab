GainsLab Core / API / Editor Overview
=====================================

This document introduces the three main layers in the GainsLab solution—the shared Core models, the backend Sync API, and the desktop Editor—and explains how they fit together in the current implementation.


Solution Layout
---------------
- `backend/GainsLab.Core` – Domain abstractions, DTOs, and shared utilities that power both the API and client layers.
- `backend/GainsLab.Infrastructure` – Entity Framework Core persistence, database DTOs, and sync state storage.
- `backend/GainsLab.Contracts` – Synchronisation contracts (DTOs, services) plus the HTTP data provider consumed by clients.
- `backend/GainsLab.Api` – ASP.NET Core service that exposes sync endpoints backed by PostgreSQL.
- `desktop/GainsLab.Editor` – Avalonia-based desktop client that orchestrates seeding, local caching, and authoring workflows.

The entire solution targets .NET 9.0.


Core Models
-----------
Location: `backend/GainsLab.Core`

Key responsibilities:
- **Domain primitives** – Enumeration types, identifiers, component lists, and other foundational models under `Models/Core`.
- **Synchronisation contracts** – Interfaces such as `ISyncPage`, `ISyncCursor`, `ISyncDto`, `IDataManager`, `ILocalRepository`, and `IRemoteProvider` define how data moves between remote services, local storage, and in-memory caches.
- **Caching interfaces** – `IComponentCacheRegistry` and related contracts coordinate component-level caching within the editor.
- **Result helpers** – `Result` and `ResultList` types provide a consistent success/error pattern across layers.
- **Utilities** – Logging, clock abstractions, and miscellaneous helpers that allow the API and Editor to stay decoupled from concrete implementations.

Together these models let any client build upon a consistent domain vocabulary while swapping out infrastructure details as needed.


Sync API
-------
Location: `backend/GainsLab.Api`

Highlights:
- Built on ASP.NET Core with automatic Swagger UI in development.
- Relies on Entity Framework Core (`GainLabPgDBContext`) to persist descriptors and equipment to PostgreSQL (default schema `public`).
- On startup the API:
  1. Loads the `GainsLabDb` connection string (stored in `appsettings.Development.json` or user secrets).
  2. Runs pending EF Core migrations and logs migration status.
  3. Seeds baseline data through `DBDataInitializer`.
- Exposes the health check endpoint `GET /healthz`.
- Registers `DescriptorSyncService` and `EquipmentSyncService`, each implementing `ISyncService<TDto>`.
- Primary sync endpoint: `GET /sync/{entity}?ts=<timestamp>&seq=<seq>&take=<n>` which streams incremental changes for the requested `EntityType`. Responses include a `SyncCursor` for pagination and a strongly typed list of DTOs.

Prerequisites:
- PostgreSQL instance reachable with a connection string named `GainsLabDb`.
- Migrations run automatically; ensure the configured user has create/update permissions.

Running locally:
```
dotnet restore GainsLab.sln
dotnet run --project backend/GainsLab.Api/GainsLab.Api.csproj
```
Set `ASPNETCORE_ENVIRONMENT=Development` (default in Program.cs) and supply secrets with `dotnet user-secrets` if you prefer not to commit the connection string.


Desktop Editor
--------------
Location: `desktop/GainsLab.Editor`

Highlights:
- Cross-platform UI built with Avalonia (`App.axaml`, `MainWindow.axaml`).
- The `DataManager` orchestrates lifecycle events:
  - Creates a local data directory under `%LOCALAPPDATA%\GainsLab\Files`.
  - Subscribes to application exit events via `IAppLifeCycle` to flush data to disk.
  - Performs an initial seed through `ISyncOrchestrator`, storing cursors in `SyncState`.
  - Uses `IComponentCacheRegistry` to hydrate in-memory caches from local storage.
- `ISyncOrchestrator` coordinates remote pulls via an `IRemoteProvider` (current implementation: `HttpDataProvider`) and pushes local outbox mutations when implemented.
- `IDataManager` exposes methods to load/cache data, resolve components, and persist changes—providing a single façade for editor view models.

Running locally:
```
dotnet run --project desktop/GainsLab.Editor/GainsLab.Editor.csproj
```
The editor expects the API to be reachable; configure the base address for `HttpDataProvider` via DI or the hosting container (see `desktop` project composition for details). Ensure the API is running before starting the editor to allow the initial seed.


Data Flow Summary
-----------------
1. **API** exposes descriptor and equipment sync streams backed by PostgreSQL.
2. **HttpDataProvider** (in `GainsLab.Contracts`) calls the API, materialising `SyncPage<DescriptorSyncDto>` and `SyncPage<EquipmentSyncDto>` payloads.
3. **SyncOrchestrator** drives seed and delta syncs, delegating to entity-specific processors (plug-in model via `ISyncEntityProcessor`).
4. **ILocalRepository** persists data locally and tracks `SyncState` so seeds/deltas can resume.
5. **DataManager** waits for seeding, loads persisted data, populates caches, and exposes CRUD helpers to the Avalonia UI.


Development Tips
----------------
- Restore and build once before running any layer: `dotnet restore GainsLab.sln`.
- For database schema changes, add EF Core migrations in `backend/GainsLab.Infrastructure` and ensure `GainLabPgDBContext` picks them up at API startup.
- XML documentation in the core interfaces explains the intent of each contract; use these summaries when implementing adapters.
- The solution includes multiple projects targeting .NET 9; install the .NET 9 SDK or use the matching container/toolchain.
- When extending sync to new entities, create matching DTOs (`SyncDto`), register a new `ISyncService<T>` in the API, implement processors in the editor, and update cursors/state handling.


Directory Quick Reference
-------------------------
- `backend/GainsLab.Core/Models/Core` – Enumerations, identifiers, component utilities.
- `backend/GainsLab.Core/Models/Core/Interfaces` – Abstractions for data management, caching, sync, logging.
- `backend/GainsLab.Contracts/SyncService` – DTOs, REST client (`HttpDataProvider`), sync service interfaces, controller.
- `backend/GainsLab.Infrastructure` – EF Core DB context, DTOs, seeders, sync state storage.
- `desktop/GainsLab.Editor/Models/DataManagement` – `DataManager`, sync orchestrator, local data strategies.
- `desktop/GainsLab.Editor/App.axaml` – Avalonia application shell.


Next Steps
----------
- Flesh out TODO sections in `DataManager` (entity resolution, persistence, file export).
- Implement additional sync processors and expose new endpoints as domain entities expand.
- Document configuration (API base URL, local storage paths) once their hosting story is finalised.
