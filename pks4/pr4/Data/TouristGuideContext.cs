using Microsoft.EntityFrameworkCore;
using PKS4.Pr4.Models;

namespace PKS4.Pr4.Data;

public class TouristGuideContext : DbContext
{
    public TouristGuideContext(DbContextOptions<TouristGuideContext> options)
        : base(options)
    {
    }

    public DbSet<City> Cities => Set<City>();
    public DbSet<Attraction> Attractions => Set<Attraction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<City>()
            .HasMany(city => city.Attractions)
            .WithOne(attraction => attraction.City)
            .HasForeignKey(attraction => attraction.CityId);

        modelBuilder.Entity<Attraction>()
            .Property(attraction => attraction.VisitPrice)
            .HasPrecision(10, 2);
    }
}
