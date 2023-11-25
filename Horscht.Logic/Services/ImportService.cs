using ATL;
using Azure.Storage.Blobs.Models;
using Horscht.Contracts.Options;
using Horscht.Contracts.Services;
using Microsoft.Extensions.Options;

namespace Horscht.Logic.Services;

internal class ImportService : IImportService
{
    private readonly IOptions<AppStorageOptions> _storageOptions;
    private readonly IStorageClientProvider _storageClientProvider;

    public ImportService(IOptions<AppStorageOptions> storageOptions, IStorageClientProvider storageClientProvider)
    {
        _storageOptions = storageOptions;
        _storageClientProvider = storageClientProvider;
    }

    public async Task ImportFile(string filename, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Import file {filename}...");
        try
        {
            var blobContainerClient = await _storageClientProvider.GetContainerClient(_storageOptions.Value.UploadContainer);

            var blobClient = blobContainerClient.GetBlobClient(filename);

            if (!await blobClient.ExistsAsync(cancellationToken))
            {
                Console.WriteLine($"File {filename} does not exist.");
                return;
            }

            await using (var fileStream = await blobClient.OpenReadAsync(new BlobOpenReadOptions(false), cancellationToken))
            {
                var track = new Track(fileStream);

                Console.WriteLine(track.Artist);
            }

            Console.WriteLine($"Delete file {filename}");
            // ReSharper disable once MethodSupportsCancellation
#pragma warning disable CA2016 // Forward the 'CancellationToken' parameter to methods
            await blobClient.DeleteAsync();
#pragma warning restore CA2016 // Forward the 'CancellationToken' parameter to methods

            Console.WriteLine($"File {filename} deleted.");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }
}
