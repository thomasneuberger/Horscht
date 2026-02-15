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

            // Configure CORS for Table service via REST API
            // The Azure.Data.Tables SDK doesn't support SetPropertiesAsync, so we use the REST API directly
            await ConfigureTableCorsViaRestApiAsync(cancellationToken);
            _logger.LogInformation("CORS configured for Table service");

            _logger.LogInformation("CORS configuration completed");
        }
        catch (Exception ex)
        {
            // CORS configuration failure shouldn't prevent the app from starting
            _logger.LogWarning(ex, "Failed to configure CORS (this is expected in production)");
        }
    }

    private async Task ConfigureTableCorsViaRestApiAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Build the Table service properties XML with CORS rules
            var servicePropertiesXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<StorageServiceProperties>
    <Cors>
        <CorsRule>
            <AllowedOrigins>*</AllowedOrigins>
            <AllowedMethods>GET,PUT,POST,DELETE,OPTIONS</AllowedMethods>
            <AllowedHeaders>*</AllowedHeaders>
            <ExposedHeaders>*</ExposedHeaders>
            <MaxAgeInSeconds>3600</MaxAgeInSeconds>
        </CorsRule>
    </Cors>
</StorageServiceProperties>";

            var tableServiceUri = _tableServiceClient.Uri;
            var requestUri = new Uri($"{tableServiceUri}?restype=service&comp=properties");

            using var httpClient = new HttpClient();
            using var content = new StringContent(servicePropertiesXml, System.Text.Encoding.UTF8, "application/xml");
            
            // Add required headers for Azurite
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/xml");
            
            var request = new HttpRequestMessage(HttpMethod.Put, requestUri)
            {
                Content = content
            };

            // Add authentication header with the Azurite key
            var accountName = "devstoreaccount1";
            var accountKey = "Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==";
            var dateString = DateTime.UtcNow.ToString("R");
            
            request.Headers.Add("x-ms-date", dateString);
            request.Headers.Add("x-ms-version", "2019-02-02");
            
            // For Azurite, we can use a simpler SharedKey authorization
            var stringToSign = $"PUT\n\n\n{servicePropertiesXml.Length}\n\napplication/xml\n\n\n\n\n\nx-ms-date:{dateString}\nx-ms-version:2019-02-02\n/{accountName}/?comp=properties\nrestype:service";
            
            using var hmac = new System.Security.Cryptography.HMACSHA256(Convert.FromBase64String(accountKey));
            var signature = Convert.ToBase64String(hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(stringToSign)));
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("SharedKey", $"{accountName}:{signature}");

            var response = await httpClient.SendAsync(request, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Failed to set Table CORS via REST API. Status: {StatusCode}, Content: {Content}", 
                    response.StatusCode, errorContent);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to configure Table CORS via REST API");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
