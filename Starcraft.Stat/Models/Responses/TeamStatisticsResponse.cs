namespace Starcraft.Stat.Models.Responses;

public record TeamStatisticsResponse(string Player1, string Player2, int Wins, int Losses, double WinRate) : IPretty
{
    public static string Header => $"{"Wins",-5}{"Losses",-7}{"Win Rate",-11}Teams";
    public string ToPretty() => $"{$"{Wins}",-5}{$"{Losses}",-7}{$"{WinRate:N}",-11}{$"{Player1}",-15}{Player2}";
}