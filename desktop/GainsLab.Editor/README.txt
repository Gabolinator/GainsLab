GainsLab.Editor
================

Overview
--------
`GainsLab.Editor` is the Avalonia-based desktop client for managing GainsLab content.
It references the shared Core and Infrastructure projects to access domain models,
caching, and local persistence. Key features include:
- Dependency-injected startup via `AppHost` and `ServiceConfig`.
- Local SQLite storage (seeded through Infrastructure) and optional remote sync.
- Extensible caching and sync processors for desktop data operations.
- An outbox backed by EF Core interceptors that normalize payloads (ignoring timestamps/row versions) so duplicate pushes are avoided.
- Awareness of the `DataAuthority` flag — UI and processors can prevent edits to upstream-owned rows.

Build & Run
-----------
Restore and build the solution from the repo root:

```
dotnet build GainsLab.sln
```

To run the editor directly:

```
dotnet run --project desktop/GainsLab.Editor/GainsLab.Editor.csproj
```

During development you can launch via your IDE (JetBrains Rider, VS, etc.) using the
Avalonia desktop configuration.

Project Layout
--------------
- `Program.cs` / `App.axaml.*` – Avalonia entry point and application bootstrapper.
- `Models/App/` – Dependency injection setup, lifecycle management, system initialization.
- `Models/DataManagement/` – Desktop data manager, file access helpers, sync orchestrator & processors.
- `Models/Utilities/` – Platform utilities (e.g., network checks).
- `MainWindow.axaml.*` – Default UI shell (placeholder UI for now).

Environment
-----------
- Requires .NET SDK 9.0+
- Uses local app-data (`%LOCALAPPDATA%/GainsLab/`) for SQLite DB and sync cursor files.
- Remote sync URL can be configured via `GAINS_SYNC_BASE_URL` environment variable.
- Each row in SQLite now stores `authority`, so clearing/reseeding the database after migrations is the quickest way to adopt new ownership defaults.

Contribution Notes
------------------
1. Keep XML documentation up to date—editor code is the integration surface for multiple subsystems.
2. When adding services, register them in `ServiceConfig.ConfigureServices` and ensure Avalonia DI can resolve them.
3. Test sync workflows against both offline (SQLite) and online (remote API) scenarios; verify that `DataAuthority` is respected in both.
4. Use the existing file/sync abstractions when extending persistence to avoid duplicating logic.
5. Keep the outbox dedup logic up to date when introducing new fields so payload comparisons stay deterministic.
