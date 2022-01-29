using Starcraft.Stat.Exceptions;
using Starcraft.Stat.Models;
using Starcraft.Stat.Models.Requests;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace Starcraft.Stat.Services;

public class BotHandleService : IBotHandleService
{
    private const string AddFormat = "winner1 race winner2 race loser1 race loser2 race";
    private readonly ITelegramBotClient _botClient;

    private readonly IDictionary<string, string> _commands = new Dictionary<string, string>
    {
        ["statistics"] = "Gets the statistics",
        ["addgame"] = $"Add game in format: {AddFormat}"
    };

    private readonly IGameService _gameService;
    private readonly ILogger<BotHandleService> _logger;
    private readonly IStatisticsService _statisticsService;
    private readonly BotConfiguration _botConfig;

    public BotHandleService(ITelegramBotClient botClient, ILogger<BotHandleService> logger, IStatisticsService statisticsService, IGameService gameService, IConfiguration configuration)
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
            _logger.LogError("Message is null");
            return;
        }

        //TODO: check chat id and prevent to call if from other chats
        var chatId = update.Message.Chat.Id;
        _logger.LogInformation("Message {Message} from chat {ChatId}", update.Message.Text, chatId);
        var handler = update.Type switch
        {
            // UpdateType.Unknown:
            // UpdateType.ChannelPost:
            // UpdateType.EditedChannelPost:
            // UpdateType.ShippingQuery:
            // UpdateType.PreCheckoutQuery:
            // UpdateType.Poll:
            UpdateType.Message => BotOnMessageReceived(update.Message),
            // UpdateType.EditedMessage => BotOnMessageReceived(update.EditedMessage!),
            // UpdateType.CallbackQuery => BotOnCallbackQueryReceived(update.CallbackQuery!),
            // UpdateType.InlineQuery => BotOnInlineQueryReceived(update.InlineQuery!),
            // UpdateType.ChosenInlineResult => BotOnChosenInlineResultReceived(update.ChosenInlineResult!),
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
            await HandleErrorAsync(exception);
        }
    }

    private async Task HandleStarcraftExceptionAsync(StarcraftException starcraftException, long chatId)
    {
        await _botClient.SendTextMessageAsync(chatId, starcraftException.Message, ParseMode.Markdown);
    }

    private async Task BotOnMessageReceived(Message message)
    {
        _logger.LogInformation("Receive message type: {MessageType}", message.Type);
        if (message.Type != MessageType.Text)
        {
            return;
        }
        
        var action = message.Text!.Split(' ')[0].ToLower() switch
        {
            "/statistics" => GetPrettyStatisticsAsync(message.Chat.Id),
            "/addgame" => AddGameAsync(message),
            _ => HelpAsync(message)
        };
        var sentMessage = await action;
        _logger.LogInformation("The message was sent with id: {SentMessageId}", sentMessage.MessageId);

        // Send inline keyboard
        // You can process responses in BotOnCallbackQueryReceived handler
        static async Task<Message> SendInlineKeyboard(ITelegramBotClient bot, Message message)
        {
            await bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

            // Simulate longer running task
            await Task.Delay(500);

            InlineKeyboardMarkup inlineKeyboard = new(
                new[]
                {
                    // first row
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("1.1", "11"),
                        InlineKeyboardButton.WithCallbackData("1.2", "12")
                    },
                    // second row
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("2.1", "21"),
                        InlineKeyboardButton.WithCallbackData("2.2", "22")
                    }
                });

            return await bot.SendTextMessageAsync(message.Chat.Id,
                "Choose",
                replyMarkup: inlineKeyboard);
        }

        static async Task<Message> SendReplyKeyboard(ITelegramBotClient bot, Message message)
        {
            ReplyKeyboardMarkup replyKeyboardMarkup = new(
                new[]
                {
                    new KeyboardButton[] { "1.1", "1.2" },
                    new KeyboardButton[] { "2.1", "2.2" }
                })
            {
                ResizeKeyboard = true
            };

            return await bot.SendTextMessageAsync(message.Chat.Id,
                "Choose",
                replyMarkup: replyKeyboardMarkup);
        }

        static async Task<Message> RemoveKeyboard(ITelegramBotClient bot, Message message)
        {
            return await bot.SendTextMessageAsync(message.Chat.Id,
                "Removing keyboard",
                replyMarkup: new ReplyKeyboardRemove());
        }

        static async Task<Message> SendFile(ITelegramBotClient bot, Message message)
        {
            await bot.SendChatActionAsync(message.Chat.Id, ChatAction.UploadPhoto);

            const string filePath = @"Files/tux.png";
            await using FileStream fileStream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var fileName = filePath.Split(Path.DirectorySeparatorChar).Last();

            return await bot.SendPhotoAsync(message.Chat.Id,
                new InputOnlineFile(fileStream, fileName),
                "Nice Picture");
        }

        static async Task<Message> RequestContactAndLocation(ITelegramBotClient bot, Message message)
        {
            ReplyKeyboardMarkup requestReplyKeyboard = new(
                new[]
                {
                    KeyboardButton.WithRequestLocation("Location"),
                    KeyboardButton.WithRequestContact("Contact")
                });

            return await bot.SendTextMessageAsync(message.Chat.Id,
                "Who or Where are you?",
                replyMarkup: requestReplyKeyboard);
        }
    }

    private async Task<Message> AddGameAsync(Message message)
    {
        if (!_botConfig.AllowedChats.Contains(message.Chat.Id))
        {
            _logger.LogWarning("Someone tried to send {Message} from chat {Chat}, but we didn't allowed it", message.Text, message.Chat.Id);
            return await _botClient.SendTextMessageAsync(message.Chat.Id, "Only Grigory and tstk chat can add games to the statistics", ParseMode.Markdown);
        }
        
        var a = message.Text!.Split(' ');
        if (a.Length < 9)
        {
            return await _botClient.SendTextMessageAsync(message.Chat.Id, $"Not enough parameters\\. The correct format is `{AddFormat}`", ParseMode.MarkdownV2);
        }

        var request = new AddGameRequest(
            new TeamRequest(a[1], a[2], a[3], a[4]),
            new TeamRequest(a[5], a[6], a[7], a[8]),
            Winner.Team1,
            false);
        var validationResult = await new AddGameRequestValidator().ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var response = $"Validation errors:\n {string.Join("\n  ", validationResult.Errors.Select(e => e.ErrorMessage))}";
            return await _botClient.SendTextMessageAsync(message.Chat.Id, response, ParseMode.MarkdownV2);
        }

        await _gameService.AddGameAsync(request);
        return await GetPrettyStatisticsAsync(message.Chat.Id);
    }

    private async Task<Message> GetPrettyStatisticsAsync(long chatId)
    {
        //TODO: handle if we need to include history of matches
        var statistics = await _statisticsService.GetPlayerStatisticsAsync(false);
        var result = $"`{statistics.ToPretty()}`";
        return await _botClient.SendTextMessageAsync(chatId, result, ParseMode.MarkdownV2);
    }

    private async Task<Message> HelpAsync(Message message)
    {
        var helpText = $"Usage:\n{string.Join('\n', _commands.Select(x => $"/{x.Key} {x.Value}"))}";
        return await _botClient.SendTextMessageAsync(message.Chat.Id, helpText, replyMarkup: new ReplyKeyboardRemove());
    }

    // Process Inline Keyboard callback data
    private async Task BotOnCallbackQueryReceived(CallbackQuery callbackQuery)
    {
        await _botClient.AnswerCallbackQueryAsync(callbackQuery.Id, $"Received {callbackQuery.Data}");
        await _botClient.SendTextMessageAsync(callbackQuery.Message!.Chat.Id, $"Received {callbackQuery.Data}");
    }

    private Task UnknownUpdateHandlerAsync(Update update)
    {
        _logger.LogInformation("Unknown update type: {UpdateType}", update.Type);
        return Task.CompletedTask;
    }

    private Task HandleErrorAsync(Exception exception)
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        _logger.LogInformation("HandleError: {ErrorMessage}", errorMessage);
        return Task.CompletedTask;
    }

    #region Inline Mode

    private async Task BotOnInlineQueryReceived(InlineQuery inlineQuery)
    {
        _logger.LogInformation("Received inline query from: {InlineQueryFromId}", inlineQuery.From.Id);

        InlineQueryResult[] results =
        {
            // displayed result
            new InlineQueryResultArticle(
                "3",
                "TgBots",
                new InputTextMessageContent(
                    "hello"
                )
            )
        };

        await _botClient.AnswerInlineQueryAsync(inlineQuery.Id,
            results,
            isPersonal: true,
            cacheTime: 0);
    }

    private Task BotOnChosenInlineResultReceived(ChosenInlineResult chosenInlineResult)
    {
        _logger.LogInformation("Received inline result: {ChosenInlineResultId}", chosenInlineResult.ResultId);
        return Task.CompletedTask;
    }

    #endregion
}