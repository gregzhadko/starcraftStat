using System.ComponentModel.DataAnnotations;

namespace Starcraft.Stat.DbModels;

public class Race
{
    public Race(string name)
    {
        Name = name;
    }

    [Key]
    public string Name { get; set; }
}