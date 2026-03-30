using System.Collections;
using Duende.IdentityServer;
using Duende.IdentityServer.Models;

namespace IdentityServerAspNetIdentity;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        new List<IdentityResource>
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
         };


    public static IEnumerable<ApiScope> ApiScopes =>
        new List<ApiScope>
        {
            new ApiScope("flextrade.listings.read",   "Browse available listings"),
            new ApiScope("flextrade.listings.write",  "Create and manage own listings"),
            new ApiScope("flextrade.requests.read",   "View own borrow requests"),
            new ApiScope("flextrade.requests.write",  "Submit and manage borrow requests"),
            new ApiScope("flextrade.loans.read",      "View own loan history"),
            new ApiScope("flextrade.loans.manage",    "Approve, reject, or close loans"),
        };

    public static IEnumerable<ApiResource> ApiResources =>
        new List<ApiResource>
        {
            new ApiResource("flextrade-api", "FlexTradem API")
            {
                Scopes =
                {
                    "flextrade.listings.read",
                    "flextrade.listings.write",
                    "flextrade.requests.read",
                    "flextrade.requests.write",
                    "flextrade.loans.read",
                    "flextrade.loans.manage"
                }
            }
        };

    public static IEnumerable<Client> Clients =>
        new List<Client>
        {
            new Client
            {
                ClientId = "flextrade-web",
                ClientSecrets = { new Secret("change-in-production".Sha256()) },
                AllowedGrantTypes = GrantTypes.Code,
                RedirectUris           = { "https://localhost:7236/signin-oidc" },
                PostLogoutRedirectUris = { "https://localhost:7236/signout-callback-oidc" },
                AllowedScopes =
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    "flextrade.listings.read",
                    "flextrade.listings.write",
                    "flextrade.requests.read",
                    "flextrade.requests.write",
                    "flextrade.loans.read",
                    "flextrade.loans.manage"
                },
                AllowOfflineAccess = true
            }
        };

}