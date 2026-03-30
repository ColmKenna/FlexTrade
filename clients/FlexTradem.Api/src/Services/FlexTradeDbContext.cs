using FlexTradem.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FlexTradem.Api.Services;

public sealed class FlexTradeDbContext(DbContextOptions<FlexTradeDbContext> options) : DbContext(options)
{
    public DbSet<Listing> Listings => Set<Listing>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Listing>(entity =>
        {
            entity.ToTable("Listings");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(200);
            entity.Property(x => x.CreatedUtc)
                .IsRequired();
        });
    }
}
