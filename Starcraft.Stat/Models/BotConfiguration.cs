namespace Starcraft.Stat.Models;

public class BotConfiguration
{
    public string BotToken { get; init; } = null!;
    public string HostAddress { get; init; } = null!;
    public string AllowedChatsString { get; init; } = null!;

    public long[] AllowedChats
    {
        get
        {
            if (field != null)
            {
                return field;
            }

            if (string.IsNullOrWhiteSpace(AllowedChatsString))
            {
                return [];
            }

            return field = AllowedChatsString.Split(',').Select(s => Convert.ToInt64(s)).ToArray();
        }
    }
}