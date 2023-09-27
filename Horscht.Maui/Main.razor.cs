using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Horscht.App.Services;
using Microsoft.AspNetCore.Components;

namespace Horscht.Maui;
public partial class Main
{
    [Inject]
    private IAuthenticationService? AuthenticationService { get; set; }

    protected override async Task OnInitializedAsync()
    {
        if (AuthenticationService is not null)
        {
            await AuthenticationService.TryLoginSilentAsync(CancellationToken.None);
        }
    }
}
