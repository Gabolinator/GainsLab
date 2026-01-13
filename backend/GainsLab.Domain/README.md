# GainsLab.Domain

Core domain model for GainsLab. This project defines the immutable aggregates, value objects, identifiers, and shared enums used throughout the API, infrastructure, and client applications. Everything targets .NET **9.0** and is referenced by the application/contracts layers.

## Responsibilities
- Provides the canonical `EntityType` enumeration (`CoreEnums.cs`) and related type converters so other layers can reason about descriptors, equipment, muscles, movements, etc.
- Implements strongly-typed identifiers (`MovementId`, `EquipmentId`, `DescriptorId`, …) under `Entities/Identifier` to avoid GUID mix-ups.
- Hosts aggregate roots for descriptors, equipment, muscles, movement categories, movements, and related value objects (tags, notes, media info).
- Defines interfaces for clocks, logging, entity builders, caching registries, seed resolvers, and execution metadata that other projects implement.
- Supplies foundational enums like `DataAuthority`, `eMovementCategories`, `eWorkoutComponents`, and `eBodySection` that drive business logic.

## Build
The domain library is a class library compiled whenever the solution builds:

```bash
dotnet build GainsLab.sln
# or
dotnet build backend/GainsLab.Domain/GainsLab.Domain.csproj
```

## Directory Layout
| Path | Description |
| --- | --- |
| `CoreEnums.cs` / `EnumExtensions.cs` | Entity/category enums plus helpers (e.g., `eMovementCategories.GetDescription()`). |
| `DataAuthority.cs` | Ownership enum used by sync pipelines to gate writes. |
| `Entities/` | Aggregate roots and value objects grouped by concern (`Descriptor/`, `WorkoutEntity/`, `Identifier/`, `CreationInfo/`, etc.). |
| `Interfaces/` | Contracts for clocks, logging, entity builders, caching, seed resolution, and execution metadata. |

## Domain Aggregates & Value Objects
- **Descriptor** (`Entities/Descriptor/`) – `BaseDescriptorEntity` holds descriptions, notes, media info, tags, and audit metadata. `BaseDescriptorContent` encapsulates fields and simple validation.
- **Equipment** (`Entities/WorkoutEntity/EquipmentEntity.cs`) – `EquipmentEntity` couples `EquipmentContent` with descriptors and audit info.
- **Muscle** / **Movement Category** / **Movement** – Rich aggregates capturing antagonists, parent categories, muscle/equipment relationships, and `MovementPersistenceModel` (DB ID lookup tables used during seeding/migrations).
- **Creation/Audit info** (`Entities/CreationInfo/`) – `AuditedInfo` and supporting builders standardize timestamps and creator data for all aggregates.
- **Identifiers** – Every aggregate uses dedicated `record struct` IDs. Utility lists (`EquipmentIdList`, `MovementIdList`) wrap collections of IDs with helper operations.
- **Value objects** – `Description`, `Notes`, `MediaInfos`, `TagList`, `MuscleWorked`, etc. enforce invariants and provide expressive APIs for domain logic.

## Interfaces
- `Interfaces/IClock.cs`, `ILogger.cs` – abstractions consumed by factories, repositories, and background services.
- `Interfaces/Entity/*` – `IEntity`, `IEntityContent<T>`, and `IDescribed<TDescriptor>` describe what it means to be a domain entity and how descriptors attach to aggregates.
- `Interfaces/Builder/` – contracts for constructing entities via fluent builders when seeding or importing.
- `Interfaces/Caching/` – definitions for component caches used by the editor to keep domain projections hot.
- `Interfaces/IEntitySeedResolver.cs` – registry used during seeding to prevent duplicate creation and resolve cross-entity references.
- `Interfaces/IExecutionContent.cs` / `IExecutionDetails.cs` – placeholders for future workout execution metadata.

## DataAuthority & Enums
- `DataAuthority` sits here to keep the enum consistent everywhere (`Upstream`, `Downstream`, `Bidirectional`).
- `eMovementCategories`, `eWorkoutComponents`, and `eBodySection` drive both UI categorization and seeding logic (`EntitySeeder` relies on their string names and descriptions).
- `TypeConverter.GetEntityTye` bridges DTO enum representations back to `EntityType`.

## Extending the Domain
1. Add/modify the value object or aggregate in `Entities/…`.
2. Introduce new enums or identifier types as needed (keep them in `CoreEnums.cs` or `Entities/Identifier`).
3. Update interfaces if new behavior is required (e.g., caching contracts).
4. Reflect the new entity in the application layer (mappers, factories) and infrastructure (EF models/migrations).
5. Ensure any new enums or properties are surfaced through contracts/sync DTOs to maintain parity.

## Usage Notes
- Aggregates use immutable records/value objects; mutating operations typically return copies (`MovementEntity.WithMuscles`, `MovementEntity.WithVariant`, etc.). When working in repositories, respect this pattern to avoid stale state.
- The domain layer has zero dependency on EF Core or ASP.NET—keep it persistence-agnostic so it can power different storage or UI stacks.
- Prefer the domain interfaces (`IEntity`, `IEntityFactory`, etc.) over concrete types when wiring DI; this keeps the API/editor flexible and testable.
