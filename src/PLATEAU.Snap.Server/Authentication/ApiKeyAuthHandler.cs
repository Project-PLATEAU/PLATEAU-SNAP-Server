using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace PLATEAU.Snap.Server.Authentication;

public class ApiKeyAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly AppSettings settings;

    public ApiKeyAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, AppSettings settings) : base(options, logger, encoder)
    {
        this.settings = settings;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Context.Request.Headers.TryGetValue(Constants.ApiAuthentication.ApiKeyHeader, out var apiKey) || apiKey.ToString() != this.settings.ApiKey)
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid API Key"));
        }

        // ユーザによってAPIキーを変えて、ユーザを識別する必要がある場合はここに実装する
        var name = "valid user";
        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, name, ClaimValueTypes.String) }, Constants.ApiAuthentication.AuthenticationScheme));

        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, Constants.ApiAuthentication.AuthenticationScheme)));
    }
}
