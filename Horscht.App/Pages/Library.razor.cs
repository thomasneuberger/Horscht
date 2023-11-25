using Horscht.Contracts.Entities;
using Horscht.Contracts.Services;
using Microsoft.AspNetCore.Components;

namespace Horscht.App.Pages;

public partial class Library
{
    [Inject]
    public required ILibraryService LibraryService { get; set; }

    private bool _loading;

    private readonly List<Song> _songs = new List<Song>();

    protected override async Task OnInitializedAsync()
    {
        _loading = true;

        var songs = await LibraryService.GetAllSongs();

        _songs.AddRange(songs);

        _loading = false;
    }
}
