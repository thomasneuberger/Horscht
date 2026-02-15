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
    // Azurite development credentials (publicly documented, safe for local dev only)
    private const string AzuriteAccountName = "devstoreaccount1";
    private const string AzuriteAccountKey = "Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==";
    private const string TableApiVersion = "2019-02-02";

    private readonly BlobServiceClient _blobServiceClient;
    private readonly QueueServiceClient _queueServiceClient;
    private readonly TableServiceClient _tableServiceClient;
    private readonly ILogger<StorageInitializer> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public StorageInitializer(
        BlobServiceClient blobServiceClient,
        QueueServiceClient queueServiceClient,
        TableServiceClient tableServiceClient,
        IHttpClientFactory httpClientFactory,
        ILogger<StorageInitializer> logger)
    {
        _blobServiceClient = blobServiceClient;
        _queueServiceClient = queueServiceClient;
        _tableServiceClient = tableServiceClient;
        _httpClientFactory = httpClientFactory;
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

            using var httpClient = _httpClientFactory.CreateClient();
            using var content = new StringContent(servicePropertiesXml, System.Text.Encoding.UTF8, "application/xml");
            
            // Add required headers for Azurite
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/xml");
            
            var request = new HttpRequestMessage(HttpMethod.Put, requestUri)
            {
                Content = content
            };

            var dateString = DateTime.UtcNow.ToString("R");
            request.Headers.Add("x-ms-date", dateString);
            request.Headers.Add("x-ms-version", TableApiVersion);
            
            // Build canonical string for SharedKey authentication
            var stringToSign = string.Join("\n", new[]
            {
                "PUT",
                "",  // Content-Encoding
                "",  // Content-Language
                servicePropertiesXml.Length.ToString(),  // Content-Length
                "",  // Content-MD5
                "application/xml",  // Content-Type
                "",  // Date
                "",  // If-Modified-Since
                "",  // If-Match
                "",  // If-None-Match
                "",  // If-Unmodified-Since
                "",  // Range
                $"x-ms-date:{dateString}",
                $"x-ms-version:{TableApiVersion}",
                $"/{AzuriteAccountName}/?comp=properties\nrestype:service"
            });
            
            using var hmac = new System.Security.Cryptography.HMACSHA256(Convert.FromBase64String(AzuriteAccountKey));
            var signature = Convert.ToBase64String(hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(stringToSign)));
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("SharedKey", $"{AzuriteAccountName}:{signature}");

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
