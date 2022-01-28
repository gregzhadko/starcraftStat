using Telegram.Bot.Types;

namespace Starcraft.Stat.Services;

public interface IBotHandleService
{
    Task HandleAsync(Update update);
}