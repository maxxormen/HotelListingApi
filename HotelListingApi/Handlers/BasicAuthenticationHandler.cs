using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;
using System.Text;
using System.Security.Claims;
using HotelListingApi.Data;
using Microsoft.AspNetCore.Identity;

namespace HotelListingApi.Handlers;

public class BasicAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options, 
    ILoggerFactory logger, 
    UrlEncoder encoder,
    UserManager<ApplicationUser> userManager) // <-- Inject UserManager directly
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var authHeaderValue))
            return AuthenticateResult.NoResult();

        var authHeader = authHeaderValue.ToString();
        if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
            return AuthenticateResult.NoResult();

        var token = authHeader["Basic ".Length..].Trim();
        
        string decoded;
        try
        {
            var credentialBytes = Convert.FromBase64String(token);
            decoded = Encoding.UTF8.GetString(credentialBytes);
        } 
        catch
        {
            return AuthenticateResult.Fail("Invalid Basic authentication token.");
        }

        var separatorIndex = decoded.IndexOf(':');
        if (separatorIndex <= 0)
            return AuthenticateResult.Fail("Invalid Basic authentication token.");

        var userNameOrEmail = decoded[..separatorIndex];
        var password = decoded[(separatorIndex + 1)..];

        var user = await userManager.FindByEmailAsync(userNameOrEmail);

        if (user == null || !await userManager.CheckPasswordAsync(user, password))
        {
            return AuthenticateResult.Fail("Invalid username or password.");
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, userNameOrEmail)
        };
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }

    protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = 401;
        Response.Headers.Append("WWW-Authenticate", "Basic realm=\"HotelListingApi\"");
        await Response.WriteAsync("Invalid credentials.");
    }
}