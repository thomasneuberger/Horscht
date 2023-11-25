namespace Horscht.Contracts.Options;

public class AppStorageOptions
{
    public required string BlobUri { get; set; }

    public required string UploadContainer { get; set; }

    public required string SongContainer { get; set; }

    public required string QueueUri { get; set; }

    public required string ImportQueue { get; set; }

    public required string TableUri { get; set; }

    public required string SongTable { get; set; }
}
