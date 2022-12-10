// ReSharper disable InconsistentNaming

using System.Diagnostics.CodeAnalysis;

namespace Crpg.Module.Rating;

[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1312:Variable names should begin with lower-case letter", Justification = "a and A are used in the formulas")]
internal static class CrpgRatingCalculator
{
    private const float Tau = 0.75f;
    private const float ConvergenceTolerance = 0.000001f;

    /// <summary>
    /// Run through all players within a resultset and calculate their new ratings.
    /// Players within the resultset who did not compete during the rating period
    /// will have see their deviation increase (in line with Prof Glickman's paper).
    /// Note that this method will clear the results held in the association result set.
    /// </summary>
    public static void UpdateRatings(CrpgRatingPeriodResults results)
    {
        foreach (var player in results.GetParticipants())
        {
            var playerResults = results.GetPlayerResults(player);
            if (playerResults.Length > 0)
            {
                CalculateNewRating(player, playerResults);
            }
            else
            {
                // If a player does not compete during the rating period, then only step 6 applies. The player's rating
                // and volatility parameters remain the same but deviation increases
                player.WorkingRating = player.Glicko2Rating;
                player.WorkingRatingDeviation = CalculateNewRatingDeviation(player.Glicko2RatingDeviation, player.Volatility);
                player.WorkingVolatility = player.Volatility;
            }
        }

        foreach (var player in results.GetParticipants())
        {
            player.FinalizeRating();
        }
    }

    /// <summary>
    /// This is the function processing described in step 5 of Glickman's paper.
    /// </summary>
    private static void CalculateNewRating(CrpgRating player, CrpgRatingResult[] results)
    {
        float phi = player.Glicko2RatingDeviation;
        float sigma = player.Volatility;
        float a = (float)Math.Log(Math.Pow(sigma, 2));
        float delta = Delta(player, results);
        float v = V(player, results);

        // step 5.2 - set the initial values of the iterative algorithm to come in step 5.4
        float A = a;
        float B;
        if (Math.Pow(delta, 2) > Math.Pow(phi, 2) + v)
        {
            B = (float)Math.Log(Math.Pow(delta, 2) - Math.Pow(phi, 2) - v);
        }
        else
        {
            float k = 1;
            B = a - k * Math.Abs(Tau);

            while (F(B, delta, phi, v, a, Tau) < 0)
            {
                k++;
                B = a - k * Math.Abs(Tau);
            }
        }

        // step 5.3
        float fA = F(A, delta, phi, v, a, Tau);
        float fB = F(B, delta, phi, v, a, Tau);

        // step 5.4
        while (Math.Abs(B - A) > ConvergenceTolerance)
        {
            float C = A + (A - B) * fA / (fB - fA);
            float fC = F(C, delta, phi, v, a, Tau);

            if (fC * fB < 0)
            {
                A = B;
                fA = fB;
            }
            else
            {
                fA /= 2.0f;
            }

            B = C;
            fB = fC;
        }

        float newSigma = (float)Math.Exp(A / 2.0f);

        player.WorkingVolatility = newSigma;

        // Step 6
        float phiStar = CalculateNewRatingDeviation(phi, newSigma);

        // Step 7
        float newPhi = 1.0f / (float)Math.Sqrt(1.0 / Math.Pow(phiStar, 2) + 1.0 / v);

        // Note that the newly calculated rating values are stored in a "working" area in the Rating object
        // this avoids us attempting to calculate subsequent participants' ratings against a moving target.
        player.WorkingRating = player.Glicko2Rating + (float)Math.Pow(newPhi, 2) * OutcomeBasedRating(player, results);
        player.WorkingRatingDeviation = newPhi;
    }

    private static float F(float x, float delta, float phi, float v, float a, float tau)
    {
        return (float)(Math.Exp(x)
            * (Math.Pow(delta, 2) - Math.Pow(phi, 2) - v - Math.Exp(x))
            / (2.0 * Math.Pow(Math.Pow(phi, 2) + v + Math.Exp(x), 2))
            - (x - a) / Math.Pow(tau, 2));
    }

    private static float G(float deviation)
    {
        return 1.0f / (float)Math.Sqrt(1.0 + 3.0 * Math.Pow(deviation, 2) / Math.Pow(Math.PI, 2));
    }

    /// <summary>
    /// This is the second sub-function of step 3 of Glickman's paper.
    /// </summary>
    private static float E(float playerRating, float opponentRating, float opponentDeviation)
    {
        return 1.0f / (float)(1.0 + Math.Exp(-1.0 * G(opponentDeviation) * (playerRating - opponentRating)));
    }

    /// <summary>
    /// This is the main function in step 3 of Glickman's paper.
    /// </summary>
    private static float V(CrpgRating player, CrpgRatingResult[] results)
    {
        double v = 0.0;

        foreach (var result in results)
        {
            var opponent = result.GetOpponent(player);
            v += result.Percentage * Math.Pow(G(opponent.Glicko2RatingDeviation), 2)
                 * E(player.Glicko2Rating, opponent.Glicko2Rating, opponent.Glicko2RatingDeviation)
                 * (1.0 - E(player.Glicko2Rating, opponent.Glicko2Rating, opponent.Glicko2RatingDeviation));
        }

        return (float)Math.Pow(v, -1);
    }

    /// <summary>This is a formula as per step 4 of Glickman's paper.</summary>
    private static float Delta(CrpgRating player, CrpgRatingResult[] results)
    {
        return V(player, results) * OutcomeBasedRating(player, results);
    }

    /// <summary>This is a formula as per step 4 of Glickman's paper.</summary>
    /// <returns>Expected rating based on outcomes.</returns>
    private static float OutcomeBasedRating(CrpgRating player, CrpgRatingResult[] results)
    {
        float outcomeBasedRating = 0;
        foreach (var result in results)
        {
            var opponent = result.GetOpponent(player);
            outcomeBasedRating += result.Percentage *
                G(opponent.Glicko2RatingDeviation)
                * (result.GetScore(player)
                - E(player.Glicko2Rating, opponent.Glicko2Rating, opponent.Glicko2RatingDeviation));
        }

        return outcomeBasedRating;
    }

    /// <summary>
    /// This is the formula defined in step 6. It is also used for players who have not competed during the rating period.
    /// </summary>
    /// <returns>New rating deviation.</returns>
    private static float CalculateNewRatingDeviation(float phi, float sigma)
    {
        return (float)Math.Sqrt(Math.Pow(phi, 2) + Math.Pow(sigma, 2));
    }
}
