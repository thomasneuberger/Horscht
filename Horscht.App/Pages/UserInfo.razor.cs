using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace Horscht.App.Pages;
public partial class UserInfo
{
    [Inject]
    private AuthenticationStateProvider? AuthenticationStateProvider { get; set; }

    private Claim[] _claims = Array.Empty<Claim>();

    protected override async Task OnInitializedAsync()
    {
        if (AuthenticationStateProvider is not null)
        {
            var state = await AuthenticationStateProvider.GetAuthenticationStateAsync();

            _claims = state.User.Claims.ToArray();
        }
    }
}
