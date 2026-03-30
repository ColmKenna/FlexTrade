var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = "https://localhost:5001";  // IdentityServer port from Phase 1
        options.Audience  = "flextrade-api";
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ListingsRead",   p => p.RequireClaim("scope", "flextrade.listings.read"));
    options.AddPolicy("ListingsWrite",  p => p.RequireClaim("scope", "flextrade.listings.write"));
    options.AddPolicy("RequestsRead",   p => p.RequireClaim("scope", "flextrade.requests.read"));
    options.AddPolicy("RequestsWrite",  p => p.RequireClaim("scope", "flextrade.requests.write"));
    options.AddPolicy("LoansRead",      p => p.RequireClaim("scope", "flextrade.loans.read"));
    options.AddPolicy("LoansManage",    p => p.RequireClaim("scope", "flextrade.loans.manage"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

var listings = app.MapGroup("/listings");
listings.MapGet("/",        () => Results.Ok("listings placeholder"))
        .RequireAuthorization("ListingsRead");
listings.MapPost("/",       () => Results.Ok("create listing placeholder"))
        .RequireAuthorization("ListingsWrite");
listings.MapPut("/{id}",    (int id) => Results.Ok($"update listing {id}"))
        .RequireAuthorization("ListingsWrite");
listings.MapDelete("/{id}", (int id) => Results.Ok($"delete listing {id}"))
        .RequireAuthorization("ListingsWrite");

var requests = app.MapGroup("/requests");
requests.MapGet("/",        () => Results.Ok("requests placeholder"))
        .RequireAuthorization("RequestsRead");
requests.MapPost("/",       () => Results.Ok("submit request placeholder"))
        .RequireAuthorization("RequestsWrite");
requests.MapDelete("/{id}", (int id) => Results.Ok($"cancel request {id}"))
        .RequireAuthorization("RequestsWrite");

var loans = app.MapGroup("/loans");
loans.MapGet("/",                   () => Results.Ok("loans placeholder"))
     .RequireAuthorization("LoansRead");
loans.MapPost("/{id}/approve", (int id) => Results.Ok($"approve loan {id}"))
     .RequireAuthorization("LoansManage");
loans.MapPost("/{id}/reject",  (int id) => Results.Ok($"reject loan {id}"))
     .RequireAuthorization("LoansManage");
loans.MapPost("/{id}/close",   (int id) => Results.Ok($"close loan {id}"))
     .RequireAuthorization("LoansManage");

app.Run();
