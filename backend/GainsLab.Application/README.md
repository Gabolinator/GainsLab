# GainsLab.Application

Shared application layer that glues the GainsLab domain models, contracts, and infrastructure together. It defines DTOs persisted by EF Core, factories used for seeding, repository/data-management interfaces, sync contracts, and the result helpers consumed by both the API and desktop editor.

## Responsibilities
- Targets .NET **9.0** and references `GainsLab.Domain` + `GainsLab.Contracts`.
- Provides canonical records/DTOs used by EF Core (`BaseRecord`, `EquipmentRecord`, etc.).
- Converts between persistence records and domain entities through mapper extensions.
- Hosts factories/seeders that build strongly-typed domain aggregates from minimal input (used by migrations and bootstrap tasks).
- Defines interfaces for repositories, gateways, providers, data managers, lifecycle hooks, sync, and outbox dispatching.
- Supplies the `Result`/`APIResult` primitives so controllers and services can return consistent responses.

## Build
The project is a class library compiled as part of the solution:

```bash
dotnet build GainsLab.sln
# or
dotnet build backend/GainsLab.Application/GainsLab.Application.csproj
```

## Directory Layout
| Path | Purpose |
| --- | --- |
| `DTOs/` | Base record plus entity-specific DTOs (`Description/`, `Equipment/`, `Movement/`, `Muscle/`, etc.) and helpers under `Extensions/`. |
| `DomainMappers/` | `EntityDomainMapper` and entity-specific mappers that translate between domain entities and DTO records. |
| `EntityFactory/` | Factories for descriptors/equipment/muscles/movements, plus `EntitySeeder` for bootstrap data. |
| `Interfaces/` | Contract surface split across APIs, data management, sync, providers, repositories, gateways, and lifecycle hooks. |
| `Outbox/` | `IOutboxDispatcher` for pushing the offline outbox to the API. |
| `Results/` | `Result`, `ResultList`, `MessagesContainer`, `APIResult<T>`, and `APIResultValidation` helpers. |
| `IDBHandler.cs` | Abstraction for persistence handlers that materialize `IRecord` instances and expose CRUD helpers. |

## Key Abstractions

### DTOs & Records
- `DTOs/BaseDto.cs` defines `BaseRecord`, the shared metadata shape (audit stamps, `DataAuthority`, row-version, tombstone flags).
- Each entity folder (`DTOs/Equipment`, `DTOs/Movement`, etc.) supplies records that implement `IRecord`/`IVersionRecord`, request/response shapes, and conversion helpers used by repositories and sync services.
- `DTOs/Extensions` contains mapper helpers (e.g., converting contracts to records or vice versa) so infrastructure projects can stay lean.

### Domain Mapping
- `DomainMappers/EntityDomainMapper.cs` exposes `ToRecord`/`ToDomain` extension methods that fan out to entity-specific mappers (`EquipmentMapper`, `MovementMapper`, etc.). This keeps domain logic in `GainsLab.Domain` while allowing EF Core to persist flattened DTOs.
- Each mapper knows how to hydrate nested value objects (muscle lists, descriptor content, persistence metadata) to guarantee parity between in-memory aggregates and database rows.

### Factories & Seeders
- `EntityFactory/EntityFactory.cs` orchestrates creation of descriptors, equipment, muscles, movement categories, and movements. It composes the lower-level factories (`DescriptorFactory`, `MovementFactory`, etc.), ensuring audit info, descriptors, and seed resolver tracking stay consistent.
- `EntitySeeder.cs` produces baseline entities (muscles, movements, categories, equipment) and resolves relationships like antagonists or movement variants by consulting the shared `IEntitySeedResolver`.
- Factories require `IClock`, `ILogger`, `IDescriptorService<BaseDescriptorEntity>`, and `IEntitySeedResolver` so seeds can stamp timestamps, log progress, and avoid duplicate creation.

### Data Management & Repository Contracts
- `Interfaces/DataManagement/` defines the contracts consumed by the Avalonia editor: `IDataManager`, `IDataProvider`, repositories, local storage providers, file export services, gateways (`IDescriptorGateway`, `IEquipmentGateway`, etc.), and provider abstractions (`IDescriptorProvider`, ...).
- `IDBHandler` centralizes persistence operations for lists of `IRecord`—repositories in infrastructure implement it to add/update/delete rows and return domain projections.
- `Interfaces/ILifeCycle.cs` exposes start/exit events so UI hosts can flush caches or trigger sync before shutdown.
- `Interfaces/IDescriptorRepository.cs`, `IEquipmentRepository.cs`, and related APIs define the shape of the repository layer that the ASP.NET controllers consume.

### API & Sync Contracts
- `Interfaces/IEntityApi`, `IReadApi`, and `IWriteApi` represent HTTP surface abstractions (read-by-id + write operations) for any entity’s sync DTOs.
- `Interfaces/Sync/` exposes `IEntitySyncClient`, `IRemoteProvider`, and `ISyncState` so clients can orchestrate push/pull flows agnostic of transport.
- `Outbox/IOutboxDispatcher` and the sync contracts ensure local mutations are queued and retried upstream through a consistent interface.

### Result & Validation Helpers
- `Results/Result.cs` and `Result<T>` provide the common success/failure pattern with `MessagesContainer` for errors/info. `ResultList` adds convenience for bulk operations.
- `Results/APIResults/` extends the base result type with HTTP-aware statuses (`ApiResultStatus`) and includes `APIResultValidation` for turning service responses into ASP.NET `IActionResult`s with the correct codes/location headers.

## Extending the Application Layer
1. **New entity support**
   - Create DTO/record definitions under `DTOs/<Entity>/`.
   - Implement a mapper in `DomainMappers/<Entity>Mapper.cs` and wire it into `EntityDomainMapper`.
   - Add factory/seed logic if needed in `EntityFactory/`.
   - Define repository/provider/gateway interfaces (or extend existing ones) inside `Interfaces/`.
   - Update infrastructure implementations to satisfy the new contracts, then register them in the API/editor DI containers.
2. **Expose new API endpoints**
   - Return `Result`/`APIResult<T>` from your services and run them through `APIResultValidation` in your controller for standardized responses.
   - If the endpoint needs sync semantics, expose it through `IEntityApi` so both desktop and mobile clients can share implementation logic.
3. **Outbox/sync work**
   - Anything that needs to participate in offline sync should use `IDBHandler` to persist `IRecord`s with updated audit stamps and call into `IOutboxDispatcher` after local saves succeed.

## Usage From Other Projects
- **API** (`backend/GainsLab.Api`) references this project for repositories (`IDescriptorRepository`, `IEquipmentRepository`), DTOs, and `APIResult` helpers.
- **Infrastructure** implements the interfaces defined here (`IDBHandler`, repositories, `IEntitySeedResolver`) while reusing the shared DTOs.
- **Desktop editor** relies on `IDataManager`, gateway/provider interfaces, lifecycle hooks, and sync contracts to orchestrate seeding, caching, and outbox dispatching.

## Development Notes
- Keep `DataAuthority`, `UpdatedSeq`, and audit fields on DTOs accurate—sync services assume the application layer has already stamped local state correctly.
- When adding new factories or mappers, update `IEntitySeedResolver` usage so seeds remain idempotent.
- Favor the `Result` helpers over throwing exceptions for expected validation issues; API controllers and desktop workflows already understand how to surface these results to users.
