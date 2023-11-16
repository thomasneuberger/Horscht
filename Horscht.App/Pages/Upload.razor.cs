using Horscht.App.ViewModels;
using Horscht.Contracts.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Horscht.App.Pages;

[Authorize]
public partial class Upload
{
    [Inject]
    public required IUploadService UploadService { get; set; }

    private readonly List<UploadFile> _selectedFiles = new ();

    private void OnFilesSelected(InputFileChangeEventArgs args)
    {
        var files = args.GetMultipleFiles()
            .Select(f => new UploadFile
            {
                File = f
            });

        _selectedFiles.AddRange(files);

        this.StateHasChanged();
    }

    private async Task UploadSingleFile(UploadFile file)
    {
        file.IsUploading = true;

        this.StateHasChanged();

        await UploadService.UploadFile(file.File.OpenReadStream(10_000_000), file.File.Name);

        file.IsUploading = false;
        file.IsUploaded = true;

        this.StateHasChanged();
    }

    private async Task UploadAllFiles()
    {
        try
        {
            foreach (var file in _selectedFiles)
            {
                if (file is { IsUploaded: false, IsUploading: false })
                {
                    await UploadSingleFile(file);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
}
