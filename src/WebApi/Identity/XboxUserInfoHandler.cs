using OpenIddict.Abstractions;
using OpenIddict.Client;
using OpenIddict.Client.WebIntegration;

namespace Crpg.WebApi.Identity;

/// <summary>
/// Remove this handler once https://github.com/openiddict/openiddict-core/pull/1770 is released.
/// </summary>
public class XboxUserInfoHandler : IOpenIddictClientHandler<OpenIddictClientEvents.ProcessAuthenticationContext>
{
    /// <summary>
    /// Gets the default descriptor definition assigned to this handler.
    /// </summary>
    public static OpenIddictClientHandlerDescriptor Descriptor { get; }
        = OpenIddictClientHandlerDescriptor.CreateBuilder<OpenIddictClientEvents.ProcessAuthenticationContext>()
            .UseSingletonHandler<XboxUserInfoHandler>()
            .SetOrder(OpenIddictClientHandlers.EvaluateUserinfoRequest.Descriptor.Order + 250)
            .SetType(OpenIddictClientHandlerType.BuiltIn)
            .Build();

    /// <inheritdoc/>
    public ValueTask HandleAsync(OpenIddictClientEvents.ProcessAuthenticationContext context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (IsXboxRequest(context))
        {
            context.SendUserinfoRequest = false;
        }

        return ValueTask.CompletedTask;
    }

    private bool IsXboxRequest(OpenIddictClientEvents.ProcessAuthenticationContext context)
    {
        if (context.Registration.ProviderName != OpenIddictClientWebIntegrationConstants.Providers.Microsoft)
        {
            return false;
        }

        switch (context.GrantType)
        {
            case OpenIddictConstants.GrantTypes.AuthorizationCode:
            case OpenIddictConstants.GrantTypes.Implicit:
                return context.StateTokenPrincipal != null && context.StateTokenPrincipal.GetScopes().Any(s =>
                    s.StartsWith("XboxLive.", StringComparison.OrdinalIgnoreCase));
            case OpenIddictConstants.GrantTypes.RefreshToken:
                return context.Scopes.Any(s =>
                    s.StartsWith("XboxLive.", StringComparison.OrdinalIgnoreCase));
            default:
                return false;
        }
    }
}
