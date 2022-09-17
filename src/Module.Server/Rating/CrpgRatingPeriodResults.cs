namespace Crpg.Module.Rating;

/// <summary>
/// This class holds the results accumulated over a rating period.
/// </summary>
internal class CrpgRatingPeriodResults
{
    private readonly List<CrpgRatingResult> _results = new();
    private readonly HashSet<CrpgRating> _participants = new();

    public void AddResult(CrpgRating winner, CrpgRating loser, float percentage)
    {
        CrpgRatingResult result = new(winner, loser, percentage);
        _results.Add(result);
    }

    public IList<CrpgRatingResult> GetPlayerResults(CrpgRating player)
    {
        List<CrpgRatingResult> filteredResults = new();

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
    public IEnumerable<CrpgRating> GetParticipants()
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
    public void AddParticipant(CrpgRating rating)
    {
        _participants.Add(rating);
    }

    public void Clear()
    {
        _results.Clear();
        _participants.Clear();
    }
}
