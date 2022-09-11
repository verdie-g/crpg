using System;

namespace Crpg.Module.Balancing;

/// <summary>
/// Represents the result of a match between two players.
/// </summary>
public class Result
{
    private const double PointsForWin = 1.0;
    private const double PointsForLoss = 0.0;
    private const double PointsForDraw = 0.5;

    private readonly bool _isDraw;
    private readonly Rating _winner;
    private readonly Rating _loser;
    private readonly float _percentage;

    /// <summary>
    /// Record a new result from a match between two players.
    /// </summary>
    /// <param name="winner"></param>
    /// <param name="loser"></param>
    /// <param name="isDraw"></param>
    public Result(Rating winner, Rating loser,float percentage, bool isDraw = false)
    {
        if (!ValidPlayers(winner, loser))
        {
            throw new ArgumentException("Players winner and loser are the same player");
        }

        _winner = winner;
        _loser = loser;
        _isDraw = isDraw;
    }

    /// <summary>
    /// Check that we're not doing anything silly like recording a match with only one player.
    /// </summary>
    /// <param name="player1"></param>
    /// <param name="player2"></param>
    /// <returns></returns>
    private static bool ValidPlayers(Rating player1, Rating player2)
    {
        return player1 != player2;
    }

    /// <summary>
    /// Test whether a particular player participated in the match represented by this result.
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    public bool Participated(Rating player)
    {
        return player == _winner || player == _loser;
    }

    /// <summary>
    /// Returns the "score" for a match.
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    public double GetScore(Rating player)
    {
        double score;

        if (_winner == player)
        {
            score = PointsForWin;
        }
        else if (_loser == player)
        {
            score = PointsForLoss;
        }
        else
        {
            throw new ArgumentException("Player did not participate in match", "player");
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
    /// <param name="player"></param>
    /// <returns></returns>
    public Rating GetOpponent(Rating player)
    {
        Rating opponent;

        if (_winner == player)
        {
            opponent = _loser;
        }
        else if (_loser == player)
        {
            opponent = _winner;
        }
        else
        {
            throw new ArgumentException("Player did not participate in match", "player");
        }

        return opponent;
    }

    public Rating GetWinner()
    {
        return _winner;
    }

    public Rating GetLoser()
    {
        return _loser;
    }
    public float GetPercentage()
    {
        return _percentage;
    }
}
