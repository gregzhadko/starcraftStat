namespace Starcraft.Stat.Models.Responses;

public record WinLoseBase(int Wins, int Losses, double WinRate) : IPretty
{
    public int Games => Wins + Losses;
    public static string Header => $"{"Wins",-5}{"Losses",-7}{"Games",-6}{"Win Rate",-11}";
    public virtual string ToPretty() => $"{$"{Wins}",-5}{$"{Losses}",-7}{$"{Games}",-6}{$"{WinRate:N}",-11}";
}

public class WinLoseComparer : IComparer<WinLoseBase>
{
    public int Compare(WinLoseBase? x, WinLoseBase? y)
    {
        if (x!.WinRate > y!.WinRate)
        {
            return 1;
        }
        else if(x.WinRate < y.WinRate)
        {
            return -1;
        }
        else
        {
            if (x.Wins >= x.Losses) //Если WinRate больше или равен 50 процентов, то результаты с большим количеством игр должны быть в выше в списке
            {
                if (x.Games >= y.Games)
                {
                    return 1;
                }
                else
                {
                    return -1;
                }
            }
            else //Иначе результы с наименьшим количеством игр должны быть ниже
            {
                if (x.Games >= y.Games)
                {
                    return -1;
                }
                else
                {
                    return 1;
                }
            }
        }
        
    }
}
