using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Core;

namespace Horscht.Logic.Services;
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
