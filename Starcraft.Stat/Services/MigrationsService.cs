using Microsoft.EntityFrameworkCore;
using Starcraft.Stat.DataBase;

namespace Starcraft.Stat.Services;

public class MigrationsService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public MigrationsService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("Start migration");
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<StarcraftDbContext>();
        await dbContext.Database.MigrateAsync(stoppingToken);
    }
}