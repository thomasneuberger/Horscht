using Horscht.Contracts.Messages;
using Horscht.Contracts.Options;
using Horscht.Contracts.Services;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Horscht.Logic.Services;

internal class UploadService : IUploadService
{
    private readonly IOptions<AppStorageOptions> _storageOptions;
    private readonly IStorageClientProvider _storageClientProvider;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public UploadService(IOptions<AppStorageOptions> storageOptions, JsonSerializerOptions jsonSerializerOptions, IStorageClientProvider storageClientProvider)
    {
        _storageOptions = storageOptions;
        _jsonSerializerOptions = jsonSerializerOptions;
        _storageClientProvider = storageClientProvider;
    }

    public async Task UploadFile(Stream fileStream, string filename)
    {
        try
        {
            var blobContainerClient = await _storageClientProvider.GetContainerClient(_storageOptions.Value.UploadContainer);

            await blobContainerClient.CreateIfNotExistsAsync();

            var blobClient = blobContainerClient.GetBlobClient(filename);

            await blobClient.UploadAsync(fileStream);

            var queueClient = await _storageClientProvider.GetQueueClient(_storageOptions.Value.ImportQueue);

            var message = new ImportMessage
            {
                FileName = filename
            };

            var serializedMessage = JsonSerializer.Serialize(message, _jsonSerializerOptions);

            var receipt = await queueClient.SendMessageAsync(serializedMessage);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }
}
