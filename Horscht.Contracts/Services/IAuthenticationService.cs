using System.Security.Claims;
using Azure.Core;

namespace Horscht.Contracts.Services;
public interface IAuthenticationService
{
    ClaimsIdentity? CurrentUser { get; }

    event EventHandler<ClaimsIdentity?> CurrentUserChanged;

    Task TryLoginSilentAsync(CancellationToken cancellationToken);

    Task LoginInteractiveAsync(CancellationToken cancellationToken);

    Task LogoutAsync(CancellationToken cancellationToken);

    Task<AccessToken?> GetAccessTokenAsync(string[] scopes, CancellationToken cancellationToken);
}
