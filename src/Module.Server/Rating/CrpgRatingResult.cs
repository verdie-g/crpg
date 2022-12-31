namespace Crpg.Module.Rating;

/// <summary>Represents a player hitting another.</summary>
internal class CrpgRatingResult
{
    public CrpgRatingResult(CrpgPlayerRating winner, CrpgPlayerRating loser, float percentage)
    {
        Winner = winner;
        Loser = loser;
        Percentage = percentage;
    }

    public CrpgPlayerRating Winner { get; }
    public CrpgPlayerRating Loser { get; }
    public float Percentage { get; }

    public float GetScore(CrpgPlayerRating player)
    {
        if (Winner == player)
        {
            return 1;
        }

        if (Loser == player)
        {
            return 0;
        }

        throw new InvalidOperationException();
    }

    public CrpgPlayerRating GetOpponent(CrpgPlayerRating player)
    {
        if (Winner == player)
        {
            return Loser;
        }

        if (Loser == player)
        {
            return Winner;
        }

        throw new InvalidOperationException();
    }
}
