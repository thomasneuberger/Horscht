using Azure.Core;

namespace Horscht.App.Authentication;
internal class AccessTokenCredential : TokenCredential
{
    private readonly AccessToken _accessToken;

    public AccessTokenCredential(AccessToken accessToken)
    {
        _accessToken = accessToken;
    }

    public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
    {
        return new ValueTask<AccessToken>(GetToken(requestContext, cancellationToken));
    }

    public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
    {
        return _accessToken;
    }
}
