using Microsoft.AspNetCore.Authentication;
using PLATEAU.Snap.Server.Authentication;

namespace PLATEAU.Snap.Server.Extensions.DependencyInjection;

public static class AuthenticationBuilderExtensions
{
    public static AuthenticationBuilder AddApiKeyAuthentication(this AuthenticationBuilder builder)
    {
        builder.Services.AddAuthentication(Constants.ApiAuthentication.AuthenticationScheme)
            .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthHandler>(Constants.ApiAuthentication.AuthenticationScheme, _ => { });
        return builder;
    }
}
