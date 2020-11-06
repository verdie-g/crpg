using System.Collections.Generic;
using IdentityModel;
using IdentityServer4.Models;

namespace Crpg.WebApi.Identity
{
    internal class IdentityServerConfig
    {
        public static IEnumerable<IdentityResource> GetIdentityResources() => new[]
        {
            new IdentityResources.OpenId(),
        };

        public static IEnumerable<ApiScope> GetApiScopes() => new[]
        {
            // Adding user claim "role" will automatically add it to the access token for clients requesting this scope.
            new ApiScope("api", "cRPG API", new[] { JwtClaimTypes.Role }),
        };
    }
}
