using Microsoft.EntityFrameworkCore;
using Starcraft.Stat.DataBase;
using ILogger = Serilog.ILogger;

namespace Starcraft.Stat.Services;

public class MigrationsService(IServiceProvider serviceProvider, ILogger logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.Information("Starting migration");
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<StarcraftDbContext>();
        await dbContext.Database.MigrateAsync(stoppingToken);
    }
}