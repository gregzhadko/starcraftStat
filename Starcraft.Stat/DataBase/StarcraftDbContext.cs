using Microsoft.EntityFrameworkCore;
using Starcraft.Stat.Models;

namespace Starcraft.Stat.DataBase;

public class StarcraftDbContext : DbContext
{
    public StarcraftDbContext(DbContextOptions<StarcraftDbContext> options)
        : base(options)
    {
    }

    public DbSet<Race> Race { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Race>()
            .HasData(new List<Race>
            {
                new("Terran"),
                new("Zerg"),
                new("Protoss")
            });
    }
}