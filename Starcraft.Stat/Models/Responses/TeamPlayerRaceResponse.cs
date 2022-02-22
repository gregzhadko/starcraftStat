namespace Starcraft.Stat.Models.Responses;

public record TeamPlayerRaceResponse(string Player1, string Race1, string Player2, string Race2, int Wins, int Losses, double WinRate) : IPretty
{
    public static string Header => $"{"Wins",-5}{"Losses",-7}{"Win Rate",-11}{"Player1",-15}{"Race1",-15}{"Player2",-15}{"Race2",-5}";
    public string ToPretty() => $"{$"{Wins}",-5}{$"{Losses}",-7}{$"{WinRate:N}",-11}{$"{Player1}",-15}{Race1,-15}{$"{Player2}",-15}{Race2,-5}";
}