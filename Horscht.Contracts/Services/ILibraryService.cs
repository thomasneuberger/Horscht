using Horscht.Contracts.Entities;

namespace Horscht.Contracts.Services;

public interface ILibraryService
{
    Task<IReadOnlyList<Song>> GetAllSongs();
}
