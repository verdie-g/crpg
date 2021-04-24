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
            new IdentityResource("profile", new[] { JwtClaimTypes.PreferredUserName, JwtClaimTypes.Picture }),
        };

        public static IEnumerable<ApiScope> GetApiScopes() => new[]
        {
            // Adding user claim "role" will automatically add it to the access token for clients requesting this scope.
            new ApiScope("user_api", "cRPG User API", new[] { JwtClaimTypes.Role }),
            new ApiScope("game_api", "cRPG Game API"),
        };
    }
}
