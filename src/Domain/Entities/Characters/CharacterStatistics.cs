namespace Crpg.Domain.Entities.Characters;

public class CharacterStatistics
{
    public int Kills { get; set; }
    public int Deaths { get; set; }
    public int Assists { get; set; }
    public TimeSpan PlayTime { get; set; }
}
