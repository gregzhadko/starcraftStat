using Microsoft.EntityFrameworkCore;
using Starcraft.Stat.DataBase;

namespace Starcraft.Stat.Services;

public class MigrationsService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MigrationsService> _logger;

    public MigrationsService(IServiceProvider serviceProvider, ILogger<MigrationsService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Start migration");
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<StarcraftDbContext>();
        await dbContext.Database.MigrateAsync(stoppingToken);
    }
}