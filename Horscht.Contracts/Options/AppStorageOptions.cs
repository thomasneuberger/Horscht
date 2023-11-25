namespace Horscht.Contracts.Options;

public class AppStorageOptions
{
    public required string BlobUri { get; set; }

    public required string UploadContainer { get; set; }

    public required string QueueUri { get; set; }

    public required string ImportQueue { get; set; }
}
