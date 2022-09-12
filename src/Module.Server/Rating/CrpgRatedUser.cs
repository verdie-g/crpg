using Crpg.Module.Common;

namespace Crpg.Module.Rating;

internal class CrpgRatedUser
{
    public CrpgRatedUser(CrpgRepresentative crpgRepresentative, RatingCalculator calculator)
    {
        CrpgRepresentative = crpgRepresentative;
        var rating = crpgRepresentative.User!.Character.Rating;
        Rating = new Rating(calculator, rating.Value, rating.Deviation, rating.Volatility);
    }

    public CrpgRepresentative CrpgRepresentative { get; }
    public Rating Rating { get; }
}
