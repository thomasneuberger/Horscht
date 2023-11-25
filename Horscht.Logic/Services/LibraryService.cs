using Horscht.Contracts.Entities;
using Horscht.Contracts.Options;
using Horscht.Contracts.Services;
using Microsoft.Extensions.Options;

namespace Horscht.Logic.Services;

internal class LibraryService : ILibraryService
{
    private readonly IOptions<AppStorageOptions> _storageOptions;
    private readonly IStorageClientProvider _storageClientProvider;

    public LibraryService(IStorageClientProvider storageClientProvider, IOptions<AppStorageOptions> storageOptions)
    {
        _storageClientProvider = storageClientProvider;
        _storageOptions = storageOptions;
    }

    public async Task<IReadOnlyList<Song>> GetAllSongs()
    {
        var tableClient = await _storageClientProvider.GetTableClient(_storageOptions.Value.SongTable);

        var pages = tableClient.QueryAsync<Song>()
            .AsPages();

        var songs = new List<Song>();

        await foreach (var page in pages)
        {
            songs.AddRange(page.Values);
        }

        return songs;
    }
}
