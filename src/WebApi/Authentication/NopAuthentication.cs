using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Trpg.WebApi.Authentication
{
    internal static class NopAuthenticationDefaults
    {
        public const string AuthenticationScheme = "Nop";
    }

    internal class NopAuthenticationOptions : AuthenticationSchemeOptions
    {
    }

    internal class NopAuthenticationHandler : SignInAuthenticationHandler<NopAuthenticationOptions>
    {
        public NopAuthenticationHandler(IOptionsMonitor<NopAuthenticationOptions> options, ILoggerFactory logger,
            UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync() => Task.FromResult(AuthenticateResult.NoResult());
        protected override Task HandleSignInAsync(ClaimsPrincipal user, AuthenticationProperties properties) => Task.CompletedTask;
        protected override Task HandleSignOutAsync(AuthenticationProperties properties) => Task.CompletedTask;
    }

    internal static class NopAuthenticationExtensions
    {
        public static AuthenticationBuilder AddNop(this AuthenticationBuilder builder)
            => builder.AddNop(NopAuthenticationDefaults.AuthenticationScheme);

        public static AuthenticationBuilder AddNop(this AuthenticationBuilder builder, string authenticationScheme)
            => builder.AddScheme<NopAuthenticationOptions, NopAuthenticationHandler>(authenticationScheme,
                null, _ => { });
    }
}