namespace Starcraft.Stat.Models.Responses;

public record PlayerRaceResponse(string Player, string Race, int Wins, int Losses, double WinRate) : IPretty
{
    public static string Header => $"{"Wins",-5}{"Losses",-7}{"Win Rate",-11}{"Player",-15}Race";
    public string ToPretty() => $"{$"{Wins}",-5}{$"{Losses}",-7}{$"{WinRate:N}",-11}{$"{Player}",-15}{Race}";
}