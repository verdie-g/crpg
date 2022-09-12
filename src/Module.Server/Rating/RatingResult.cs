namespace Crpg.Module.Rating;

/// <summary>
/// Represents the result of a match between two players.
/// </summary>
public class RatingResult
{
    private const float PointsForWin = 1.0f;
    private const float PointsForLoss = 0.0f;
    private const float PointsForDraw = 0.5f;

    private readonly bool _isDraw;

    /// <summary>
    /// Initializes a new instance of the <see cref="RatingResult"/> class.
    /// Record a new result from a match between two players.
    /// </summary>
    /// <param name="winner">winner.</param>
    /// <param name="loser">loser.</param>
    /// <param name="isDraw">is it a draw.</param>
    /// <param name="percentage">percentage.</param>
    public RatingResult(Rating winner, Rating loser, float percentage, bool isDraw = false)
    {
        if (!ValidPlayers(winner, loser))
        {
            throw new ArgumentException("Players winner and loser are the same player");
        }

        Winner = winner;
        Loser = loser;
        _isDraw = isDraw;
    }

    public Rating Winner { get; }
    public Rating Loser { get; }
    public float Percentage { get; }

    /// <summary>
    /// Test whether a particular player participated in the match represented by this result.
    /// </summary>
    /// <param name="player">player.</param>
    public bool Participated(Rating player)
    {
        return player == Winner || player == Loser;
    }

    /// <summary>
    /// Returns the "score" for a match.
    /// </summary>
    /// <param name="player">player.</param>
    public float GetScore(Rating player)
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
    public Rating GetOpponent(Rating player)
    {
        Rating opponent;

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
    private static bool ValidPlayers(Rating player1, Rating player2)
    {
        return player1 != player2;
    }
}
