namespace Crpg.Module.Rating;

/// <summary>
/// Holds an individual's Glicko-2 rating.
/// Glicko-2 ratings are an average skill value, a standard deviation and a volatility (how consistent the player is).
/// Glickman's paper on the algorithm allows scaling of these values to be more directly comparable with existing rating
/// systems such as Elo or USCF's derivation thereof. This implementation outputs ratings at this larger scale.
/// </summary>
internal class CrpgRating
{
    private const double DefaultRating = 1500.0;
    private const double Multiplier = 173.7178;

    private double _rating;
    private double _ratingDeviation;

    public CrpgRating(double rating, double deviation, double volatility)
    {
        _rating = rating;
        _ratingDeviation = deviation;
        Volatility = volatility;
    }

    public double Volatility { get; private set; }

    // the following variables are used to hold values temporarily whilst running calculations
    public double WorkingRating { get; set; }
    public double WorkingRatingDeviation { get; set; }
    public double WorkingVolatility { get; set; }

    /// <summary>
    /// Average skill value of the player scaled down to the scale used by the algorithm's internal workings.
    /// </summary>
    public double Glicko2Rating
    {
        get => (_rating - DefaultRating) / Multiplier;
        private set => _rating = value * Multiplier + DefaultRating;
    }

    /// <summary>
    /// Rating deviation of the player scaled down to the scale used by the algorithm's internal workings.
    /// </summary>
    public double Glicko2RatingDeviation
    {
        get => _ratingDeviation / Multiplier;
        private set => _ratingDeviation = value * Multiplier;
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
