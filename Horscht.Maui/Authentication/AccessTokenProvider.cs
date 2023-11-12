using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Horscht.Contracts.Services;

namespace Horscht.Maui.Authentication;
internal class AccessTokenProvider : IAccessTokenProvider
{
    private readonly IAuthenticationService _authenticationService;

    public AccessTokenProvider(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    public ValueTask<AccessTokenResult> RequestAccessToken()
    {
        var user = _authenticationService.CurrentUser;
        var accessTokenClaim = user?.Claims.FirstOrDefault(c => c.Type == "at");
        var accessTokenExpiryClaim = user?.Claims.FirstOrDefault(c => c.Type == "ate");
        if (accessTokenClaim is not null && accessTokenExpiryClaim is not null)
        {
            var accessToken = new AccessToken
            {
                Value = accessTokenClaim.Value,
                Expires = DateTimeOffset.Parse(accessTokenExpiryClaim.Value)
            };
            var result = new AccessTokenResult(
                AccessTokenResultStatus.Success,
                accessToken,
                string.Empty,
                new InteractiveRequestOptions
                {
                    Interaction = InteractionType.GetToken,
                    ReturnUrl = string.Empty
                });
            return new ValueTask<AccessTokenResult>(result);
        }

        return new ValueTask<AccessTokenResult>((AccessTokenResult)null!);
    }

    public ValueTask<AccessTokenResult> RequestAccessToken(AccessTokenRequestOptions options)
    {
        return RequestAccessToken();
    }
}
