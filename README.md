# Horscht

A music catalog application built with .NET 10 and Blazor WebAssembly that allows users to upload, organize, and browse their music collection with automatic metadata extraction.

## Overview

Horscht is a cloud-native music cataloging system that enables users to:
- **Upload** music files through a modern web interface
- **Automatically extract** metadata (artist, title) from audio files
- **Store and organize** music files in the cloud
- **Browse** their music library with an intuitive interface
- **Secure access** through Azure AD authentication

## Architecture

The application follows a clean layered architecture:

- **Horscht.Web** - Blazor WebAssembly client application
- **Horscht.App** - Reusable Razor components for UI
- **Horscht.Logic** - Business logic and service implementations
- **Horscht.Contracts** - Shared interfaces and DTOs
- **Horscht.Importer** - Background service for processing uploaded files

## Technology Stack

- **.NET 10** - Modern .NET framework
- **Blazor WebAssembly** - Client-side web framework
- **.NET Aspire** - Local development orchestration (see [ASPIRE.md](ASPIRE.md))
- **Azure Storage** - Blob storage for files, Table storage for catalog, Queue storage for messaging
- **Azure AD** - Authentication and authorization
- **Azure Container Apps** - Cloud hosting for background services
- **ATL Library** - Audio metadata extraction

## Getting Started

### Prerequisites

- .NET 10 SDK
- Azure subscription (for cloud deployment)
- Visual Studio 2022 (with .NET Aspire workload) or VS Code
- Docker Desktop (for local development with Aspire)

### Local Development with Aspire

For local development, the application uses .NET Aspire to run all services together with Azurite (Azure Storage emulator):

1. Ensure Docker Desktop is running
2. Set `Horscht.AppHost` as the startup project in Visual Studio
3. Press F5 to run

See [ASPIRE.md](ASPIRE.md) for detailed instructions on using Aspire for local development.

### Configuration

The application requires Azure services to be configured. See the [ARCHITECTURE.md](ARCHITECTURE.md) file for detailed setup instructions and coding guidelines.

## How It Works

1. **Upload**: Users upload music files through the web interface
2. **Queue**: Files are stored in Azure Blob Storage and a message is queued
3. **Process**: Background importer service picks up the message and processes the file
4. **Extract**: Metadata (artist, title) is extracted from the audio file
5. **Catalog**: Song information is stored in Azure Table Storage
6. **Browse**: Users can view their music library through the web interface

## Project Structure

```
Horscht/
├── Horscht.AppHost/        # .NET Aspire orchestration (local dev)
├── Horscht.ServiceDefaults/ # Shared Aspire service configuration
├── Horscht.Web/           # Blazor WebAssembly host
├── Horscht.App/           # Razor components library
├── Horscht.Logic/         # Business logic services
├── Horscht.Contracts/     # Shared interfaces and models
├── Horscht.Importer/      # Background processing service
└── Horscht.Deployment/    # Infrastructure as Code (Bicep)
```

## Documentation

- **[ARCHITECTURE.md](ARCHITECTURE.md)** - Detailed architecture, coding standards, and development guidelines
- **[ASPIRE.md](ASPIRE.md)** - .NET Aspire integration for local development

## License

See [LICENSE.txt](LICENSE.txt) for details.