using Azure.Storage.Blobs;
using Horscht.Contracts;
using Horscht.Contracts.Services;
using Horscht.Logic.Options;
using Microsoft.Extensions.Options;

namespace Horscht.Logic.Services;

internal class UploadService : IUploadService
{
    private readonly IOptions<StorageOptions> _storageOptions;
    private readonly IAuthenticationService _authenticationService;

    public UploadService(IOptions<StorageOptions> storageOptions, IAuthenticationService authenticationService)
    {
        _storageOptions = storageOptions;
        _authenticationService = authenticationService;
    }

    public async Task UploadFile(Stream fileStream, string filename)
    {
        try
        {
            var containerUri = $"{_storageOptions.Value.BlobUri.TrimEnd('/')}/{_storageOptions.Value.UploadContainer}";

            var token = await _authenticationService.GetAccessTokenAsync(
                    new[]
                    {
                        StorageConstants.Scope
                    },
                    CancellationToken.None)
                .ConfigureAwait(false);
            if (token.HasValue)
            {
                var blobContainerClient = new BlobContainerClient(new Uri(containerUri), new AccessTokenCredential(token.Value));

                await blobContainerClient.CreateIfNotExistsAsync();

                var blobClient = blobContainerClient.GetBlobClient(filename);

                await blobClient.UploadAsync(fileStream);
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
