// ReSharper disable InconsistentNaming

using System.Diagnostics.CodeAnalysis;

namespace Crpg.Module.Rating;

[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1312:Variable names should begin with lower-case letter", Justification = "a and A are used in the formulas")]
[SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1204:Static elements should appear before instance elements", Justification = "It's ok")]
public class RatingCalculator
{
    private const double DefaultRating = 1500.0;
    private const double DefaultDeviation = 350;
    private const double DefaultVolatility = 0.06;
    private const double DefaultTau = 0.75;
    private const double Multiplier = 173.7178;
    private const double ConvergenceTolerance = 0.000001;

    /// <summary>
    /// Run through all players within a resultset and calculate their new ratings.
    /// Players within the resultset who did not compete during the rating period
    /// will have see their deviation increase (in line with Prof Glickman's paper).
    /// Note that this method will clear the results held in the association result set.
    /// </summary>
    public void UpdateRatings(RatingPeriodResults results)
    {
        foreach (var player in results.GetParticipants())
        {
            if (results.GetResults(player).Count > 0)
            {
                CalculateNewRating(player, results.GetResults(player));
            }
            else
            {
                // if a player does not compete during the rating period, then only Step 6 applies.
                // the player's rating and volatility parameters remain the same but deviation increases
                player.WorkingRating = player.GetGlicko2Rating();
                player.WorkingRatingDeviation = CalculateNewRatingDeviation(player.GetGlicko2RatingDeviation(), player.GetVolatility());
                player.WorkingVolatility = player.GetVolatility();
            }
        }

        // now iterate through the participants and confirm their new ratings
        foreach (var player in results.GetParticipants())
        {
            player.FinaliseRating();
        }

        // lastly, clear the result set down in anticipation of the next rating period
        results.Clear();
    }

    /// <summary>
    /// Converts from the value used within the algorithm to a rating in
    /// the same range as traditional Elo et al.
    /// </summary>
    /// <param name="rating">Rating in Glicko-2 scale.</param>
    /// <returns>Rating in Glicko scale.</returns>
    public double ConvertRatingToOriginalGlickoScale(double rating)
    {
        return rating * Multiplier + DefaultRating;
    }

    /// <summary>
    /// Converts from a rating in the same range as traditional Elo
    /// et al to the value used within the algorithm.
    /// </summary>
    /// <param name="rating">Rating in Glicko scale.</param>
    /// <returns>Rating in Glicko-2 scale.</returns>
    public double ConvertRatingToGlicko2Scale(double rating)
    {
        return (rating - DefaultRating) / Multiplier;
    }

    /// <summary>
    /// Converts from the value used within the algorithm to a
    /// rating deviation in the same range as traditional Elo et al.
    /// </summary>
    public double ConvertRatingDeviationToOriginalGlickoScale(double ratingDeviation)
    {
        return ratingDeviation * Multiplier;
    }

    /// <summary>
    /// Converts from a rating deviation in the same range as traditional Elo et al
    /// to the value used within the algorithm.
    /// </summary>
    /// <param name="ratingDeviation">Rating deviation in Glicko scale.</param>
    /// <returns>Rating deviation in Glicko-2 scale.</returns>
    public double ConvertRatingDeviationToGlicko2Scale(double ratingDeviation)
    {
        return ratingDeviation / Multiplier;
    }

    /// <summary>
    /// This is the function processing described in step 5 of Glickman's paper.
    /// </summary>
    private void CalculateNewRating(Rating player, IList<RatingResult> results)
    {
        double phi = player.GetGlicko2RatingDeviation();
        double sigma = player.GetVolatility();
        double a = Math.Log(Math.Pow(sigma, 2));
        double delta = Delta(player, results);
        double v = V(player, results);

        // step 5.2 - set the initial values of the iterative algorithm to come in step 5.4
        double A = a;
        double B;
        if (Math.Pow(delta, 2) > Math.Pow(phi, 2) + v)
        {
            B = Math.Log(Math.Pow(delta, 2) - Math.Pow(phi, 2) - v);
        }
        else
        {
            double k = 1;
            B = a - k * Math.Abs(DefaultTau);

            while (F(B, delta, phi, v, a, DefaultTau) < 0)
            {
                k++;
                B = a - k * Math.Abs(DefaultTau);
            }
        }

        // step 5.3
        double fA = F(A, delta, phi, v, a, DefaultTau);
        double fB = F(B, delta, phi, v, a, DefaultTau);

        // step 5.4
        while (Math.Abs(B - A) > ConvergenceTolerance)
        {
            double C = A + (A - B) * fA / (fB - fA);
            double fC = F(C, delta, phi, v, a, DefaultTau);

            if (fC * fB < 0)
            {
                A = B;
                fA = fB;
            }
            else
            {
                fA /= 2.0;
            }

            B = C;
            fB = fC;
        }

        double newSigma = Math.Exp(A / 2.0);

        player.SetWorkingVolatility(newSigma);

        // Step 6
        double phiStar = CalculateNewRatingDeviation(phi, newSigma);

        // Step 7
        double newPhi = 1.0 / Math.Sqrt(1.0 / Math.Pow(phiStar, 2) + 1.0 / v);

        // note that the newly calculated rating values are stored in a "working" area in the Rating object
        // this avoids us attempting to calculate subsequent participants' ratings against a moving target
        player.SetWorkingRating(
            player.GetGlicko2Rating() + Math.Pow(newPhi, 2) * OutcomeBasedRating(player, results));
        player.WorkingRatingDeviation = newPhi;
    }

    private static double F(double x, double delta, double phi, double v, double a, double tau)
    {
        return Math.Exp(x)
            * (Math.Pow(delta, 2) - Math.Pow(phi, 2) - v - Math.Exp(x))
            / (2.0 * Math.Pow(Math.Pow(phi, 2) + v + Math.Exp(x), 2))
            - (x - a) / Math.Pow(tau, 2);
    }

    private static double G(double deviation)
    {
        return 1.0 / Math.Sqrt(1.0 + 3.0 * Math.Pow(deviation, 2) / Math.Pow(Math.PI, 2));
    }

    /// <summary>
    /// This is the second sub-function of step 3 of Glickman's paper.
    /// </summary>
    private static double E(double playerRating, double opponentRating, double opponentDeviation)
    {
        return 1.0 / (1.0 + Math.Exp(-1.0 * G(opponentDeviation) * (playerRating - opponentRating)));
    }

    /// <summary>
    /// This is the main function in step 3 of Glickman's paper.
    /// </summary>
    private static double V(Rating player, IEnumerable<RatingResult> results)
    {
        double v = 0.0;

        foreach (var result in results)
        {
            v += result.Percentage
                * (Math.Pow(G(result.GetOpponent(player).GetGlicko2RatingDeviation()), 2)
                    * E(player.GetGlicko2Rating(), result.GetOpponent(player).GetGlicko2Rating(), result.GetOpponent(player).GetGlicko2RatingDeviation())
                    * (1.0 - E(player.GetGlicko2Rating(), result.GetOpponent(player).GetGlicko2Rating(), result.GetOpponent(player).GetGlicko2RatingDeviation())));
        }

        return Math.Pow(v, -1);
    }

    /// <summary>This is a formula as per step 4 of Glickman's paper.</summary>
    private double Delta(Rating player, IList<RatingResult> results)
    {
        return V(player, results) * OutcomeBasedRating(player, results);
    }

    /// <summary>This is a formula as per step 4 of Glickman's paper.</summary>
    /// <returns>Expected rating based on outcomes.</returns>
    private static double OutcomeBasedRating(Rating player, IEnumerable<RatingResult> results)
    {
        double outcomeBasedRating = 0;

        foreach (var result in results)
        {
            outcomeBasedRating += G(
                result.Percentage * result.GetOpponent(player).GetGlicko2RatingDeviation() * result.GetScore(player)
                - E(player.GetGlicko2Rating(), result.GetOpponent(player).GetGlicko2Rating(), result.GetOpponent(player).GetGlicko2RatingDeviation()));
        }

        return outcomeBasedRating;
    }

    /// <summary>
    /// This is the formula defined in step 6. It is also used for players who have not competed during the rating period.
    /// </summary>
    /// <returns>New rating deviation.</returns>
    private static double CalculateNewRatingDeviation(double phi, double sigma)
    {
        return Math.Sqrt(Math.Pow(phi, 2) + Math.Pow(sigma, 2));
    }
}
