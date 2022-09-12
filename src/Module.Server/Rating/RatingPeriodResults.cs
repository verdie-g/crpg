using System.Collections.Generic;

namespace Crpg.Module.Balancing;

/// <summary>
/// This class holds the results accumulated over a rating period.
/// </summary>
public class RatingPeriodResults
{
    private readonly List<Result> _results = new();
    private readonly HashSet<Rating> _participants = new();

    /// <summary>
    /// Create an empty result set.
    /// </summary>
    public RatingPeriodResults()
    {
    }

    /// <summary>
    /// Constructor that allows you to initialise the list of participants.
    /// </summary>
    /// <param name="participants">participants.</param>
    public RatingPeriodResults(HashSet<Rating> participants)
    {
        _participants = participants;
    }

    /// <summary>
    /// Add a result to the set.
    /// </summary>
    /// <param name="winner">winner.</param>
    /// <param name="loser">loser.</param>
    /// <param name="percentage">percentage.</param>
    public void AddResult(Rating winner, Rating loser, float percentage)
    {
        var result = new Result(winner, loser, percentage);

        _results.Add(result);
    }

    /// <summary>
    /// Record a draw between two players and add to the set.
    /// </summary>
    /// <param name="player1">player1.</param>
    /// <param name="player2">player2.</param>
    /// <param name="percentage">percentage.</param>
    public void AddDraw(Rating player1, Rating player2, float percentage)
    {
        var result = new Result(player1, player2, percentage, true);

        _results.Add(result);
    }

    /// <summary>
    /// Get a list of the results for a given player.
    /// </summary>
    /// <param name="player">player.</param>
    public IList<Result> GetResults(Rating player)
    {
        var filteredResults = new List<Result>();

        foreach (var result in _results)
        {
            if (result.Participated(player))
            {
                filteredResults.Add(result);
            }
        }

        return filteredResults;
    }

    /// <summary>
    /// Get all the participants whose results are being tracked.
    /// </summary>
    public IEnumerable<Rating> GetParticipants()
    {
        // Run through the results and make sure all players have been pushed into the participants set.
        foreach (var result in _results)
        {
            _participants.Add(result.GetWinner());
            _participants.Add(result.GetLoser());
        }

        return _participants;
    }

    /// <summary>
    /// Add a participant to the rating period, e.g. so that their rating will
    /// still be calculated even if they don't actually compete.
    /// </summary>
    /// <param name="rating">rating.</param>
    public void AddParticipant(Rating rating)
    {
        _participants.Add(rating);
    }

    /// <summary>
    /// Clear the result set.
    /// </summary>
    public void Clear()
    {
        _results.Clear();
    }
}
