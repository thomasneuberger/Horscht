# Horscht - GitHub Copilot Instructions

This repository contains a music catalog application built with .NET 10 and Blazor WebAssembly.

## Important Documentation

For detailed architecture, coding standards, and development guidelines, please refer to:
- **[ARCHITECTURE.md](../ARCHITECTURE.md)** - Comprehensive technical documentation
- **[README.md](../README.md)** - Project overview and getting started guide

## Key Points for Code Generation

### Technology Stack
- .NET 10 with C# 12
- Blazor WebAssembly for client UI
- Azure services (Blob Storage, Table Storage, Queue Storage)
- Azure AD authentication

### Coding Standards
- Use file-scoped namespaces
- Nullable reference types enabled
- Use `var` when type is obvious from the right side
- All I/O operations must be async
- Private fields with underscore prefix (e.g., `_fieldName`)
- Services registered via extension methods

### Architecture
- **Horscht.Contracts** - Interfaces and DTOs (no dependencies)
- **Horscht.Logic** - Business logic implementations
- **Horscht.App** - Reusable Blazor components
- **Horscht.Web** - Blazor WebAssembly host
- **Horscht.Importer** - Background processing service

### Dependency Injection Pattern
```csharp
public static IServiceCollection AddServiceName(this IServiceCollection services)
{
    services.AddScoped<IServiceInterface, ServiceImplementation>();
    return services;
}
```

### Authentication
- Client: MSAL (Microsoft Authentication Library)
- Server: JWT Bearer with Microsoft.Identity.Web

Please consult [ARCHITECTURE.md](../ARCHITECTURE.md) for complete guidelines including:
- Detailed project structure
- Service patterns
- Configuration management
- Error handling
- Security considerations
- Best practices
