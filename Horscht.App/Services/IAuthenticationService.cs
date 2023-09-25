using System.Security.Claims;

namespace Horscht.App.Services;
public interface IAuthenticationService
{
    ClaimsIdentity? CurrentUser { get; }

    event EventHandler<ClaimsIdentity?> CurrentUserChanged;

    Task LoginAsync(CancellationToken cancellationToken);

    Task LogoutAsync(CancellationToken cancellationToken);
}
