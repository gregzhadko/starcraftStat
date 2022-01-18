namespace Starcraft.Stat.Models.Responses;

public record RacesStatisticsResponse(string Race1, string Race2, int Wins, int Losses, double WinRate) : IPretty
{
    public static string Header => $"{"Wins",-5}{"Losses",-7}{"Win Rate",-11}Team";
    public string ToPretty() => $"{$"{Wins}",-5}{$"{Losses}",-7}{$"{WinRate:N}",-11}{$"{Race1}",-15}{Race2}";
}