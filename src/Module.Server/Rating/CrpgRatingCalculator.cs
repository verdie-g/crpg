// ReSharper disable InconsistentNaming

using System.Diagnostics.CodeAnalysis;

namespace Crpg.Module.Rating;

/// <summary>
/// A rating system using Glicko 2 (http://www.glicko.net/glicko/glicko2.pdf).
/// </summary>
[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1312:Variable names should begin with lower-case letter", Justification = "a and A are used in the formulas")]
internal static class CrpgRatingCalculator
{
    /// <summary>
    /// Constrains the change in volatility over time. Smaller values of τ prevent the volatility measures from changing
    /// by large amounts, which in turn prevent enormous changes in ratings based on very improbable results. (source:
    /// http://www.glicko.net/glicko/glicko2.pdf).
    /// </summary>
    private const float Tau = 0.75f;
    private const float ConvergenceTolerance = 0.000001f;

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
                // If a player did not compete during the period its deviation increases.
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

    private static void CalculateNewRating(CrpgPlayerRating player, CrpgRatingResult[] results)
    {
        float phi = player.Glicko2RatingDeviation;
        float sigma = player.Volatility;
        float a = (float)Math.Log(sigma * sigma);
        float delta = Delta(player, results);
        float v = V(player, results);

        // Step 5.2
        float A = a;
        float B;
        if (delta * delta > phi * phi + v)
        {
            B = (float)Math.Log(delta * delta - phi * phi - v);
        }
        else
        {
            float k = 1;
            B = a - k * Math.Abs(Tau);

            while (F(B, delta, phi, v, a) < 0)
            {
                k++;
                B = a - k * Math.Abs(Tau);
            }
        }

        // Step 5.3
        float fA = F(A, delta, phi, v, a);
        float fB = F(B, delta, phi, v, a);

        // Step 5.4
        while (Math.Abs(B - A) > ConvergenceTolerance)
        {
            float C = A + (A - B) * fA / (fB - fA);
            float fC = F(C, delta, phi, v, a);

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

        // Step 6
        float phiStar = CalculateNewRatingDeviation(phi, newSigma);

        // Step 7
        float newPhi = 1.0f / (float)Math.Sqrt(1.0 / (phiStar * phiStar) + 1.0 / v);

        player.WorkingRating = player.Glicko2Rating + (float)Math.Pow(newPhi, 2) * OutcomeBasedRating(player, results);
        player.WorkingRatingDeviation = newPhi;
        player.WorkingVolatility = newSigma;
    }

    // Step 5.1
    private static float F(float x, float delta, float phi, float v, float a)
    {
        return (float)(Math.Exp(x)
                       * (delta * delta - phi * phi - v - Math.Exp(x))
                       / (2.0 * Math.Pow(phi * phi + v + Math.Exp(x), 2))
                       - (x - a) / (Tau * Tau));
    }

    private static float G(float deviation)
    {
        return 1.0f / (float)Math.Sqrt(1.0 + 3.0 * (deviation * deviation) / (Math.PI * Math.PI));
    }

    private static float E(float playerRating, float opponentRating, float opponentDeviation)
    {
        return 1.0f / (float)(1.0 + Math.Exp(-1.0 * G(opponentDeviation) * (playerRating - opponentRating)));
    }

    private static float V(CrpgPlayerRating player, CrpgRatingResult[] results)
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

    private static float Delta(CrpgPlayerRating player, CrpgRatingResult[] results)
    {
        return V(player, results) * OutcomeBasedRating(player, results);
    }

    private static float OutcomeBasedRating(CrpgPlayerRating player, CrpgRatingResult[] results)
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

    private static float CalculateNewRatingDeviation(float phi, float sigma)
    {
        return (float)Math.Sqrt(phi * phi + sigma * sigma);
    }
}
