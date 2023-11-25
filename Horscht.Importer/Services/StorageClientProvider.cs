using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using Horscht.Contracts.Services;
using Microsoft.Extensions.Options;

namespace Horscht.Importer.Services;

internal class StorageClientProvider : IStorageClientProvider
{
    private readonly IOptions<ImporterStorageOptions> _storageOptions;

    public StorageClientProvider(IOptions<ImporterStorageOptions> storageOptions)
    {
        _storageOptions = storageOptions;
    }

    public async Task<BlobContainerClient> GetContainerClient(string container)
    {
        var blobContainerClient = new BlobContainerClient(_storageOptions.Value.ConnectionString, container);

        return await Task.FromResult(blobContainerClient);
    }

    public async Task<QueueClient> GetQueueClient(string queue)
    {
        var queueClient = new QueueClient(_storageOptions.Value.ConnectionString, queue);

        return await Task.FromResult(queueClient);
    }

    public async Task<TableClient> GetTableClient(string table)
    {
        var tableClient = new TableClient(_storageOptions.Value.ConnectionString, table);

        return await Task.FromResult(tableClient);
    }
}
