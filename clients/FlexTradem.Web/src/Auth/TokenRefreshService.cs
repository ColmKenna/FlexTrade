using Duende.IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace FlexTradem.Web.Auth;

public sealed class TokenRefreshService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public TokenRefreshService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    /// <summary>
    /// Exchanges the saved refresh token for a new access token and rewrites
    /// the auth cookie. Returns null if the refresh fails.
    /// </summary>
    public async Task<string?> RefreshAccessTokenAsync(HttpContext httpContext)
    {
        var refreshToken = await httpContext.GetTokenAsync("refresh_token");
        if (string.IsNullOrEmpty(refreshToken))
            return null;

        var client = _httpClientFactory.CreateClient("IdentityServer");
        var disco = await client.GetDiscoveryDocumentAsync(
            _configuration["IdentityServer:Authority"]);

        if (disco.IsError)
            return null;

        var response = await client.RequestRefreshTokenAsync(new RefreshTokenRequest
        {
            Address      = disco.TokenEndpoint,
            ClientId     = _configuration["IdentityServer:ClientId"],
            ClientSecret = _configuration["IdentityServer:ClientSecret"],
            RefreshToken = refreshToken
        });

        if (response.IsError)
            return null;

        await UpdateCookieTokensAsync(httpContext, response);

        return response.AccessToken;
    }

    private static async Task UpdateCookieTokensAsync(
        HttpContext httpContext,
        TokenResponse response)
    {
        var authenticateResult = await httpContext
            .AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        if (authenticateResult?.Principal is null)
            return;

        var properties = authenticateResult.Properties!;

        properties.UpdateTokenValue("access_token",  response.AccessToken!);
        properties.UpdateTokenValue("refresh_token", response.RefreshToken!);
        properties.UpdateTokenValue("expires_at",
            DateTimeOffset.UtcNow
                .AddSeconds(response.ExpiresIn)
                .ToString("o"));

        await httpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            authenticateResult.Principal,
            properties);
    }
}
