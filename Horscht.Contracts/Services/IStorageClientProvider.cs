﻿using Azure.Storage.Blobs;
using Azure.Storage.Queues;

namespace Horscht.Contracts.Services;

public interface IStorageClientProvider
{
    Task<BlobContainerClient> GetContainerClient(string container);
    Task<QueueClient> GetQueueClient(string queue);
}
