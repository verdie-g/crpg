namespace Crpg.Domain.Entities.Characters;

public class CharacterRating
{
    public float Value { get; set; }
    public float Deviation { get; set; }

    /// <summary>
    /// The volatility measure indicates the degree of expected fluctuation in a player’s rating. The volatility measure
    /// is high when a player has erratic performances (e.g., when the player has had exceptionally strong results after
    /// a period of stability), and the volatility measure is low when the player performs at a consistent level
    /// (source: http://www.glicko.net/glicko/glicko2.pdf).
    /// </summary>
    public float Volatility { get; set; }
}
