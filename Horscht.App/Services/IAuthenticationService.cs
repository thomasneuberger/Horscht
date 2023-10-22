using System.Security.Claims;

namespace Horscht.App.Services;
public interface IAuthenticationService
{
    ClaimsIdentity? CurrentUser { get; }

    event EventHandler<ClaimsIdentity?> CurrentUserChanged;

    Task TryLoginSilentAsync(CancellationToken cancellationToken);

    Task LoginInteractiveAsync(CancellationToken cancellationToken);

    Task LogoutAsync(CancellationToken cancellationToken);
    Task<string?> GetAccessTokenAsync(string[] scopes, CancellationToken cancellationToken);
}
