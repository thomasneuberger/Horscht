namespace Horscht.Importer.HostedServices;

public interface IObservableHostedService : IHostedService
{
    Guid InstanceId { get; }

    string Name { get; }
    string GetServiceInfo();
}
