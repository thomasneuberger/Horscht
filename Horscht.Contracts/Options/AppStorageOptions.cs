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

    /// <summary>
    /// Optional connection string for local development with Azurite.
    /// When set, storage clients will use connection string authentication instead of Azure AD.
    /// </summary>
    public string? ConnectionString { get; set; }
}
