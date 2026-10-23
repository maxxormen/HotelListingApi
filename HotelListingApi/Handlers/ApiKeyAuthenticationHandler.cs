using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;
using System.Text;
using HotelListingApi.DTOs.Auth;
using HotelListingApi.Contracts;
using System.Security.Claims;
using HotelListingApi.Constants;

namespace HotelListingApi.Handlers;

public class ApiKeyAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, 
        ILoggerFactory logger, 
        UrlEncoder encoder,
        IApiKeyValidatorService apiKeyValidatorService) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(AuthenticationDefaults.ApiKeyHeaderName, out var headerValues))
        {
            return AuthenticateResult.NoResult();
        }

        var apiKey = headerValues.ToString();

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return AuthenticateResult.NoResult();
        }

        var valid = await apiKeyValidatorService.IsValidAsync(apiKey, Context.RequestAborted);

        if (!valid)
        {
            return AuthenticateResult.Fail("Invalid API Key.");
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "apikey"),
            new(ClaimTypes.Name, "ApiKeyClient"),
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }

    protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = 401;
        Response.Headers.Append("WWW-Authenticate", "ApiKey");
        await Response.WriteAsync("Invalid or missing API Key.");
    }
}

