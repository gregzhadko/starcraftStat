namespace Starcraft.Stat.Models;

public class BotConfiguration
{
    private long[]? _allowedChats;
    public string BotToken { get; init; } = null!;
    public string HostAddress { get; init; } = null!;
    public string AllowedChatsString { get; init; } = null!;

    public long[] AllowedChats
    {
        get
        {
            if (_allowedChats != null)
            {
                return _allowedChats;
            }

            if (string.IsNullOrWhiteSpace(AllowedChatsString))
            {
                return Array.Empty<long>();
            }

            return _allowedChats = AllowedChatsString.Split(',').Select(s => Convert.ToInt64(s)).ToArray();
        }
    }
}