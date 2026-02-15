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

    public async Task<BlobContainerClient> GetContainerClient(string container)
    {
        // Use connection string if available (local development with Azurite)
        if (!string.IsNullOrWhiteSpace(_storageOptions.Value.ConnectionString))
        {
            var blobContainerClient = new BlobContainerClient(_storageOptions.Value.ConnectionString, container);
            return await Task.FromResult(blobContainerClient);
        }

        // Use Azure AD authentication (production)
        var containerUri = $"{_storageOptions.Value.BlobUri.TrimEnd('/')}/{container}";
        var credentials = await GetCredentials();
        var authenticatedClient = new BlobContainerClient(new Uri(containerUri), credentials);

        return authenticatedClient;
    }

    public async Task<QueueClient> GetQueueClient(string queue)
    {
        // Use connection string if available (local development with Azurite)
        if (!string.IsNullOrWhiteSpace(_storageOptions.Value.ConnectionString))
        {
            var queueClient = new QueueClient(_storageOptions.Value.ConnectionString, queue);
            return await Task.FromResult(queueClient);
        }

        // Use Azure AD authentication (production)
        var queueUri = $"{_storageOptions.Value.QueueUri.TrimEnd('/')}/{_storageOptions.Value.ImportQueue}";
        var credentials = await GetCredentials();
        var authenticatedClient = new QueueClient(new Uri(queueUri), credentials);

        return authenticatedClient;
    }

    public async Task<TableClient> GetTableClient(string table)
    {
        // Use connection string if available (local development with Azurite)
        if (!string.IsNullOrWhiteSpace(_storageOptions.Value.ConnectionString))
        {
            var tableClient = new TableClient(_storageOptions.Value.ConnectionString, table);
            return await Task.FromResult(tableClient);
        }

        // Use Azure AD authentication (production)
        var credentials = await GetCredentials();
        var authenticatedClient = new TableClient(new Uri(_storageOptions.Value.TableUri), table, credentials);

        return authenticatedClient;
    }
}
