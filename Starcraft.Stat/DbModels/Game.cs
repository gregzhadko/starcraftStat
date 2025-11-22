using Starcraft.Stat.Models;

namespace Starcraft.Stat.DbModels;

public class Game
{
    public int Id { get; init; }

    public int Team1Id { get; init; }
    public Team Team1 { get; init; } = null!;

    public int Team2Id { get; init; }
    public Team Team2 { get; init; } = null!;

    public Winner Winner { get; init; }

    public DateTime Date { get; init; }
}