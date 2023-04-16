using System.Security.Claims;
using Crpg.Application.Common.Results;
using Crpg.Application.Restrictions.Queries;
using Crpg.Application.Users.Models;
using Crpg.Application.Users.Queries;
using MediatR;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.WebApi.Controllers;

[Route("[controller]")]
public class ConnectController : ControllerBase
{
    private static readonly ILogger Logger = LoggerFactory.CreateLogger<ConnectController>();

    private readonly IOpenIddictApplicationManager _applicationManager;
    private readonly IOpenIddictAuthorizationManager _authorizationManager;
    private readonly IOpenIddictScopeManager _scopeManager;

    public ConnectController(
        IOpenIddictApplicationManager applicationManager,
        IOpenIddictAuthorizationManager authorizationManager,
        IOpenIddictScopeManager scopeManager)
    {
        _applicationManager = applicationManager;
        _authorizationManager = authorizationManager;
        _scopeManager = scopeManager;
    }

    [HttpGet("authorize")]
    [HttpPost("authorize")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Authorize()
    {
        var request = HttpContext.GetOpenIddictServerRequest()
                      ?? throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        // Try to retrieve the user principal stored in the authentication cookie and redirect
        // the user agent to the login page (or to an external provider) in the following cases:
        //
        //  - If the user principal can't be extracted or the cookie is too old.
        //  - If prompt=login was specified by the client application.
        //  - If a max_age parameter was provided and the authentication cookie is not considered "fresh" enough.
        AuthenticateResult result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        if (!result.Succeeded
            || request.HasPrompt(OpenIddictConstants.Prompts.Login)
            || (request.MaxAge != null
                && result.Properties?.IssuedUtc != null
                && DateTimeOffset.UtcNow - result.Properties.IssuedUtc > TimeSpan.FromSeconds(request.MaxAge.Value)))
        {
            return RedirectToLogin(request);
        }

        int userId = GetUserIdFromPrincipal(result.Principal);
        object application = await _applicationManager.FindByClientIdAsync(request.ClientId!)
                             ?? throw new InvalidOperationException($"Client {request.ClientId} not found");

        // Retrieve the permanent authorizations associated with the user and the calling client application.
        List<object> authorizations = new();
        await foreach (object authorization in _authorizationManager.FindAsync(
                           subject: userId.ToString(),
                           client: (await _applicationManager.GetIdAsync(application))!,
                           status: OpenIddictConstants.Statuses.Valid,
                           type: OpenIddictConstants.AuthorizationTypes.Permanent,
                           scopes: request.GetScopes()))
        {
            authorizations.Add(authorization);
        }

        string? consentType = await _applicationManager.GetConsentTypeAsync(application);
        switch (consentType)
        {
            // If the consent is external (e.g when authorizations are granted by a sysadmin), immediately return an
            // error if no authorization can be found in the database.
            case OpenIddictConstants.ConsentTypes.External when authorizations.Count == 0:
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string?>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.ConsentRequired,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The logged in user is not allowed to access this client application.",
                    }));

            // If the consent is implicit or if an authorization was found, return an authorization response without
            // displaying the consent form.
            case OpenIddictConstants.ConsentTypes.Implicit:
            case OpenIddictConstants.ConsentTypes.External when authorizations.Count != 0:
            case OpenIddictConstants.ConsentTypes.Explicit when authorizations.Count != 0 && !request.HasPrompt(OpenIddictConstants.Prompts.Consent):

                var user = (await GetUserAsync(userId)).Data;

                // Create the claims-based identity that will be used by OpenIddict to generate tokens.
                ClaimsIdentity identity = new(TokenValidationParameters.DefaultAuthenticationType);

                // Add the claims that will be persisted in the tokens.
                identity.SetClaim(OpenIddictConstants.Claims.Subject, user!.Id.ToString());
                identity.SetClaim(OpenIddictConstants.Claims.Role, user.Role.ToString());

                identity.SetScopes(request.GetScopes());
                await foreach (string scope in _scopeManager.ListResourcesAsync(identity.GetScopes()))
                {
                    identity.SetResources(scope);
                }

                // Automatically create a permanent authorization to avoid requiring explicit consent
                // for future authorization or token requests containing the same scopes.
                object? authorization = authorizations.LastOrDefault();
                authorization ??= await _authorizationManager.CreateAsync(
                    identity: identity,
                    subject: user.Id.ToString(),
                    client: (await _applicationManager.GetIdAsync(application))!,
                    type: OpenIddictConstants.AuthorizationTypes.Permanent,
                    scopes: identity.GetScopes());

                identity.SetAuthorizationId(await _authorizationManager.GetIdAsync(authorization));
                identity.SetDestinations(GetDestinations);

                return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            // At this point, no authorization was found in the database and an error must be returned
            // if the client application specified prompt=none in the authorization request.
            case OpenIddictConstants.ConsentTypes.Explicit when request.HasPrompt(OpenIddictConstants.Prompts.None):
            case OpenIddictConstants.ConsentTypes.Systematic when request.HasPrompt(OpenIddictConstants.Prompts.None):
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string?>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.ConsentRequired,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "Interactive user consent is required.",
                    }));

            // In every other case, render the consent form.
            default:
                throw new NotImplementedException();
        }
    }

    [HttpGet("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        // Returning a SignOutResult will ask OpenIddict to redirect the user agent to the post_logout_redirect_uri
        // specified by the client application or to the RedirectUri specified in the authentication properties if none
        // was set.
        return SignOut(
            authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
            properties: new AuthenticationProperties { RedirectUri = "/" });
    }

    [HttpPost("token")]
    [IgnoreAntiforgeryToken]
    [Produces("application/json")]
    public Task<IActionResult> Token()
    {
        var request = HttpContext.GetOpenIddictServerRequest()
                      ?? throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        if (request.IsClientCredentialsGrantType())
        {
            return TokenClientCredentialsGrantType(request);
        }

        if (request.IsAuthorizationCodeGrantType() || request.IsRefreshTokenGrantType())
        {
            return TokenAuthorizationCodeGrantType();
        }

        return Task.FromResult<IActionResult>(Forbid(
            authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
            properties: new AuthenticationProperties(new Dictionary<string, string?>
            {
                [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.UnsupportedGrantType,
                [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The specified grant type is not supported.",
            })));
    }

    private IActionResult RedirectToLogin(OpenIddictRequest request)
    {
        // If the client application requested promptless authentication, return an error indicating that the user
        // is not logged in.
        if (request.HasPrompt(OpenIddictConstants.Prompts.None))
        {
            return Forbid(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.LoginRequired,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user is not logged in.",
                }));
        }

#if false
        if (request.IdentityProvider == null)
        {
            return Forbid(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidRequest,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The specified identity provider is not valid.",
                }));
        }
#endif

        // To avoid endless login -> authorization redirects, the prompt=login flag is removed from the
        // authorization request payload before redirecting the user.
        Dictionary<string, StringValues> parameters = new(Request.Query) { ["prompt"] = "continue" };

        return Challenge(
            authenticationSchemes: CookieAuthenticationDefaults.AuthenticationScheme,
            properties: new AuthenticationProperties
            {
                RedirectUri = Request.PathBase + Request.Path + QueryString.Create(parameters),
                // TODO: pass request.IdentityProvider
            });
    }

    private async Task<IActionResult> TokenClientCredentialsGrantType(OpenIddictRequest request)
    {
        object application = await _applicationManager.FindByClientIdAsync(request.ClientId!)
                             ?? throw new InvalidOperationException($"Client {request.ClientId} not found");

        // Create the claims-based identity that will be used by OpenIddict to generate tokens.
        ClaimsIdentity identity = new(TokenValidationParameters.DefaultAuthenticationType);

        // Add the claims that will be persisted in the tokens (use the client_id as the subject identifier).
        identity.AddClaim(OpenIddictConstants.Claims.Subject, (await _applicationManager.GetClientIdAsync(application))!);

        // Set the list of scopes granted to the client application in access_token.
        identity.SetScopes(request.GetScopes());
        await foreach (string scope in _scopeManager.ListResourcesAsync(identity.GetScopes()))
        {
            identity.SetResources(scope);
        }

        identity.SetDestinations(GetDestinations);

        return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    private async Task<IActionResult> TokenAuthorizationCodeGrantType()
    {
        // Retrieve the claims principal stored in the authorization code/refresh token.
        var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

        // Retrieve the user profile corresponding to the authorization code/refresh token.
        int userId = GetUserIdFromPrincipal(result.Principal!);
        var res = await GetUserAsync(userId);
        if (res.Errors != null)
        {
            return Forbid(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidGrant,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The token is no longer valid.",
                }));
        }

        var user = res.Data!;

        if (await IsUserBannedAsync(user.Id))
        {
            Logger.LogInformation("User '{0}' could not get access token: banned", user.Id);
            return Forbid(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidGrant,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user is no longer allowed to sign in.",
                }));
        }

        Logger.LogInformation("User '{0}' got access token", user.Id);

        var identity = new ClaimsIdentity(result.Principal!.Claims,
            authenticationType: TokenValidationParameters.DefaultAuthenticationType,
            nameType: OpenIddictConstants.Claims.Name,
            roleType: OpenIddictConstants.Claims.Role);

        // Override the user claims present in the principal in case they changed since the authorization code/refresh
        // token was issued.
        identity.SetClaim(OpenIddictConstants.Claims.Role, user.Role.ToString());
        identity.SetDestinations(GetDestinations);

        // Returning a SignInResult will ask OpenIddict to issue the appropriate access/identity tokens.
        return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    private IEnumerable<string> GetDestinations(Claim claim)
    {
        // Note: by default, claims are NOT automatically included in the access and identity tokens.
        // To allow OpenIddict to serialize them, you must attach them a destination, that specifies
        // whether they should be included in access tokens, in identity tokens or in both.

        switch (claim.Type)
        {
            case OpenIddictConstants.Claims.Role:
                yield return OpenIddictConstants.Destinations.AccessToken;

                if (claim.Subject!.HasScope(OpenIddictConstants.Permissions.Scopes.Roles))
                {
                    yield return OpenIddictConstants.Destinations.IdentityToken;
                }

                yield break;

            default:
                yield return OpenIddictConstants.Destinations.AccessToken;
                yield break;
        }
    }

    private int GetUserIdFromPrincipal(ClaimsPrincipal principal)
    {
        string? sub = principal.GetClaim(OpenIddictConstants.Claims.Subject);
        if (sub == null || !int.TryParse(sub, out int userId))
        {
            throw new InvalidOperationException("Could not find a valid sub");
        }

        return userId;
    }

    private Task<Result<UserViewModel>> GetUserAsync(int userId)
    {
        var mediator = HttpContext.RequestServices.GetRequiredService<IMediator>();
        return mediator.Send(new GetUserQuery { UserId = userId }, CancellationToken.None);
    }

    private async Task<bool> IsUserBannedAsync(int userId)
    {
        var mediator = HttpContext.RequestServices.GetRequiredService<IMediator>();
        var res = await mediator.Send(new IsUserBannedQuery
        {
            UserId = userId,
        }, CancellationToken.None);

        if (res.Errors != null)
        {
            return true;
        }

        return res.Data;
    }
}
