using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Resonanse.Api.Auth;

public class BasicAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "Basic";

    private readonly BasicAuthOptions _authOptions;

    public BasicAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IOptions<BasicAuthOptions> authOptions)
        : base(options, logger, encoder)
    {
        _authOptions = authOptions.Value;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (string.IsNullOrWhiteSpace(_authOptions.Username) ||
            string.IsNullOrWhiteSpace(_authOptions.Password))
        {
            return Task.FromResult(AuthenticateResult.Fail(
                "Basic auth is not configured. Set Resonanse:Auth:Username and Password."));
        }

        if (!Request.Headers.TryGetValue("Authorization", out var authHeader))
            return Task.FromResult(AuthenticateResult.NoResult());

        var header = authHeader.ToString();
        if (!header.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
            return Task.FromResult(AuthenticateResult.NoResult());

        string decoded;
        try
        {
            var encoded = header["Basic ".Length..].Trim();
            var bytes = Convert.FromBase64String(encoded);
            decoded = Encoding.UTF8.GetString(bytes);
        }
        catch
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid Authorization header."));
        }

        var sep = decoded.IndexOf(':');
        if (sep < 0)
            return Task.FromResult(AuthenticateResult.Fail("Invalid credentials format."));

        var user = decoded[..sep];
        var pass = decoded[(sep + 1)..];

        if (!FixedTimeEquals(user, _authOptions.Username) ||
            !FixedTimeEquals(pass, _authOptions.Password))
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid username or password."));
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, user),
            new Claim(ClaimTypes.Role, "Admin")
        };
        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.Headers["WWW-Authenticate"] = "Basic realm=\"Resonanse\", charset=\"UTF-8\"";
        return base.HandleChallengeAsync(properties);
    }

    private static bool FixedTimeEquals(string a, string b)
    {
        if (a.Length != b.Length) return false;
        var result = 0;
        for (int i = 0; i < a.Length; i++)
            result |= a[i] ^ b[i];
        return result == 0;
    }
}