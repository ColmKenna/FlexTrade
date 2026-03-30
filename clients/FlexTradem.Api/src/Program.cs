using FlexTradem.Api.Models;
using FlexTradem.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<FlexTradeDbContext>(options =>
{
        var connectionString = builder.Configuration.GetConnectionString("FlexTradeDb")
                ?? "Server=localhost,1433;Database=FlexTrade;User Id=sa;Password=YourStrong!Passw0rd;MultipleActiveResultSets=true;Encrypt=False;TrustServerCertificate=True";

        options.UseSqlServer(connectionString);
});

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

using (var scope = app.Services.CreateScope())
{
        var db = scope.ServiceProvider.GetRequiredService<FlexTradeDbContext>();
        db.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

var listings = app.MapGroup("/listings");
listings.MapGet("/", async (FlexTradeDbContext db) =>
        {
                var allListings = await db.Listings
                        .OrderByDescending(x => x.CreatedUtc)
                        .ToListAsync();

                return Results.Ok(allListings);
        })
        .RequireAuthorization("ListingsRead");
listings.MapPost("/", async (CreateListingRequest request, FlexTradeDbContext db) =>
        {
                if (string.IsNullOrWhiteSpace(request.Title))
                {
                        return Results.ValidationProblem(new Dictionary<string, string[]>
                        {
                                ["title"] = ["Title is required."]
                        });
                }

                var listing = new Listing
                {
                        Title = request.Title.Trim(),
                        CreatedUtc = DateTime.UtcNow
                };

                db.Listings.Add(listing);
                await db.SaveChangesAsync();

                return Results.Created($"/listings/{listing.Id}", listing);
        })
        .RequireAuthorization("ListingsWrite");
listings.MapPut("/{id}", async (int id, CreateListingRequest request, FlexTradeDbContext db) =>
        {
                if (string.IsNullOrWhiteSpace(request.Title))
                {
                        return Results.ValidationProblem(new Dictionary<string, string[]>
                        {
                                ["title"] = ["Title is required."]
                        });
                }

                var listing = await db.Listings.FindAsync(id);
                if (listing is null)
                {
                        return Results.NotFound();
                }

                listing.Title = request.Title.Trim();
                await db.SaveChangesAsync();

                return Results.Ok(listing);
        })
        .RequireAuthorization("ListingsWrite");
listings.MapDelete("/{id}", async (int id, FlexTradeDbContext db) =>
        {
                var listing = await db.Listings.FindAsync(id);
                if (listing is null)
                {
                        return Results.NotFound();
                }

                db.Listings.Remove(listing);
                await db.SaveChangesAsync();

                return Results.NoContent();
        })
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
