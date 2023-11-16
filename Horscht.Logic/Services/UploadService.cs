using System.Text.Json;
using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using Horscht.Contracts;
using Horscht.Contracts.Messages;
using Horscht.Contracts.Services;
using Horscht.Logic.Options;
using Microsoft.Extensions.Options;

namespace Horscht.Logic.Services;

internal class UploadService : IUploadService
{
    private readonly IOptions<StorageOptions> _storageOptions;
    private readonly IAuthenticationService _authenticationService;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public UploadService(IOptions<StorageOptions> storageOptions, IAuthenticationService authenticationService, JsonSerializerOptions jsonSerializerOptions)
    {
        _storageOptions = storageOptions;
        _authenticationService = authenticationService;
        _jsonSerializerOptions = jsonSerializerOptions;
    }

    public async Task UploadFile(Stream fileStream, string filename)
    {
        try
        {
            var containerUri = $"{_storageOptions.Value.BlobUri.TrimEnd('/')}/{_storageOptions.Value.UploadContainer}";
            var queueUri = $"{_storageOptions.Value.QueueUri.TrimEnd()}/{_storageOptions.Value.ImportQueue}";

            var token = await _authenticationService.GetAccessTokenAsync(
                    new[]
                    {
                        StorageConstants.Scope
                    },
                    CancellationToken.None)
                .ConfigureAwait(false);
            if (token.HasValue)
            {
                var credentials = new AccessTokenCredential(token.Value);
                var blobContainerClient = new BlobContainerClient(new Uri(containerUri), credentials);

                await blobContainerClient.CreateIfNotExistsAsync();

                var blobClient = blobContainerClient.GetBlobClient(filename);

                await blobClient.UploadAsync(fileStream);

                var queueClient = new QueueClient(new Uri(queueUri), credentials);

                var message = new ImportMessage
                {
                    FileUri = blobClient.Uri.AbsoluteUri
                };

                var serializedMessage = JsonSerializer.Serialize(message, _jsonSerializerOptions);

                var receipt = await queueClient.SendMessageAsync(serializedMessage);
            }
            else
            {
                throw new UnauthorizedAccessException();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }
}
