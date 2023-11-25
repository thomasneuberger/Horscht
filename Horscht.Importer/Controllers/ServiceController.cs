using Horscht.Importer.HostedServices;
using Microsoft.AspNetCore.Mvc;

namespace Horscht.Importer.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ServiceController : ControllerBase
{
    [HttpGet]
    [Produces<IDictionary<string, Guid>>]
    public IActionResult Get([FromServices]IEnumerable<IHostedService> services)
    {
        var serviceDescriptions = services
            .OfType<IObservableHostedService>()
            .ToDictionary(s => s.Name, s => s.InstanceId);

        return Ok(serviceDescriptions);
    }

    [HttpGet("{id}")]
    [Produces<string>]
    public IActionResult Get([FromServices]IEnumerable<IHostedService> services, Guid id)
    {
        var observableService = services
            .OfType<IObservableHostedService>()
            .FirstOrDefault(s => s.InstanceId == id);

        if (observableService is null)
        {
            return NotFound();
        }

        var info = observableService.GetServiceInfo();

        return Ok(info);
    }
}
