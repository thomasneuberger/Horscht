using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;

namespace Horscht.Importer.HostedServices;

/// <summary>
/// Initializes Azure Storage resources (blobs, queues, and tables) on startup.
/// This ensures that required storage structures exist before the application starts processing.
/// Also configures CORS for Azurite to allow browser-based access.
/// </summary>
internal class StorageInitializer : IHostedService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly QueueServiceClient _queueServiceClient;
    private readonly TableServiceClient _tableServiceClient;
    private readonly ILogger<StorageInitializer> _logger;

    public StorageInitializer(
        BlobServiceClient blobServiceClient,
        QueueServiceClient queueServiceClient,
        TableServiceClient tableServiceClient,
        ILogger<StorageInitializer> logger)
    {
        _blobServiceClient = blobServiceClient;
        _queueServiceClient = queueServiceClient;
        _tableServiceClient = tableServiceClient;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Initializing storage resources...");

        try
        {
            // Configure CORS for Azurite (local development)
            await ConfigureCorsAsync(cancellationToken);

            // Create import queue if it doesn't exist
            var importQueue = _queueServiceClient.GetQueueClient("import");
            await importQueue.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
            _logger.LogInformation("Import queue initialized");

            // Create songs table if it doesn't exist
            var songsTable = _tableServiceClient.GetTableClient("songs");
            await songsTable.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
            _logger.LogInformation("Songs table initialized");

            _logger.LogInformation("Storage resources initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize storage resources");
            throw;
        }
    }

    private async Task ConfigureCorsAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Configure CORS for Blob service
            var blobServiceProperties = new BlobServiceProperties
            {
                Cors = new List<BlobCorsRule>
                {
                    new BlobCorsRule
                    {
                        AllowedOrigins = "*",
                        AllowedMethods = "GET,PUT,POST,DELETE,OPTIONS",
                        AllowedHeaders = "*",
                        ExposedHeaders = "*",
                        MaxAgeInSeconds = 3600
                    }
                }
            };
            await _blobServiceClient.SetPropertiesAsync(blobServiceProperties, cancellationToken);
            _logger.LogInformation("CORS configured for Blob service");

            // Configure CORS for Queue service
            var queueServiceProperties = new QueueServiceProperties
            {
                Cors = new List<QueueCorsRule>
                {
                    new QueueCorsRule
                    {
                        AllowedOrigins = "*",
                        AllowedMethods = "GET,PUT,POST,DELETE,OPTIONS",
                        AllowedHeaders = "*",
                        ExposedHeaders = "*",
                        MaxAgeInSeconds = 3600
                    }
                }
            };
            await _queueServiceClient.SetPropertiesAsync(queueServiceProperties, cancellationToken);
            _logger.LogInformation("CORS configured for Queue service");

            // Note: Table service CORS configuration is not supported via SDK in the same way
            // Browser access to Table storage works without explicit CORS in Azurite
            _logger.LogInformation("CORS configuration completed");
        }
        catch (Exception ex)
        {
            // CORS configuration failure shouldn't prevent the app from starting
            _logger.LogWarning(ex, "Failed to configure CORS (this is expected in production)");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
