namespace Starcraft.Stat.Models.Responses;

public record WinLoseBase(int Wins, int Losses, double WinRate) : IPretty
{
    public int Games => Wins + Losses;
    public static string Header => $"{"Wins",-5}{"Losses",-7}{"Games",-6}{"Win Rate",-11}";
    public virtual string ToPretty() => $"{$"{Wins}",-5}{$"{Losses}",-7}{$"{Games}",-6}{$"{WinRate:N}",-11}";
}