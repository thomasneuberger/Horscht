namespace Horscht.Contracts.Services;

public interface IImportService
{
    Task ImportFile(string filename, CancellationToken cancellationToken);
}
