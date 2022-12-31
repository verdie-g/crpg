namespace Crpg.Module.Rating;

internal class CrpgPlayerRating
{
    private const float DefaultRating = 1500.0f; // TODO: use constants.json
    private const float Multiplier = 173.7178f;

    public CrpgPlayerRating(float rating, float deviation, float volatility)
    {
        Rating = rating;
        RatingDeviation = deviation;
        Volatility = volatility;
    }

    public float Rating { get; private set; }
    public float RatingDeviation { get; private set; }

    /// <remarks>The volatility is the same using Glicko and Glicko2 so there is no Glicko2Volatility.</remarks>
    public float Volatility { get; private set; }

    public float WorkingRating { get; set; }
    public float WorkingRatingDeviation { get; set; }
    public float WorkingVolatility { get; set; }

    public float Glicko2Rating
    {
        get => (Rating - DefaultRating) / Multiplier;
        private set => Rating = value * Multiplier + DefaultRating;
    }

    public float Glicko2RatingDeviation
    {
        get => RatingDeviation / Multiplier;
        private set => RatingDeviation = value * Multiplier;
    }

    public void FinalizeRating()
    {
        Glicko2Rating = WorkingRating;
        Glicko2RatingDeviation = WorkingRatingDeviation;
        Volatility = WorkingVolatility;

        WorkingRatingDeviation = 0;
        WorkingRating = 0;
        WorkingVolatility = 0;
    }
}
