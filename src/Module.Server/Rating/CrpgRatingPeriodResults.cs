namespace Crpg.Module.Rating;

/// <summary>This class holds the results accumulated over a rating period.</summary>
internal class CrpgRatingPeriodResults
{
    private readonly List<CrpgRatingResult> _results = new();
    private readonly HashSet<CrpgPlayerRating> _participants = new();

    public void AddResult(CrpgPlayerRating winner, CrpgPlayerRating loser, float percentage)
    {
        CrpgRatingResult result = new(winner, loser, percentage);
        _results.Add(result);
    }

    public CrpgRatingResult[] GetPlayerResults(CrpgPlayerRating player)
    {
        List<CrpgRatingResult> filteredResults = new();
        foreach (var result in _results)
        {
            if (result.Winner == player || result.Loser == player)
            {
                filteredResults.Add(result);
            }
        }

        return filteredResults.ToArray();
    }

    public IEnumerable<CrpgPlayerRating> GetParticipants()
    {
        foreach (var result in _results)
        {
            _participants.Add(result.Winner);
            _participants.Add(result.Loser);
        }

        return _participants;
    }

    public void AddParticipant(CrpgPlayerRating rating)
    {
        _participants.Add(rating);
    }

    public void Clear()
    {
        _results.Clear();
        _participants.Clear();
    }
}
