namespace Horscht.Importer;

public class ImporterStorageOptions
{
    public required string ConnectionString { get; set; }

    public required string ImportQueue { get; set; }
}
