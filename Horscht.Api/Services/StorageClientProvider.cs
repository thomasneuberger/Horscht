using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using Horscht.Contracts.Services;

namespace Horscht.Api.Services;

internal class StorageClientProvider : IStorageClientProvider
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly QueueServiceClient _queueServiceClient;
    private readonly TableServiceClient _tableServiceClient;

    public StorageClientProvider(
        BlobServiceClient blobServiceClient,
        QueueServiceClient queueServiceClient,
        TableServiceClient tableServiceClient)
    {
        _blobServiceClient = blobServiceClient;
        _queueServiceClient = queueServiceClient;
        _tableServiceClient = tableServiceClient;
    }

    public async Task<BlobContainerClient> GetContainerClient(string container)
    {
        var blobContainerClient = _blobServiceClient.GetBlobContainerClient(container);
        return await Task.FromResult(blobContainerClient);
    }

    public async Task<QueueClient> GetQueueClient(string queue)
    {
        var queueClient = _queueServiceClient.GetQueueClient(queue);
        return await Task.FromResult(queueClient);
    }

    public async Task<TableClient> GetTableClient(string table)
    {
        var tableClient = _tableServiceClient.GetTableClient(table);
        return await Task.FromResult(tableClient);
    }
}
