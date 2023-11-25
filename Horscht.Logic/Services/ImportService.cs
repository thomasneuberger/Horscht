using ATL;
using Azure.Storage.Blobs.Models;
using Horscht.Contracts.Entities;
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
            var uploadContainerClient = await _storageClientProvider.GetContainerClient(_storageOptions.Value.UploadContainer);

            var uploadedFileClient = uploadContainerClient.GetBlobClient(filename);

            if (!await uploadedFileClient.ExistsAsync(cancellationToken))
            {
                Console.WriteLine($"File {filename} does not exist.");
                return;
            }

            var songContainerClient = await _storageClientProvider.GetContainerClient(_storageOptions.Value.SongContainer);

            await songContainerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

            var id = Guid.NewGuid().ToString();
            var songFileClient = songContainerClient.GetBlobClient(Path.ChangeExtension(filename, $"_{id}{Path.GetExtension(filename)}"));

            await using (var fileStream = await uploadedFileClient.OpenReadAsync(new BlobOpenReadOptions(false), cancellationToken))
            {
                var track = new Track(fileStream);

                Console.WriteLine(track.Artist);

                var song = new Song
                {
                    RowKey = Guid.NewGuid().ToString(),
                    Artist = track.Artist,
                    Title = track.Title,
                    Filename = songFileClient.Name,
                    Uri = songFileClient.Uri.AbsoluteUri
                };

                var tableClient = await _storageClientProvider.GetTableClient(_storageOptions.Value.SongTable);

                var response = await tableClient.AddEntityAsync(song, cancellationToken);
            }

            Console.WriteLine($"Copy file {filename}...");
            await using (var fileStream = await uploadedFileClient.OpenReadAsync(new BlobOpenReadOptions(false), cancellationToken))
            {
                await using var songStream = await songFileClient.OpenWriteAsync(true, cancellationToken: cancellationToken);

                await fileStream.CopyToAsync(songStream, cancellationToken);
            }

            Console.WriteLine($"Delete file {filename}");
            // ReSharper disable once MethodSupportsCancellation
#pragma warning disable CA2016 // Forward the 'CancellationToken' parameter to methods
            await uploadedFileClient.DeleteAsync();
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
