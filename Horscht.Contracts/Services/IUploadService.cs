namespace Horscht.Contracts.Services;
public interface IUploadService
{
    Task UploadFile(Stream fileStream, string filename);
}
