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

public class BotHandleService : IBotHandleService
{
    private const string AddFormat = "winner1 race winner2 race loser1 race loser2 race";
    private readonly ITelegramBotClient _botClient;
    private readonly BotConfiguration _botConfig;

    private readonly IDictionary<string, string> _commands = new Dictionary<string, string>
    {
        ["statistics"] = "Gets the statistics",
        ["addgame"] = $"Add game in format: {AddFormat}"
    };

    private readonly IGameService _gameService;
    private readonly ILogger _logger;
    private readonly IStatisticsService _statisticsService;

    public BotHandleService(ITelegramBotClient botClient, ILogger logger, IStatisticsService statisticsService, IGameService gameService, IConfiguration configuration)
    {
        _botClient = botClient;
        _logger = logger;
        _statisticsService = statisticsService;
        _gameService = gameService;
        _botConfig = configuration.GetSection("BotConfiguration").Get<BotConfiguration>();
    }

    public async Task HandleAsync(Update update)
    {
        if (update.Message == null)
        {
            _logger.Error("Message is null");
            return;
        }

        var chatId = update.Message.Chat.Id;
        _logger.Information("Message {Message} from chat {ChatId}", update.Message.Text, chatId);
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
        await _botClient.SendTextMessageAsync(chatId, starcraftException.Message, ParseMode.Markdown);
    }

    private async Task BotOnMessageReceived(Message message)
    {
        _logger.Information("Receive message type: {MessageType}", message.Type);
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
        _logger.Information("The message was sent with id: {SentMessageId}", sentMessage.MessageId);

        // Send inline keyboard
        // You can process responses in BotOnCallbackQueryReceived handler
    }

    private async Task<Message> AddGameAsync(Message message)
    {
        var allowedChats = _botConfig.AllowedChats;
        if (allowedChats.Length > 0 && !allowedChats.Contains(message.Chat.Id))
        {
            _logger.Warning("Someone tried to send {Message} from chat {Chat}, but we didn't allowed it", message.Text, message.Chat.Id);
            return await _botClient.SendTextMessageAsync(message.Chat.Id, "Only Grigory and tstk chat can add games to the statistics", ParseMode.Markdown);
        }

        var s = message.Text!.Split(' ').Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
        if (s.Length < 9)
        {
            return await _botClient.SendTextMessageAsync(message.Chat.Id, $"Not enough parameters\\. The correct format is `{AddFormat}`", ParseMode.MarkdownV2);
        }

        var request = new AddGameRequest(
            new TeamRequest(s[1], s[2], s[3], s[4]),
            new TeamRequest(s[5], s[6], s[7], s[8]),
            Winner.Team1,
            false);
        var validationResult = await new AddGameRequestValidator().ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var response = $"Validation errors:\n {string.Join("\n  ", validationResult.Errors.Select(e => e.ErrorMessage))}";
            return await _botClient.SendTextMessageAsync(message.Chat.Id, response, ParseMode.MarkdownV2);
        }

        await _gameService.AddGameAsync(request);
        var number = await _gameService.GetGamesCountAsync();
        await _botClient.SendTextMessageAsync(message.Chat.Id, $"Game №{number} is added", ParseMode.MarkdownV2);
        return await GetPrettyStatisticsAsync(message.Chat.Id);
    }

    private async Task<Message> GetPrettyStatisticsAsync(long chatId)
    {
        var statistics = await _statisticsService.GetPlayerStatisticsAsync(false, false);
        var result = $"`{statistics.ToPretty()}`";
        return await _botClient.SendTextMessageAsync(chatId, result, ParseMode.MarkdownV2);
    }

    private async Task<Message> HelpAsync(Message message)
    {
        var helpText = $"Usage:\n{string.Join('\n', _commands.Select(x => $"/{x.Key} {x.Value}"))}";
        return await _botClient.SendTextMessageAsync(message.Chat.Id, helpText, replyMarkup: new ReplyKeyboardRemove());
    }

    // Process Inline Keyboard callback data

    private Task UnknownUpdateHandlerAsync(Update update)
    {
        _logger.Information("Unknown update type: {UpdateType}", update.Type);
        return Task.CompletedTask;
    }

    private async Task HandleErrorAsync(long chatIt, Exception exception)
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        _logger.Information("HandleError: {ErrorMessage}", errorMessage);
        await _botClient.SendTextMessageAsync(chatIt, $"Грег наговнокодил: {exception.Message}");
    }

    #region Inline Mode

    #endregion
}