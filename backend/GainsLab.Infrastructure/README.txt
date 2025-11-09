GainsLab.Infrastructure
=======================

Overview
--------
`GainsLab.Infrastructure` implements persistence, caching, and outbox plumbing for the
GainsLab desktop/editor applications. The project targets .NET 9 and includes:
- EF Core DbContexts for both local SQLite storage and remote Postgres (`DB/Context`).
- Repository and handler classes that map domain entities to DTOs and vice-versa (`DB/Handlers`, `DB/DomainMappers`).
- Shared DTO base types that now include the `DataAuthority` column for every synced entity.
- Outbox support for reliable upstream sync (`DB/Outbox`).
- In-memory caches that back the editor’s data layer (`Caching/`).
- Migration history for SQLite/Postgres (`Migrations/`).

Build
-----
Compile through the root solution:

```
dotnet build GainsLab.sln
```

Or target the infrastructure project directly:

```
dotnet build backend/GainsLab.Infrastructure/GainsLab.Infrastructure.csproj
```

Migrations
----------
- Local SQLite migrations live under `Migrations/GainLabSQLDB/`.
- Postgres migrations reside in the root `Migrations/`.
- Use the design-time factory (`DB/Context/GainLabDBContextFactory.cs`) when running EF Core CLI commands.
- Reseed both databases after schema updates that introduce new defaults (e.g., `authority`), or delete/recreate them if you prefer a clean slate.

Key Components
--------------
- `DB/DataRepository.cs` – orchestrates local persistence via EF Core handlers.
- `DB/DBDataInitializer.cs` – seeds baseline data (e.g., default equipment).
- `SyncState.cs` – serializable sync state shared with other layers.
- `Caching/` – component caches keyed by `EntityType` for quick lookups.
- `DB/Outcomes.cs` – helper records describing seed/delta sync results.

Prerequisites
-------------
- .NET SDK 9.0+
- SQLite (for local desktop storage).
- PostgreSQL (when running the server that consumes this infrastructure).

Contribution Notes
------------------
1. Keep XML documentation current; infrastructure types are widely shared.
2. When adding new entity types, implement the corresponding handler, mapper, cache, and make sure both contexts/migration trees stay in parity.
3. Prefer pure EF Core code—avoid direct SQL unless necessary, and keep migrations in sync with changes.
4. Verify seeds and migrations on both SQLite and Postgres to avoid drift; `DataAuthority` defaults should match across engines.
