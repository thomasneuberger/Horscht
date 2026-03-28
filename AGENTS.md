# AGENTS.md

## Purpose
- Help coding agents become productive in Horscht quickly and safely using code-backed patterns.

## Repo Map (strict boundaries)
- `Horscht.Contracts`: interfaces, DTOs, options only; no dependencies on other Horscht projects.
- `Horscht.Logic`: business services implementing contract interfaces (`UploadService`, `ImportService`, `LibraryService`).
- `Horscht.App`: reusable Razor components and shared DI (`AddSharedHorschtServices`).
- `Horscht.Web`: Blazor WebAssembly host with MSAL auth and browser storage access.
- `Horscht.Api` + `Horscht.Importer`: ASP.NET Core services with JWT bearer + Swagger OAuth in Development.
- `Horscht.AppHost` + `Horscht.ServiceDefaults`: local Aspire orchestration, Azurite, telemetry, health endpoints.

## Core Data Flow (upload -> catalog)
1. `Horscht.App/Pages/Upload.razor.cs` calls `IUploadService.UploadFile(...)`.
2. `Horscht.Logic/Services/UploadService.cs` uploads to `upload` blob container and enqueues `ImportMessage` in `import` queue.
3. `Horscht.Importer/HostedServices/FileImport.cs` polls queue and calls `IImportService.ImportFile(...)`.
4. `Horscht.Logic/Services/ImportService.cs` reads metadata via ATL, writes `Song` to `songs` table, copies blob to `songs` container, deletes source blob.
5. `Horscht.App/Pages/Library.razor.cs` uses `ILibraryService.GetAllSongs()` to query table storage.

## Developer Workflows (what actually works here)
- Preferred local run (Web + API + Importer + Azurite + storage init): `dotnet run --project .\Horscht.AppHost`.
- Basic build: `dotnet build .\Horscht.sln`.
- CI parity currently means Docker builds only (no test project in solution):
  - `docker build . --file .\Horscht.Web\Dockerfile --tag horscht-web:local`
  - `docker build . --file .\Horscht.Importer\Dockerfile --tag horscht-importer:local`

## Project-Specific Conventions
- Follow `.editorconfig`: file-scoped namespaces, nullable enabled, `_fieldName` private fields, and `var` where obvious.
- Keep I/O async end-to-end (storage calls in Logic and HostedServices are async).
- Register DI via extension methods returning `IServiceCollection` (`Horscht.Logic/HorschtExtensions.cs`, `Horscht.App/SharedHorschtExtensions.cs`).
- Add reusable cross-project abstractions in `Horscht.Contracts` first.

## Integration Points and Coupling
- Auth split: Web uses `AddMsalAuthentication` with `StorageConstants.Scope`; API/Importer use `AddMicrosoftIdentityWebApi` + fallback auth policy.
- Storage auth split: Web `StorageClientProvider` switches between Azurite `ConnectionString` (dev) and Azure AD token (prod).
- Aspire storage reference names must stay aligned (`blobs`, `queues`, `tables`) across `Horscht.AppHost/AppHost.cs` and service `Program.cs` files.
- OpenAI integration is Importer-only via `AzureOpenAI` config (`Horscht.Importer/AzureOpenAIOptions.cs`, `OPENAI.md`).

## Guardrails for AI Agents
- Prefer new behavior in `Horscht.Logic` and new interfaces in `Horscht.Contracts`; keep host projects thin.
- If adding API endpoints, create `Horscht.Api/Controllers/*` (there are currently no domain controllers there).
- Keep storage names aligned with `AppStorageOptions` keys used by Logic (`UploadContainer`, `SongContainer`, `ImportQueue`, `SongTable`).
- When changing auth, storage, or OpenAI wiring, update code and docs together (`AUTHENTICATION.md`, `ASPIRE.md`, `OPENAI.md`, `deployment/*.bicep`).
