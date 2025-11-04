GainsLab.Core
==============

Overview
--------
`GainsLab.Core` hosts the domain model, shared interfaces, and cross-cutting utilities
that power the GainsLab editor and API. The project targets .NET 9 and exposes types such as:
- Domain entities (equipment, movements, muscles) under `Models/Core/Entities` and `Models/WorkoutComponents`.
- Audit/value objects (`Models/Core/CreationInfo`, `Models/Core/Descriptor`).
- Service contracts used across the solution (`Models/Core/Interfaces`).
- Result helpers, logging, and singleton utilities (`Models/Core/Results`, `Models/Core/Utilities`).

Other projects (editor, infrastructure, contracts) reference this library to share
domain definitions and infrastructure abstractions.

Build
-----
Build the project via the root solution:

```
dotnet build GainsLab.sln
```

To target just the core library:

```
dotnet build backend/GainsLab.Core/GainsLab.Core.csproj
```

Key Directories
---------------
- `Models/Core/Entities` – aggregate roots and supporting identifier types.
- `Models/Core/Interfaces` – contracts for repositories, caches, factories, and DTOs.
- `Models/Core/Results` – `Result`, `Result<T>`, and result list helpers used throughout the stack.
- `Models/Core/Utilities` – shared services such as `Clock` and `GainsLabLogger`.
- `Models/WorkoutComponents` – domain objects representing workout elements exposed via the UI.

Guidance for Contributors
-------------------------
1. Keep XML documentation up to date when introducing or changing public APIs.
2. Prefer immutable records for DTOs and value types; use factories for aggregate creation.
3. When extending the domain, add identifiers/descriptors alongside aggregates to maintain parity.
4. Avoid referencing platform-specific dependencies—this project should remain portable.

Prerequisites
-------------
- .NET SDK 9.0 or newer.
- The rest of the GainsLab solution when you want to exercise the models in context.
