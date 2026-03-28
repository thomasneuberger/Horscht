namespace Horscht.Contracts.Services;

public interface ISongMetadataExtractionService
{
    Task<SongMetadataExtractionResult?> TryExtractFromFilename(string filename, CancellationToken cancellationToken);
}

public sealed record SongMetadataExtractionResult(string? Artist, string? Title);
