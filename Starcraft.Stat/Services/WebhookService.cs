using Microsoft.Extensions.Options;
using Starcraft.Stat.Models;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using ILogger = Serilog.ILogger;

namespace Starcraft.Stat.Services;

public class WebhookService(ILogger logger, IServiceProvider serviceProvider, IOptions<BotConfiguration> botConfiguration) : IHostedService
{
    private readonly BotConfiguration _botConfig = botConfiguration.Value;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();

        var webhookAddress = $"{_botConfig.HostAddress}/bot/{_botConfig.BotToken}";
        logger.Information("Setting webhook: {WebhookAddress}", webhookAddress);
        await botClient.SetWebhookAsync(webhookAddress, allowedUpdates: Array.Empty<UpdateType>(), cancellationToken: cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();

        // Remove webhook upon app shutdown
        logger.Information("Removing webhook");
        await botClient.DeleteWebhookAsync(cancellationToken: cancellationToken);
    }
}