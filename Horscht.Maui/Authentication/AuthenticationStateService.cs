using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Horscht.App.Services;
using System.Security.Claims;

namespace Horscht.Maui.Authentication;
internal class AuthenticationStateService : AuthenticationStateProvider, IDisposable
{
    private readonly IAuthenticationService _authenticationService;

    private Task<AuthenticationState> _authenticationState;

    public AuthenticationStateService(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;

        var user = _authenticationService.CurrentUser;
        var principal = user is not null ? new ClaimsPrincipal(user) : new ClaimsPrincipal();

        _authenticationState = Task.FromResult(new AuthenticationState(principal));

        _authenticationService.CurrentUserChanged += OnCurrentUserChanged;
    }

    private void OnCurrentUserChanged(object? sender, ClaimsIdentity? user)
    {
        var principal = user is not null ? new ClaimsPrincipal(user) : new ClaimsPrincipal();

        _authenticationState = Task.FromResult(new AuthenticationState(principal));

        NotifyAuthenticationStateChanged(_authenticationState);
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        return _authenticationState;
    }

    public void Dispose()
    {
        _authenticationState.Dispose();
        _authenticationService.CurrentUserChanged -= OnCurrentUserChanged;
    }
}
