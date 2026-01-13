# GainsLab.WebLayer

Blazor Server (Razor Components) front end for GainsLab’s data management workflows. This app gives editors an authenticated workstation to browse, add, edit, and delete domain entities such as equipment, descriptors, muscles, and movement categories while staying in sync with the backend API.

## Highlights
- Targets .NET **9.0** with Interactive Server rendering (`@rendermode InteractiveServer`).
- Uses the shared infrastructure stack: remote providers (`HttpDataProvider`), sync client, caching registries, and gateways built on GainsLab.Application/Infrastructure.
- Toast + confirmation services (`Model/Notification/`) provide consistent UX feedback for create/update/delete flows.
- Preconfigured HTTP clients for descriptors, equipment, and muscles with automatic base URL resolution via `GAINS_SYNC_BASE_URL`.

## Prerequisites
- .NET SDK **9.0.100** or newer.
- Running instance of the GainsLab API (default `https://localhost:5001/`).
- Access to the same PostgreSQL/SQLite databases the API uses if you intend to see persisted changes reflected elsewhere.

## Configuration
`Program.cs` resolves the sync API base address from an environment variable:

```bash
export GAINS_SYNC_BASE_URL="https://localhost:5001/"
```

If unset, the app defaults to `https://localhost:5001/`. The value must be an absolute HTTP/S URL; the bootstrapper validates it and throws if malformed.

You can override other ASP.NET Core settings via the usual `appsettings.json` or user-secrets APIs (`ConnectionStrings` are not required because persistence lives in the API).

## Run Locally
```bash
dotnet restore GainsLab.sln
dotnet run --project web/GainsLab.WebLayer/GainsLab.WebLayer.csproj
```

Browse to `https://localhost:5002` (or the port printed in the console). Ensure the API is running and reachable at the configured `GAINS_SYNC_BASE_URL`; otherwise the HTTP clients will log failures when trying to load data.

## Project Layout
| Path | Description |
| --- | --- |
| `Components/` | Razor components, layouts, and routes. `Components/Pages/` hosts feature pages (equipment, movement categories, muscles, home, error). `Components/Control/` contains reusable editor widgets. |
| `Model/` | UI-facing models (form state, dropdown configs, entity links) plus notification abstractions (`ToastService`, `ConfirmDialogService`). |
| `Program.cs` | Configures DI: registers `HttpDataProvider`, gateways, query caches, sync client, toast/confirm services, and Razor Components. |
| `wwwroot/` | Static assets (CSS, icons, etc.). |
| `appsettings*.json` | Standard ASP.NET Core configuration files. |

## Key Services & Dependencies
- **HTTP clients** (`DescriptorApi`, `EquipmentApi`, `MuscleApi`) defined in `Infrastructure.Api` for typed REST access. Registered via `IApiClientRegistry`.
- **Providers/gateways** (`IDescriptorProvider`, `IDescriptorGateway`, etc.) expose domain-friendly operations; pages inject these to load and mutate data.
- **Caching** – `EquipmentQueryCache`, `DescriptorQueryCache`, and registries (`EquipmentRegistry`, `DescriptorRegistry`) reduce redundant HTTP calls and keep component state in sync with remote updates.
- **Sync utilities** – `IEntitySyncClient` connects to the API’s `/sync` endpoints for bulk operations when seeding from the web UI is required.
- **Notifications** – `ToastService` and `ConfirmDialogService` supply popups for success/error flows or destructive actions.

## Feature Pages
- `/` – Home dashboard with quick links to entity editors.
- `/equipments` – List view + inline component (`EquipmentDisplayComponent`) with links to view/edit/delete.
- `/equipments/new` / `/equipments/{id}/edit` – Forms for creating or editing equipment entries, driving descriptor updates when needed.
- `/movement-category` and `/muscle` folders are scaffolding for similar CRUD experiences (see `Components/Pages/MovementCategory` and `Components/Pages/Muscle`).
- `/error` – Friendly error page for unhandled exceptions when not in development.

Routes are declared in `Components/Routes.razor`, and `App.razor` wires layouts + notification components globally.

## Extending the WebLayer
1. **Add a new entity editor**
   - Create Razor pages under `Components/Pages/<Entity>/`.
   - Build reusable form components in `Components/Control/`.
   - Implement model classes (view models, dropdown sources) in `Model/<Entity>/`.
   - Register any new gateways/providers/caches in `Program.cs`.
2. **Talk to new API endpoints**
   - Wrap the endpoint in a typed client under `GainsLab.Infrastructure.Api`.
   - Register the client via `AddHttpClient` and expose it through the existing gateways/providers.
3. **Customize UX**
   - Extend `ToastService` or `ConfirmDialogService` to support new message types.
   - Update layouts in `Components/Layout/` for navigation or theming tweaks.

## Development Tips
- Because the app runs as Interactive Server, component state lives on the server; keep payloads small and dispose of scopes appropriately (e.g., caches are singletons, registries are scoped).
- Use the shared `ILogger` (`GainsLabLogger`) via DI for consistent console output. Logging noise can be toggled with `ToggleDecoration`.
- When debugging HTTP calls, monitor the API logs—`HttpDataProvider` logs each request/response and uses `NetworkChecker` to fail fast when offline.
- Wrap destructive actions with `IConfirmDialog` so users receive confirmation prompts before deleting important content.

With the API online and `GAINS_SYNC_BASE_URL` configured, this web layer becomes the easiest way to manage GainsLab data without diving into direct database scripts or the desktop editor.
