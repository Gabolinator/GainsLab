# GainsLab.Infrastructure

Persistence, sync, and caching infrastructure for GainsLab. This project stitches EF Core contexts, repositories, seeders, sync services, HTTP providers, and outbox plumbing together so both the API and editor can share the same data access layer. Targets .NET **9.0**.

## Responsibilities
- Hosts the PostgreSQL (`GainLabPgDBContext`) and SQLite (`GainLabSQLDBContext`) contexts plus a design-time factory for EF CLI usage.
- Implements repositories, handlers, and mapping utilities that translate between EF entities and domain/application DTOs.
- Exposes sync services (`ISyncService<T>`, `SyncPage<T>`, etc.) used by the API’s `/sync` endpoints and by clients that consume them.
- Provides caches and registries that feed the desktop editor with hot domain projections.
- Includes HTTP clients (`HttpDataProvider`) for calling the API, logging helpers, network checks, and utility classes reused across layers.
- Manages the outbox interceptor/dispatcher so offline mutations can be replayed upstream reliably.

## Build
```bash
dotnet build GainsLab.sln
# or focus on the infrastructure assembly
dotnet build backend/GainsLab.Infrastructure/GainsLab.Infrastructure.csproj
```

## Prerequisites
- .NET SDK **9.0.100**+
- PostgreSQL 14+ for the main API database.
- SQLite 3.x (bundled) for the editor’s local cache.
- `dotnet-ef` global tool if you plan to add migrations.

## Database & Migrations
- `DB/Context/` contains `GainLabPgDBContext`, `GainLabSQLDBContext`, and `GainLabDBContextFactory`. The factory wires connection strings/user secrets for EF CLI.
- `Migrations/` holds root migrations (SQLite) while `Migrations/GainLabPgDB/` tracks Postgres history. Use the matching context when running CLI commands:
  ```bash
  dotnet ef migrations add <Name> \
    --project backend/GainsLab.Infrastructure \
    --startup-project backend/GainsLab.Api \
    --context GainLabPgDBContext
  ```
- `DB/DBDataInitializer.cs` plus `EntitySeedResolver` bootstrap baseline data and resolve cross-entity references during seeds.
- `DB/DataRepository.cs` provides higher-level helpers to orchestrate seeding and ensure both contexts stay in parity.

## Project Layout
| Path | Description |
| --- | --- |
| `DB/` | Contexts, repositories, handlers, seeders, outbox DTOs, and EF exceptions. |
| `Caching/` | Component caches (`EquipmentsCache`, `MusclesCache`, etc.) and registries consumed by the editor. |
| `SyncService/` | `ISyncService` implementations (equipment, descriptor, movement, muscle, movement category) plus shared `SyncCursor`, `SyncPage<T>`, and push envelope types. |
| `Sync/` | Client-side sync orchestrators/registries (e.g., `EntitySyncClient`). |
| `Outbox/` | `OutboxInterceptor` (EF SaveChanges hook) and `OutboxDispatcher` used to deduplicate payloads and push them upstream. |
| `Api/` | Contracts for API-facing HTTP clients plus `IApiClientRegistry` used by `HttpDataProvider`. |
| `HttpDataProvider.cs` | Implementation of `IRemoteProvider`, `IEquipmentProvider`, `IDescriptorProvider`, and `IMuscleProvider` rooted in `HttpClient`. |
| `Utilities/` | Cross-cutting helpers such as `Clock`, `GainsLabLogger`, network utilities, and extension methods. |
| `Logging/` | Thin wrappers (e.g., `GainsLabLogger`) bridging domain logging interfaces with console output. |

## Sync Pipeline Highlights
- Each sync service implements `ISyncService<TSyncDto>` and registers its `EntityType`. Services expose `PullAsync` (ordered by `UpdatedAtUtc` + `UpdatedSeq`) and `PushAsync` (validates `DataAuthority`, stamps server metadata, writes to EF + outbox).
- `SyncController` in the API resolves these services from DI and handles envelope deserialization. Infrastructure types (`SyncCursor`, `SyncPage<T>`, `SyncPushEnvelope`) live here to keep API and clients aligned.
- The desktop/web layers call into `HttpDataProvider`, which fans out to strongly typed APIs (`IEquipmentApi`, `IDescriptorApi`, etc.) registered via `IApiClientRegistry`.

## Caching & Desktop Support
- `Caching/ComponentCacheRegistry.cs` indexes caches per `EntityType` so the Avalonia editor can hydrate view models without re-querying SQLite constantly.
- Individual caches (`EquipmentsCache`, `MovementCache`, `MusclesCache`, …) wrap concurrency primitives and expose lookups keyed by GUID or other identifiers.
- `SyncState.cs` tracks cursor positions and per-entity timestamps so the editor resumes sync efficiently after restarts.

## Outbox & Offline Support
- `OutboxInterceptor` plugs into the EF change pipeline and writes normalized payloads to the `outbox_changes` table whenever tracked entities mutate.
- `OutboxDispatcher` batches unsent rows, POSTs them to the API, and marks them as sent when successful.
- Payload normalization ignores volatility (timestamps, row versions) so duplicate logical changes do not pile up.

## Extending the Infrastructure Layer
1. **New entity type**
   - Add handlers (`DB/Handlers`), EF mappings, and repository implementations (`DB/Repository`).
   - Create sync services + DTO mappers under `SyncService/`.
   - Update caches if the editor needs hot access and wire factories/seeders to populate baseline data.
   - Register everything via DI in the API/editor projects (`DIExtensions`).
2. **New migrations**
   - Use the design-time factory with the correct context.
   - Keep Postgres + SQLite migrations in sync when schema changes affect both.
3. **Transport changes**
   - Wrap new HTTP endpoints in `Api/` client interfaces and expose them through `HttpDataProvider`.
   - Ensure outbox dispatcher/interceptor normalize any new columns or metadata you introduce.

## Development Notes
- Both contexts share DTO classes from `GainsLab.Application`; keep row configuration aligned (default schema, concurrency tokens, `DataAuthority` defaults).
- Logging is provided through `GainsLabLogger` (console) but respects the domain `ILogger` interface so you can plug in Serilog or other sinks later.
- `NetworkChecker` is used defensively by `HttpDataProvider`; when testing offline scenarios, stub it via DI.
- Treat caches as optional optimizations; they can be swapped for fakes when running tests.
