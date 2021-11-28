using System.ComponentModel.DataAnnotations;

namespace Starcraft.Stat.Models;

public class Player
{
    public Player(int id, string name)
    {
        Name = name;
        Id = id;
    }

    [Key]
    public int Id { get; set; }

    public string Name { get; set; }
}