# Horscht - Architecture and Coding Style Guide

## Overview

Horscht is a music catalog application built with .NET 8 and Blazor WebAssembly. The application allows users to upload, import, store, and browse music files with metadata extraction. The system uses Azure cloud services for storage, queuing, and data management.

## Purpose

The application provides:
- Music file upload functionality
- Automatic metadata extraction from audio files (artist, title)
- Cataloging and storage of music files
- Library browsing interface
- User authentication via Azure AD/Microsoft Identity

## Architecture

### Layered Architecture

The solution follows a clean layered architecture with clear separation of concerns:

```
┌─────────────────────────────────────────────────────────┐
│                   Presentation Layer                     │
│  Horscht.Web (Blazor WebAssembly)                       │
│  Horscht.App (Razor Components)                         │
└─────────────────────────────────────────────────────────┘
                          │
                          ↓
┌─────────────────────────────────────────────────────────┐
│                    Service Layer                         │
│  Horscht.Logic (Business Logic & Services)              │
└─────────────────────────────────────────────────────────┘
                          │
                          ↓
┌─────────────────────────────────────────────────────────┐
│                   Contracts Layer                        │
│  Horscht.Contracts (Interfaces, DTOs, Options)          │
└─────────────────────────────────────────────────────────┘
                          │
                          ↓
┌─────────────────────────────────────────────────────────┐
│                 Infrastructure Layer                     │
│  Azure Storage (Blobs, Tables, Queues)                  │
│  Azure AD (Authentication)                               │
└─────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────┐
│                  Background Services                     │
│  Horscht.Importer (ASP.NET Core Web API + Hosted Service)│
└─────────────────────────────────────────────────────────┘
```

### Project Dependencies

```
Horscht.Web
  ├─> Horscht.App
  │     ├─> Horscht.Logic
  │     └─> Horscht.Contracts
  └─> Horscht.Logic
        └─> Horscht.Contracts

Horscht.Importer
  ├─> Horscht.Logic
  └─> Horscht.Contracts

Horscht.Logic
  └─> Horscht.Contracts

Horscht.Contracts (no dependencies on other projects)

Horscht.Deployment (infrastructure as code)
```

### Projects Description

#### Horscht.Contracts
**Purpose**: Shared contracts and interfaces
- Defines service interfaces (ILibraryService, IUploadService, IImportService, etc.)
- Domain entities (Song)
- Data transfer objects (ImportMessage)
- Configuration options (AppStorageOptions)
- Constants (StorageConstants)
- **No dependencies on other projects** - pure contract definitions

#### Horscht.Logic
**Purpose**: Core business logic and service implementations
- Implements service interfaces from Horscht.Contracts
- LibraryService: Retrieves songs from Azure Table Storage
- UploadService: Uploads files to blob storage and queues import messages
- ImportService: Processes uploaded files, extracts metadata, and catalogs songs
- Extension methods for dependency injection registration
- Uses ATL library for audio metadata extraction

#### Horscht.App
**Purpose**: Reusable Blazor Razor components
- Razor pages (Library, Upload, UserInfo, Index)
- Shared components (NavMenu, MainLayout, LoginDisplay)
- View models (UploadFile)
- Client-side storage provider implementation with token-based authentication
- Can be consumed by different Blazor hosts

#### Horscht.Web
**Purpose**: Blazor WebAssembly host application
- Main entry point for the web application
- Configures MSAL authentication
- Hosts Horscht.App components
- Progressive Web App (PWA) with service worker support

#### Horscht.Importer
**Purpose**: Background service for processing uploaded files
- ASP.NET Core Web API with Swagger documentation
- Hosted service (FileImport) that monitors Azure Queue for import requests
- Processes files asynchronously
- Extracts audio metadata using ATL library
- Moves files from upload container to song container
- Updates catalog in Azure Table Storage
- Protected with Azure AD JWT authentication

#### Horscht.Deployment
**Purpose**: Infrastructure as Code
- Bicep templates for Azure deployment
- Defines storage accounts, container apps, certificates
- Environment configuration

## Technology Stack

### Core Technologies
- **.NET 8**: Target framework
- **C# 12**: Programming language with latest features
- **Blazor WebAssembly**: Client-side web framework
- **ASP.NET Core**: Backend services

### Azure Services
- **Azure Blob Storage**: File storage for music files
- **Azure Table Storage**: NoSQL database for song catalog
- **Azure Queue Storage**: Message queue for asynchronous processing
- **Azure AD/Microsoft Identity**: Authentication and authorization
- **Azure Container Apps**: Hosting for the importer service

### Key NuGet Packages
- **Microsoft.Identity.Web**: Azure AD integration
- **Microsoft.Authentication.WebAssembly.Msal**: Client-side authentication
- **Azure.Storage.Blobs**: Blob storage client
- **Azure.Storage.Queues**: Queue storage client
- **Azure.Data.Tables**: Table storage client
- **Azure.Identity**: Azure authentication
- **z440.atl.core**: Audio metadata extraction library
- **Swashbuckle.AspNetCore**: API documentation

### Build and Deployment
- **Central Package Management**: Directory.Packages.props
- **Bicep**: Infrastructure as Code
- **Docker**: Containerization support

## Project Structure

### Standard Directory Layout

Each project follows a consistent structure:

```
ProjectName/
├── Services/           # Service implementations
├── Pages/             # Razor pages (UI projects)
├── Shared/            # Shared components (UI projects)
├── Authentication/    # Authentication-related code
├── Controllers/       # API controllers (Web API projects)
├── HostedServices/    # Background services
├── Entities/          # Domain entities (Contracts)
├── Messages/          # Message DTOs (Contracts)
├── Options/           # Configuration options
├── ViewModels/        # View models (UI projects)
├── Properties/        # Project properties and settings
├── wwwroot/           # Static web assets
├── _Imports.razor     # Global using directives for Razor
├── Usings.cs          # Global using directives
└── Program.cs         # Application entry point
```

## Coding Standards and Conventions

### EditorConfig Rules

The project uses a comprehensive `.editorconfig` file that enforces:

#### Formatting
- **Indentation**: 4 spaces (not tabs)
- **Line endings**: CRLF (Windows-style)
- **Charset**: UTF-8
- **Final newline**: Not required

#### Code Style
- **var usage**: Explicit types required in this project (`string name` not `var name`). This is a project-specific convention enforced through .editorconfig that differs from general .NET guidelines which recommend `var` when type is apparent. This convention prioritizes explicit type visibility.
- **Braces**: Always required for control structures
- **Expression-bodied members**: 
  - Properties: Preferred (`public int Age => _age;`)
  - Methods: Full body preferred
  - Accessors: Expression-bodied preferred
- **Pattern matching**: Strongly encouraged
- **Null-checking**: Use null-coalescing and null-propagation operators
- **File-scoped namespaces**: Required (`namespace MyApp;` not `namespace MyApp { }`)
- **Primary constructors**: Preferred where applicable
- **Top-level statements**: Preferred for Program.cs

#### Naming Conventions
- **Interfaces**: PascalCase with 'I' prefix (e.g., `ILibraryService`)
- **Classes**: PascalCase (e.g., `LibraryService`)
- **Methods**: PascalCase (e.g., `GetAllSongs`)
- **Properties**: PascalCase (e.g., `FileName`)
- **Private fields**: Camel case with underscore prefix (e.g., `_storageOptions`)
- **Parameters**: Camel case (e.g., `fileName`)
- **Local variables**: Camel case (e.g., `songList`)

### C# Features and Patterns

#### Nullable Reference Types
- **Enabled**: `<Nullable>enable</Nullable>` in all projects
- Use `required` keyword for mandatory properties
- Use `?` for nullable reference types
- Initialize non-nullable properties appropriately

#### Implicit Usings
- **Enabled**: `<ImplicitUsings>enable</ImplicitUsings>`
- Global usings defined in `Usings.cs` files
- Example: `global using Microsoft.Extensions.DependencyInjection;`

#### Modern C# Patterns
```csharp
// File-scoped namespaces
namespace Horscht.Logic.Services;

// Required properties
public class Song : ITableEntity
{
    public required string RowKey { get; set; }
    public required string Filename { get; set; }
}

// Pattern matching
if (importMessage is not null)
{
    await _importService.ImportFile(importMessage.FileName, cancellationToken);
}

// Expression-bodied properties
public string Name => _name;

// Null-coalescing
_token ??= await _authenticationService.GetAccessTokenAsync(...);

// String interpolation
string containerUri = $"{_storageOptions.Value.BlobUri.TrimEnd('/')}/{container}";
```

### Dependency Injection

#### Service Registration Patterns

Services are registered using extension methods for better organization:

```csharp
// In Horscht.Logic/HorschtExtensions.cs
public static IServiceCollection AddUpload(this IServiceCollection services)
{
    services.AddScoped<IUploadService, UploadService>();
    return services;
}

public static IServiceCollection AddLibrary(this IServiceCollection services)
{
    services.AddScoped<ILibraryService, LibraryService>();
    return services;
}
```

#### Service Lifetimes
- **Scoped**: UI services that require per-request state (UploadService, LibraryService)
- **Singleton**: Background services and stateless services (ImportService, hosted services)
- **Transient**: Generally avoided; use scoped or singleton instead

#### Constructor Injection
```csharp
internal class LibraryService : ILibraryService
{
    private readonly IOptions<AppStorageOptions> _storageOptions;
    private readonly IStorageClientProvider _storageClientProvider;

    public LibraryService(IStorageClientProvider storageClientProvider, 
                          IOptions<AppStorageOptions> storageOptions)
    {
        _storageClientProvider = storageClientProvider;
        _storageOptions = storageOptions;
    }
}
```

#### Property Injection (Blazor Components)
```csharp
public partial class Library
{
    [Inject]
    public required ILibraryService LibraryService { get; set; }
}
```

### Configuration Management

#### Options Pattern
All configuration uses the strongly-typed Options pattern:

```csharp
// Define options class
public class AppStorageOptions
{
    public required string BlobUri { get; set; }
    public required string UploadContainer { get; set; }
}

// Register in Program.cs
builder.Services.AddOptions<AppStorageOptions>()
    .Bind(builder.Configuration.GetSection("Storage"))
    .ValidateDataAnnotations();

// Inject using IOptions<T>
public LibraryService(IOptions<AppStorageOptions> storageOptions)
{
    _storageOptions = storageOptions;
}
```

#### Configuration Sources
- `appsettings.json`: Base configuration
- `appsettings.Development.json`: Development overrides
- User Secrets: Local development secrets
- Environment Variables: Production deployment

### Authentication and Authorization

#### Blazor WebAssembly (Client-side)
- Uses **MSAL (Microsoft Authentication Library)**
- Token-based authentication with Azure AD
- Custom `IAuthenticationService` for token acquisition
- `AccessTokenCredential` wrapper for Azure SDK clients

```csharp
// Authentication setup
builder.Services.AddMsalAuthentication(options =>
{
    builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
    options.ProviderOptions.DefaultAccessTokenScopes.Add(StorageConstants.Scope);
});

// Authorize attribute on components
[Authorize]
public partial class Upload { }
```

#### ASP.NET Core (Server-side)
- Uses **Microsoft.Identity.Web**
- JWT Bearer token authentication
- All endpoints require authentication by default

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});
```

### Storage and Data Access Patterns

#### Storage Client Provider Pattern
Abstraction layer for Azure Storage clients:

```csharp
public interface IStorageClientProvider
{
    Task<BlobContainerClient> GetContainerClient(string container);
    Task<QueueClient> GetQueueClient(string queue);
    Task<TableClient> GetTableClient(string table);
}
```

**Client implementation**: Token-based authentication with caching
**Server implementation**: Connection string-based authentication

#### Repository Pattern
Services act as repositories for domain entities:

```csharp
public interface ILibraryService
{
    Task<IReadOnlyList<Song>> GetAllSongs();
}
```

#### Asynchronous Operations
- **All I/O operations are async**: Use `async`/`await` consistently
- Return `Task` or `Task<T>` from async methods
- Pass `CancellationToken` for long-running operations

### Error Handling

#### Logging
```csharp
// Console logging for diagnostic information
Console.WriteLine($"Import file {filename}...");
Console.WriteLine($"File {filename} does not exist.");

// Exception logging
catch (Exception ex)
{
    Console.WriteLine(ex);
    throw;
}
```

#### Exception Handling
- Let exceptions bubble up unless you can handle them meaningfully
- Use try-catch at service boundaries
- Always rethrow after logging unless you're handling the exception

### UI Patterns (Blazor)

#### Component Structure
- Separate code-behind files (`.razor.cs`) from markup (`.razor`)
- Use partial classes
- Lifecycle methods: `OnInitializedAsync` for data loading

```csharp
// Library.razor.cs
public partial class Library
{
    [Inject]
    public required ILibraryService LibraryService { get; set; }

    private bool _loading;
    private readonly List<Song> _songs = new List<Song>();

    protected override async Task OnInitializedAsync()
    {
        _loading = true;
        IReadOnlyList<Song> songs = await LibraryService.GetAllSongs();
        _songs.AddRange(songs);
        _loading = false;
    }
}
```

**Note**: While this pattern works, consider using immutable collections or reassigning the entire list for better change detection in complex scenarios.

#### State Management
- Use `StateHasChanged()` to trigger UI updates after async operations
- Track loading states with boolean flags
- Use view models for complex UI state (e.g., `UploadFile`)

### Background Processing

#### Queue-Based Architecture
1. Upload service puts files in blob storage and sends message to queue
2. Background service (FileImport) polls the queue
3. When message received, import service processes the file
4. File metadata is extracted and stored in Table Storage
5. File is moved from upload to song container
6. Original upload is deleted

#### Hosted Service Pattern
```csharp
internal class FileImport : IObservableHostedService, IDisposable
{
    // StartAsync initiates background processing and returns immediately
    public Task StartAsync(CancellationToken cancellationToken)
    {
        ListenToQueueMessagesAsync(); // Fire-and-forget pattern for background work
        _state = State.Started;
        return Task.CompletedTask;
    }

    // Async void is acceptable here for fire-and-forget background processing
    // IMPORTANT: Must include try-catch to handle exceptions (no caller to propagate to)
    private async void ListenToQueueMessagesAsync()
    {
        try
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                QueueMessage response = await _queueClient.ReceiveMessageAsync(...);
                // Process message
                await Task.Delay(5000); // Polling interval
            }
        }
        catch (TaskCanceledException)
        {
            // Expected when cancellation is requested
        }
        // Add additional catch blocks for other expected exceptions
    }
}
```

## Deployment

### Infrastructure as Code
- **Bicep templates** define all Azure resources
- Resources organized by concern (storage, importer, certificates)
- Parameterized for multiple environments

### Container Deployment
- Importer service runs in Azure Container Apps
- Docker support included
- Managed identity for authentication (in production)

### Environment Configuration
- Development: Uses user secrets and connection strings
- Production: Uses managed identity and environment variables
- Invariant globalization enabled for reduced container size

## Development Guidelines

### Adding a New Feature

1. **Define contracts**: Add interfaces to `Horscht.Contracts`
2. **Implement logic**: Create service in `Horscht.Logic`
3. **Register service**: Add extension method in `HorschtExtensions.cs`
4. **Create UI**: Add Razor component in `Horscht.App`
5. **Wire up**: Inject and use service in component

### Code Review Checklist

- [ ] Follows naming conventions (PascalCase, underscore prefix for fields)
- [ ] Uses file-scoped namespaces
- [ ] All I/O operations are async
- [ ] Proper use of nullable reference types
- [ ] Services registered with appropriate lifetime
- [ ] Configuration uses Options pattern
- [ ] Error handling includes logging
- [ ] No hardcoded values; use configuration
- [ ] Consistent with existing patterns

### Testing Considerations

While the repository doesn't currently include automated tests, consider:
- Unit tests for business logic in `Horscht.Logic`
- Integration tests for Azure Storage operations
- UI component tests for Blazor components

## Security Considerations

### Authentication
- All sensitive operations require authentication
- Azure AD provides identity verification
- Token-based access to Azure Storage

### Authorization
- User-based access control via Azure AD
- Role assignments in Bicep templates
- Scoped access tokens for storage

### Data Protection
- HTTPS enforced
- Secrets managed via Azure Key Vault or User Secrets
- Connection strings never in source code
- Sensitive configuration in environment variables

## Best Practices

### DO
✅ Use async/await for all I/O operations
✅ Inject dependencies through constructors
✅ Use file-scoped namespaces
✅ Apply required keyword for non-nullable properties
✅ Use pattern matching where appropriate
✅ Log exceptions before rethrowing
✅ Use Options pattern for configuration
✅ Follow single responsibility principle
✅ Keep services focused and cohesive
✅ Use extension methods for service registration

### DON'T
❌ Use var for type declarations (project-specific convention that differs from general .NET guidelines)
❌ Block on async code (.Result, .Wait())
❌ Catch exceptions without logging
❌ Hardcode configuration values
❌ Mix authentication approaches
❌ Create circular dependencies between projects
❌ Put business logic in Blazor components
❌ Ignore cancellation tokens in long-running operations
❌ Use public fields; use properties instead
❌ Omit braces in control structures

## Maintenance and Updates

### Package Management
- Central package management via `Directory.Packages.props`
- Dependabot configured for automatic updates
- Regular security updates applied

### Version Targeting
- Target .NET 8 LTS
- Update to newer .NET versions as they become LTS

## Summary

Horscht follows modern .NET best practices with:
- **Clean architecture**: Clear separation of concerns
- **Cloud-native**: Built for Azure from the ground up
- **Async-first**: Non-blocking I/O throughout
- **Type-safe**: Nullable reference types and strong typing
- **Maintainable**: Consistent patterns and conventions
- **Secure**: Azure AD integration and proper secret management

The codebase emphasizes simplicity, consistency, and adherence to established .NET conventions while leveraging modern C# features effectively.
