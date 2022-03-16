namespace Starcraft.Stat.Models.Responses;

public record PlayerRaceResponse(string Player, string Race, int Wins, int Losses, double WinRate) : WinLoseBase(Wins, Losses, WinRate)
{
    public new static string Header => $"{WinLoseBase.Header}{"Player",-15}Race";
    public override string ToPretty() => $"{base.ToPretty()}{$"{Player}",-15}{Race}";
}