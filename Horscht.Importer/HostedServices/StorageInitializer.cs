using Azure.Data.Tables;
using Azure.Storage.Queues;

namespace Horscht.Importer.HostedServices;

/// <summary>
/// Initializes Azure Storage resources (queues and tables) on startup.
/// This ensures that required storage structures exist before the application starts processing.
/// </summary>
internal class StorageInitializer : IHostedService
{
    private readonly QueueServiceClient _queueServiceClient;
    private readonly TableServiceClient _tableServiceClient;
    private readonly ILogger<StorageInitializer> _logger;

    public StorageInitializer(
        QueueServiceClient queueServiceClient,
        TableServiceClient tableServiceClient,
        ILogger<StorageInitializer> logger)
    {
        _queueServiceClient = queueServiceClient;
        _tableServiceClient = tableServiceClient;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Initializing storage resources...");

        try
        {
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

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
