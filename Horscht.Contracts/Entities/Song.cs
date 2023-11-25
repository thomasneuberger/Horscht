using Azure;
using Azure.Data.Tables;

namespace Horscht.Contracts.Entities;

public class Song : ITableEntity
{
    public string PartitionKey { get; set; } = nameof(Song);
    public required string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public required string Filename { get; set; }

    public required string Uri { get; set; }

    public required string Artist { get; set; }

    public required string Title { get; set; }
}
