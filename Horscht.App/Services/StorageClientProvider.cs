using Azure.Core;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using Horscht.App.Authentication;
using Horscht.Contracts;
using Horscht.Contracts.Options;
using Horscht.Contracts.Services;
using Microsoft.Extensions.Options;

namespace Horscht.App.Services;
internal class StorageClientProvider : IStorageClientProvider
{
    private readonly IAuthenticationService _authenticationService;
    private readonly IOptions<AppStorageOptions> _storageOptions;

    private AccessToken? _token;
    private readonly SemaphoreSlim _tokenSemaphore = new(1, 1);

    public StorageClientProvider(IAuthenticationService authenticationService, IOptions<AppStorageOptions> storageOptions)
    {
        _authenticationService = authenticationService;
        _storageOptions = storageOptions;
    }

    private async Task<AccessTokenCredential> GetCredentials()
    {
        await _tokenSemaphore.WaitAsync();
        try
        {
            _token ??= await _authenticationService.GetAccessTokenAsync(
                    new[]
                    {
                        StorageConstants.Scope
                    },
                    CancellationToken.None)
                .ConfigureAwait(false);

            if (_token is null)
            {
                throw new UnauthorizedAccessException();
            }

            return new AccessTokenCredential(_token.Value);
        }
        finally
        {
            _tokenSemaphore.Release();
        }
    }

    public Task<BlobContainerClient> GetContainerClient(string container)
    {
        // Use connection string if available (local development with Azurite)
        if (!string.IsNullOrWhiteSpace(_storageOptions.Value.ConnectionString))
        {
            var blobContainerClient = new BlobContainerClient(_storageOptions.Value.ConnectionString, container);
            return Task.FromResult(blobContainerClient);
        }

        // Use Azure AD authentication (production)
        var containerUri = $"{_storageOptions.Value.BlobUri.TrimEnd('/')}/{container}";
        return GetCredentialsAndCreateBlobClient(containerUri);
    }

    private async Task<BlobContainerClient> GetCredentialsAndCreateBlobClient(string containerUri)
    {
        var credentials = await GetCredentials();
        return new BlobContainerClient(new Uri(containerUri), credentials);
    }

    public Task<QueueClient> GetQueueClient(string queue)
    {
        // Use connection string if available (local development with Azurite)
        if (!string.IsNullOrWhiteSpace(_storageOptions.Value.ConnectionString))
        {
            var queueClient = new QueueClient(_storageOptions.Value.ConnectionString, queue);
            return Task.FromResult(queueClient);
        }

        // Use Azure AD authentication (production)
        var queueUri = $"{_storageOptions.Value.QueueUri.TrimEnd('/')}/{queue}";
        return GetCredentialsAndCreateQueueClient(queueUri);
    }

    private async Task<QueueClient> GetCredentialsAndCreateQueueClient(string queueUri)
    {
        var credentials = await GetCredentials();
        return new QueueClient(new Uri(queueUri), credentials);
    }

    public Task<TableClient> GetTableClient(string table)
    {
        // Use connection string if available (local development with Azurite)
        if (!string.IsNullOrWhiteSpace(_storageOptions.Value.ConnectionString))
        {
            var tableClient = new TableClient(_storageOptions.Value.ConnectionString, table);
            return Task.FromResult(tableClient);
        }

        // Use Azure AD authentication (production)
        return GetCredentialsAndCreateTableClient(table);
    }

    private async Task<TableClient> GetCredentialsAndCreateTableClient(string table)
    {
        var credentials = await GetCredentials();
        return new TableClient(new Uri(_storageOptions.Value.TableUri), table, credentials);
    }
}
