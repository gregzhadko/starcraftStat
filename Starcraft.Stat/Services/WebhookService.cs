using Starcraft.Stat.Models;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using ILogger = Serilog.ILogger;

namespace Starcraft.Stat.Services;

public class WebhookService : IHostedService
{
    private readonly BotConfiguration _botConfig;
    private readonly ILogger _logger;
    private readonly IServiceProvider _services;

    public WebhookService(ILogger logger, IServiceProvider serviceProvider, IConfiguration configuration)
    {
        _logger = logger;
        _services = serviceProvider;
        _botConfig = configuration.GetSection("BotConfiguration").Get<BotConfiguration>();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _services.CreateScope();
        var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();

        var webhookAddress = @$"{_botConfig.HostAddress}/bot/{_botConfig.BotToken}";
        _logger.Information("Setting webhook: {WebhookAddress}", webhookAddress);
        await botClient.SetWebhookAsync(webhookAddress, allowedUpdates: Array.Empty<UpdateType>(), cancellationToken: cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        using var scope = _services.CreateScope();
        var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();

        // Remove webhook upon app shutdown
        _logger.Information("Removing webhook");
        await botClient.DeleteWebhookAsync(cancellationToken: cancellationToken);
    }
}