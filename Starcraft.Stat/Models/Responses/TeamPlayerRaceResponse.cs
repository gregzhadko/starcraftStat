namespace Starcraft.Stat.Models.Responses;

public record TeamPlayerRaceResponse(string Player1, string Race1, string Player2, string Race2, int Wins, int Losses, double WinRate) : WinLoseBase(Wins, Losses, WinRate)
{
    public new static string Header => $"{WinLoseBase.Header}{"Player1",-15}{"Race1",-15}{"Player2",-15}{"Race2",-5}";
    public override string ToPretty() => $"{base.ToPretty()}{$"{Player1}",-15}{Race1,-15}{$"{Player2}",-15}{Race2,-5}";
}