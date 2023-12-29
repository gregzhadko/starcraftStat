using Microsoft.Extensions.Options;
using Starcraft.Stat.Exceptions;
using Starcraft.Stat.Models;
using Starcraft.Stat.Models.Requests;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using ILogger = Serilog.ILogger;

namespace Starcraft.Stat.Services;

public class BotHandleService(ITelegramBotClient botClient, ILogger logger, IStatisticsService statisticsService, IGameService gameService, IOptions<BotConfiguration> botConfiguration)
    : IBotHandleService
{
    private const string AddFormat = "winner1 race winner2 race loser1 race loser2 race";
    private readonly BotConfiguration _botConfig = botConfiguration.Value;

    private readonly IDictionary<string, string> _commands = new Dictionary<string, string>
    {
        ["statistics"] = "Gets the statistics",
        ["addgame"] = $"Add game in format: {AddFormat}"
    };

    public async Task HandleAsync(Update update)
    {
        if (update.Message == null)
        {
            logger.Error("Message is null");
            return;
        }

        var chatId = update.Message.Chat.Id;
        logger.Information("Message {Message} from chat {ChatId}", update.Message.Text, chatId);
        var handler = update.Type switch
        {
            UpdateType.Message => BotOnMessageReceived(update.Message),
            _ => UnknownUpdateHandlerAsync(update)
        };

        try
        {
            await handler;
        }
        catch (StarcraftException exception)
        {
            await HandleStarcraftExceptionAsync(exception, chatId);
        }
        catch (Exception exception)
        {
            await HandleErrorAsync(chatId, exception);
        }
    }

    private async Task HandleStarcraftExceptionAsync(StarcraftException starcraftException, long chatId)
    {
        await botClient.SendTextMessageAsync(chatId, starcraftException.Message, parseMode: ParseMode.Markdown);
    }

    private async Task BotOnMessageReceived(Message message)
    {
        logger.Information("Receive message type: {MessageType}", message.Type);
        if (message.Type != MessageType.Text)
        {
            return;
        }

        Task<Message>? action;
        var command = message.Text!.Split(' ')[0].ToLower();
        if (command.StartsWith("/statistics"))
        {
            action = GetPrettyStatisticsAsync(message.Chat.Id);
        }
        else if (command.StartsWith("/addgame"))
        {
            action = AddGameAsync(message);
        }
        else if (command.StartsWith("/help"))
        {
            action = HelpAsync(message);
        }
        else
        {
            return;
        }

        var sentMessage = await action;
        logger.Information("The message was sent with id: {SentMessageId}", sentMessage.MessageId);

        // Send inline keyboard
        // You can process responses in BotOnCallbackQueryReceived handler
    }

    private async Task<Message> AddGameAsync(Message message)
    {
        var allowedChats = _botConfig.AllowedChats;
        if (allowedChats.Length > 0 && !allowedChats.Contains(message.Chat.Id))
        {
            logger.Warning("Someone tried to send {Message} from chat {Chat}, but we didn't allowed it", message.Text, message.Chat.Id);
            return await botClient.SendTextMessageAsync(message.Chat.Id, "Only Grigory and tstk chat can add games to the statistics", parseMode: ParseMode.Markdown);
        }

        var s = message.Text!.Split(' ').Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
        if (s.Length < 9)
        {
            return await botClient.SendTextMessageAsync(message.Chat.Id, $"Not enough parameters\\. The correct format is `{AddFormat}`", parseMode: ParseMode.MarkdownV2);
        }

        var request = new AddGameRequest(
            new(s[1], s[2], s[3], s[4]),
            new(s[5], s[6], s[7], s[8]),
            Winner.Team1,
            false);
        var validationResult = await new AddGameRequestValidator().ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var response = $"Validation errors:\n {string.Join("\n  ", validationResult.Errors.Select(e => e.ErrorMessage))}";
            return await botClient.SendTextMessageAsync(message.Chat.Id, response, parseMode: ParseMode.MarkdownV2);
        }

        await gameService.AddGameAsync(request);
        var number = await gameService.GetGamesCountAsync();
        await botClient.SendTextMessageAsync(message.Chat.Id, $"Game №{number} is added", parseMode: ParseMode.MarkdownV2);
        return await GetPrettyStatisticsAsync(message.Chat.Id);
    }

    private async Task<Message> GetPrettyStatisticsAsync(long chatId)
    {
        var statistics = await statisticsService.GetPlayerStatisticsAsync(false, false);
        var result = $"`{statistics.ToPretty()}`";
        return await botClient.SendTextMessageAsync(chatId, result, parseMode: ParseMode.MarkdownV2);
    }

    private async Task<Message> HelpAsync(Message message)
    {
        var helpText = $"Usage:\n{string.Join('\n', _commands.Select(x => $"/{x.Key} {x.Value}"))}";
        return await botClient.SendTextMessageAsync(message.Chat.Id, helpText, replyMarkup: new ReplyKeyboardRemove());
    }

    // Process Inline Keyboard callback data

    private Task UnknownUpdateHandlerAsync(Update update)
    {
        logger.Information("Unknown update type: {UpdateType}", update.Type);
        return Task.CompletedTask;
    }

    private async Task HandleErrorAsync(long chatIt, Exception exception)
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        logger.Information("HandleError: {ErrorMessage}", errorMessage);
        await botClient.SendTextMessageAsync(chatIt, $"Грег наговнокодил: {exception.Message}");
    }
}
