using Microsoft.EntityFrameworkCore;
using Starcraft.Stat.DbModels;

namespace Starcraft.Stat.DataBase;

public class StarcraftDbContext : DbContext
{
    public StarcraftDbContext(DbContextOptions<StarcraftDbContext> options)
        : base(options)
    {
    }

    public DbSet<Race> Races { get; set; } = null!;

    public DbSet<Player> Players { get; set; } = null!;
    public DbSet<Team> Teams { get; set; } = null!;
    public DbSet<Game> Games { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.EnableSensitiveDataLogging();
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Race>()
            .HasData(new List<Race>
            {
                new("Terran"),
                new("Zerg"),
                new("Protoss")
            });

        modelBuilder.Entity<Player>()
            .HasData(new List<Player>
            {
                new(1, "@gregzhadko"),
                new(2, "@Novikov_N"),
                new(3, "@sivykh"),
                new(4, "@dfomin")
            });
    }
}