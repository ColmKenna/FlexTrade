using FlexTradem.Web.Auth;
using Microsoft.AspNetCore.Authentication;

namespace FlexTradem.Web.Http;

public sealed class AccessTokenHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly TokenRefreshService _tokenRefreshService;

    public AccessTokenHandler(
        IHttpContextAccessor httpContextAccessor,
        TokenRefreshService tokenRefreshService)
    {
        _httpContextAccessor = httpContextAccessor;
        _tokenRefreshService = tokenRefreshService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null)
            return await base.SendAsync(request, cancellationToken);

        var accessToken = await httpContext.GetTokenAsync("access_token");
        SetBearerToken(request, accessToken);

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            var newToken = await _tokenRefreshService
                .RefreshAccessTokenAsync(httpContext);

            if (newToken is not null)
            {
                var retryRequest = await CloneRequestAsync(request);
                SetBearerToken(retryRequest, newToken);
                response = await base.SendAsync(retryRequest, cancellationToken);
            }
            else
            {
                httpContext.Response.Redirect("/Account/Login");
            }
        }

        return response;
    }

    private static void SetBearerToken(HttpRequestMessage request, string? token)
    {
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(
                    "Bearer", token);
        }
    }

    private static async Task<HttpRequestMessage> CloneRequestAsync(
        HttpRequestMessage original)
    {
        var clone = new HttpRequestMessage(original.Method, original.RequestUri);

        if (original.Content is not null)
        {
            var bytes = await original.Content.ReadAsByteArrayAsync();
            clone.Content = new ByteArrayContent(bytes);
            foreach (var header in original.Content.Headers)
                clone.Content.Headers.TryAddWithoutValidation(
                    header.Key, header.Value);
        }

        foreach (var header in original.Headers)
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);

        return clone;
    }
}
