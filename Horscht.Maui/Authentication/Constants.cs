using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horscht.Maui.Authentication;

public static class Constants
{
    public static readonly string ClientId = "3f62331b-7209-458c-86dc-269bb6deced9";
    public static readonly string[] Scopes = new string[] { "openid", "offline_access" };

    public static readonly string TenantId = "18864c24-4dfc-418f-8866-e9f3dd9ea13c";
    /* Uncomment the next code to add B2C
   public static readonly string TenantName = "YOUR_TENANT_NAME";
   public static readonly string TenantId = $"{TenantName}.onmicrosoft.com";
   public static readonly string SignInPolicy = "B2C_1_client";
   public static readonly string AuthorityBase = $"https://{TenantName}.b2clogin.com/tfp/{TenantId}/";
   public static readonly string AuthoritySignIn = $"{AuthorityBase}{SignInPolicy}";
   */
}
