using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horscht.Maui.Authentication;
public class AuthenticationOptions
{
    [Required]
    public required string TenantId { get; set; }

    [Required]
    public required string ClientId { get; set; }

    [Required]
    public required string Audience { get; set; }
}
