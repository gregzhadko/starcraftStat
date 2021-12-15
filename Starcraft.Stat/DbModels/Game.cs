using Starcraft.Stat.Models;

namespace Starcraft.Stat.DbModels;

public class Game
{
    public int Id { get; set; }

    public int Team1Id { get; set; }
    public Team Team1 { get; set; } = null!;

    public int Team2Id { get; set; }
    public Team Team2 { get; set; } = null!;

    public Winner Winner { get; set; }

    public DateOnly Date { get; set; }
}