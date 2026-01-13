# GainsLab.Contracts

Cross-project contract library that standardizes the DTOs, sync cursors, and result enums used across the GainsLab API, infrastructure, desktop editor, and future clients. Everything targets .NET **9.0** and is imported via project references.

## Responsibilities
- Declares every DTO exchanged between layers: request/response models for CRUD plus the sync payloads used by offline workflows.
- Provides shared interfaces (`IRecord`, `IVersionRecord`, `ISyncPage`, `ISyncDto`, etc.) so infrastructure and clients can reason about records without tight coupling.
- Centralizes enum-based protocols (update outcomes, create/delete requests) to keep API/controllers/editor logic aligned.
- Supplies typed ID helpers so callers can reference entities by GUID or name when supported.

## Build
Contracts ship as part of the main solution—there is no standalone executable even though the SDK template includes `Program.cs`.

```bash
dotnet build GainsLab.sln
# or focus on just this library
dotnet build backend/GainsLab.Contracts/GainsLab.Contracts.csproj
```

## Directory Layout
| Path | Description |
| --- | --- |
| `Dtos/` | Families of DTOs grouped by verb (`GetDto/`, `PostDto/`, `PutDto/`, `UpdateDto/`, `Delete/`, `SyncDto/`). Also includes `ID/` helpers for referencing entities. |
| `Interface/` | Shared abstractions such as `IRecord`, `IVersionRecord`, `ISyncDto`, `ISyncCursor`, and `ISyncPage`. |
| `Enums.cs` | Outcome/request enums (`UpdateOutcome`, `CreateRequest`, `UpsertOutcome`, etc.) used throughout the API and editor flows. |
| `Program.cs` | Template stub (unused). |

## DTO Families
- **Sync DTOs (`Dtos/SyncDto/`)** – Lightweight records (`EquipmentSyncDTO`, `DescriptorSyncDTO`, `MovementSyncDto`, etc.) that flow through `/sync` endpoints. They always expose the entity GUID, optional descriptor references, audit metadata (`UpdatedAtUtc`, `UpdatedSeq`), soft-delete flags, and `DataAuthority`.
- **REST read models (`Dtos/GetDto/`)** – Shape returned by CRUD endpoints. Mirrors sync metadata so caches can stay coherent.
- **Write models** – `PostDto/`, `PutDto/`, and `UpdateDto/` capture creation, replacement, and partial-update payloads respectively. Validation attributes (e.g., `[Required]`, `[StringLength]`) signal what controllers expect.
- **Outcome + request DTOs** – Under `Dtos/Delete/` and `Dtos/UpdateDto/Outcome/` sit strongly typed responses describing what actually happened (`EquipmentDeleteOutcome`, `DescriptorUpdateOutcome`, `EquipmentUpdateCombinedOutcome`, etc.).
- **Entity IDs (`Dtos/ID/EntityId.cs`)** – Utility records allowing clients to submit either a GUID or a name (future-friendly) when referencing another entity. Includes helpers such as `IsIdValid()` and `IsValid()`.

## Interfaces
- `IRecord`/`IVersionRecord` define the minimum persistence metadata required by repositories (GUID + DbId + timestamps, soft-delete markers, sequence ordering).
- `ISyncDto`, `ISyncCursor`, and `ISyncPage<T>` describe the paging contract that both the API and HTTP clients implement. Controllers accept `SyncPushEnvelope` instances (see API project) and return types implementing these interfaces.
- `IMessagesContainer` is a marker interface implemented on the application layer so result models can surface user-facing info without leaking concrete implementations back into contracts.

## Enums & Outcome Modeling
`Enums.cs` gathers all of the shared enums used by DTOs:
- `UpdateRequest`, `CreateRequest` – used by composite payloads to indicate whether a nested resource should be touched.
- `UpsertOutcome`, `UpdateOutcome`, `DeleteOutcome`, `CreateOutcome` – reflected back to callers to explain if the write actually created, updated, skipped, or failed.

Keep these enums in sync with the application- and infrastructure-layer logic; controllers and repositories often branch directly on them.

## Usage by Other Layers
- **API (`backend/GainsLab.Api`)** – Controllers consume `PostDto/PutDto/UpdateDto` types for validation and return `GetDto`/`UpdateOutcome` records. Sync endpoints emit the `SyncDto` records.
- **Infrastructure** – Implements repositories that read/write `IRecord` + `IVersionRecord` DTOs, stamps audit metadata, and materializes `ISyncPage<T>` for API responses.
- **Desktop editor / web layer** – Uses the same DTOs when calling the API through `HttpClient` wrappers, ensuring serialization remains consistent.

## Adding a New Entity Contract
1. Create sync/read/write DTOs under the appropriate `Dtos/<Verb>/` folders.
2. Add any supporting enums/outcomes (or extend existing ones) in `Enums.cs`.
3. If the entity needs a dedicated ID helper, add it under `Dtos/ID/`.
4. Update repositories/controllers in consuming projects to reference the new DTOs and ensure their DI registrations are updated.

## Development Notes
- Keep DTOs immutable records where possible so equality checks during sync/outbox comparisons remain cheap.
- Always include `DataAuthority`, `UpdatedAtUtc`, and `UpdatedSeq` on new sync DTOs; the API enforces them for conflict detection.
- Bump documentation comments when fields change—both controllers and UI clients rely on XML docs generated from this project for intellisense.
