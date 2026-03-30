using FlexTradem.Web.Auth;
using FlexTradem.Web.Http;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<TokenRefreshService>();
builder.Services.AddTransient<AccessTokenHandler>();

builder.Services.AddHttpClient("IdentityServer", client =>
    client.BaseAddress = new Uri(
        builder.Configuration["IdentityServer:Authority"]!));

builder.Services.AddHttpClient("FlexTradeApi", client =>
    client.BaseAddress = new Uri("https://localhost:7198"))
    .AddHttpMessageHandler<AccessTokenHandler>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "Cookies";
    options.DefaultChallengeScheme = "oidc";
})
.AddCookie("Cookies")
.AddOpenIdConnect("oidc", options =>
{
    options.Authority = builder.Configuration["IdentityServer:Authority"];
    options.ClientId = builder.Configuration["IdentityServer:ClientId"];
    options.ClientSecret = builder.Configuration["IdentityServer:ClientSecret"];
    options.ResponseType = "code";

    options.SaveTokens = true;
    options.Scope.Add("offline_access");
    options.Scope.Add("flextrade.listings.read");
    options.Scope.Add("flextrade.listings.write");
    options.Scope.Add("flextrade.requests.read");
    options.Scope.Add("flextrade.requests.write");
    options.Scope.Add("flextrade.loans.read");
    options.Scope.Add("flextrade.loans.manage");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
