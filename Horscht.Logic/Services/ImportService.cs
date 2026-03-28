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
    private readonly ISongMetadataExtractionService _songMetadataExtractionService;

    public ImportService(
        IOptions<AppStorageOptions> storageOptions,
        IStorageClientProvider storageClientProvider,
        ISongMetadataExtractionService songMetadataExtractionService)
    {
        _storageOptions = storageOptions;
        _storageClientProvider = storageClientProvider;
        _songMetadataExtractionService = songMetadataExtractionService;
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

                var (artist, title) = await ResolveSongMetadata(track, filename, cancellationToken);

                var song = new Song
                {
                    RowKey = Guid.NewGuid().ToString(),
                    Artist = artist,
                    Title = title,
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

    private async Task<(string Artist, string Title)> ResolveSongMetadata(Track track, string filename, CancellationToken cancellationToken)
    {
        var artist = NormalizeMetadataValue(track.Artist);
        var title = NormalizeMetadataValue(track.Title);

        if (string.IsNullOrWhiteSpace(artist) || string.IsNullOrWhiteSpace(title))
        {
            var extractedMetadata = await _songMetadataExtractionService.TryExtractFromFilename(filename, cancellationToken);
            artist = string.IsNullOrWhiteSpace(artist) ? NormalizeMetadataValue(extractedMetadata?.Artist) : artist;
            title = string.IsNullOrWhiteSpace(title) ? NormalizeMetadataValue(extractedMetadata?.Title) : title;
        }

        artist ??= "Unknown Artist";

        title ??= NormalizeMetadataValue(Path.GetFileNameWithoutExtension(filename));
        title ??= "Unknown Title";

        return (artist, title);
    }

    private static string? NormalizeMetadataValue(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
