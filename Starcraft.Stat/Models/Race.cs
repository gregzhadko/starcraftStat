using System.ComponentModel.DataAnnotations;

namespace Starcraft.Stat.Models;

public class Race
{
    public Race(string name)
    {
        Name = name;
    }

    [Key]
    public string Name { get; set; }
}