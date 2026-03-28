using System.ComponentModel.DataAnnotations;

namespace Horscht.Importer;

public class AzureOpenAIOptions
{
    [Required]
    public required string Endpoint { get; set; }

    [Required]
    public required string ApiKey { get; set; }

    [Required]
    public required string DeploymentName { get; set; }
}
