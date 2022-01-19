namespace Starcraft.Stat.Models.Responses;

public record PlayerStatisticsResponse(string Name, int Wins, int Losses, double WinRate) : IPretty
{
    public static string Header => $"{"Wins",-5}{"Losses",-7}{"Win Rate",-11}Player";
    public string ToPretty() => $"{$"{Wins}",-5}{$"{Losses}",-7}{$"{WinRate:N}",-11}{Name}";
}