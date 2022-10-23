namespace Crpg.Module.Rating;

/// <summary>
/// Represents a player hitting another.
/// </summary>
internal class CrpgRatingResult
{
    public CrpgRatingResult(CrpgRating winner, CrpgRating loser, float score)
    {
        Winner = winner;
        Loser = loser;
        Score = score;
    }

    public CrpgRating Winner { get; }
    public CrpgRating Loser { get; }
    public float Score { get; }

    /// <summary>
    /// Test whether a particular player participated in the match represented by this result.
    /// </summary>
    /// <param name="player">player.</param>
    public bool Participated(CrpgRating player)
    {
        return player == Winner || player == Loser;
    }

    /// <summary>
    /// Returns the "score" for a match.
    /// </summary>
    /// <param name="player">player.</param>
    public float GetScore(CrpgRating player)
    {
        if (Winner == player)
        {
            return Score;
        }

        if (Loser == player)
        {
            return 0;
        }

        throw new ArgumentException("Player did not participate in match", nameof(player));
    }

    /// <summary>
    /// Given a particular player, returns the opponent.
    /// </summary>
    /// <param name="player">player.</param>
    public CrpgRating GetOpponent(CrpgRating player)
    {
        if (Winner == player)
        {
            return Loser;
        }

        if (Loser == player)
        {
            return Winner;
        }

        throw new ArgumentException("Player did not participate in match", nameof(player));
    }
}
