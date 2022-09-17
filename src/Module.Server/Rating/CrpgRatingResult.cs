namespace Crpg.Module.Rating;

/// <summary>
/// Represents the result of a match between two players.
/// </summary>
internal class CrpgRatingResult
{
    private const float PointsForWin = 1.0f;
    private const float PointsForLoss = 0.0f;
    private const float PointsForDraw = 0.5f;

    private readonly bool _isDraw;

    /// <summary>
    /// Initializes a new instance of the <see cref="CrpgRatingResult"/> class.
    /// Record a new result from a match between two players.
    /// </summary>
    /// <param name="winner">winner.</param>
    /// <param name="loser">loser.</param>
    /// <param name="isDraw">is it a draw.</param>
    /// <param name="percentage">percentage.</param>
    public CrpgRatingResult(CrpgRating winner, CrpgRating loser, float percentage, bool isDraw = false)
    {
        if (!ValidPlayers(winner, loser))
        {
            throw new ArgumentException("Players winner and loser are the same player");
        }

        Winner = winner;
        Loser = loser;
        Percentage = percentage;
        _isDraw = isDraw;
    }

    public CrpgRating Winner { get; }
    public CrpgRating Loser { get; }
    public float Percentage { get; }

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
        float score;

        if (Winner == player)
        {
            score = PointsForWin;
        }
        else if (Loser == player)
        {
            score = PointsForLoss;
        }
        else
        {
            throw new ArgumentException("Player did not participate in match", nameof(player));
        }

        if (_isDraw)
        {
            score = PointsForDraw;
        }

        return score;
    }

    /// <summary>
    /// Given a particular player, returns the opponent.
    /// </summary>
    /// <param name="player">player.</param>
    public CrpgRating GetOpponent(CrpgRating player)
    {
        CrpgRating opponent;

        if (Winner == player)
        {
            opponent = Loser;
        }
        else if (Loser == player)
        {
            opponent = Winner;
        }
        else
        {
            throw new ArgumentException("Player did not participate in match", nameof(player));
        }

        return opponent;
    }

    /// <summary>
    /// Check that we're not doing anything silly like recording a match with only one player.
    /// </summary>
    /// <param name="player1">player1.</param>
    /// <param name="player2">player2.</param>
    private static bool ValidPlayers(CrpgRating player1, CrpgRating player2)
    {
        return player1 != player2;
    }
}
