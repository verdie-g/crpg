namespace Crpg.Module.Rating;

/// <summary>
/// This class holds the results accumulated over a rating period.
/// </summary>
public class RatingPeriodResults
{
    private readonly List<RatingResult> _results = new();
    private readonly HashSet<Rating> _participants = new();

    public void AddResult(Rating winner, Rating loser, float percentage)
    {
        var result = new RatingResult(winner, loser, percentage);

        _results.Add(result);
    }

    /// <summary>
    /// Get a list of the results for a given player.
    /// </summary>
    /// <param name="player">player.</param>
    public IList<RatingResult> GetResults(Rating player)
    {
        var filteredResults = new List<RatingResult>();

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
            _participants.Add(result.Winner);
            _participants.Add(result.Loser);
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
