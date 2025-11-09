GainsLab.Contracts
===================

Overview
--------
`GainsLab.Contracts` packages the shared contracts that connect the desktop editor,
the sync API, and the backing infrastructure. It exposes:
- DTOs used by the sync pipeline (`SyncDto/`). Every DTO now carries `UpdatedAtUtc`, `UpdatedSeq`, tombstone metadata, and a `DataAuthority` flag so pushes can be gated by ownership.
- Interfaces consumed by the editor and server (`Interface/`).
- HTTP/persistence helpers such as the outbox dispatcher (`Outbox/`) and remote provider.
- The sync controller and services that back the API (`SyncService/`).

These contracts compile into a .NET 9 project; other projects reference it directly via
project references.

Build & Test
------------
The Contracts project has no standalone entry point. Build it via the solution:

```
dotnet build GainsLab.sln
```

Or target the project explicitly:

```
dotnet build backend/GainsLab.Contracts/GainsLab.Contracts.csproj
```

Because this assembly mostly defines shared types, tests live in the consuming
projects rather than here.

Key Directories
---------------
- `Interface/` – service abstractions such as `ISyncService`.
- `SyncDto/` – DTOs exchanged during synchronization.
- `SyncService/` – ASP.NET controllers and service implementations for sync endpoints.
- `Outbox/` – EF interceptors and dispatchers for outbox processing (including payload normalization to prevent duplicate rows).
- `HttpDataProvider.cs` – desktop-side remote provider hitting the sync API.

Prerequisites
-------------
- .NET SDK 9.0+
- Local PostgreSQL connection if you intend to run the API that consumes these contracts.

Usage Tips
----------
1. Keep DTOs immutable and versioned to minimize breaking changes.
2. Update XML documentation alongside contracts to maintain clarity.
3. When adding new entity types, implement an `ISyncService<T>` and wire it into DI.
4. Remember to propagate `DataAuthority` (and any new sync metadata) through DTOs, mappers, and processors simultaneously to avoid schema drift.
