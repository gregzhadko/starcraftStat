using System.ComponentModel.DataAnnotations;

namespace Starcraft.Stat.DbModels;

public class Player(int id, string name)
{
    [Key]
    public int Id { get; init; } = id;

    [MaxLength(20)]
    public string Name { get; init; } = name;
}