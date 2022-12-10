namespace Crpg.Module.Helpers;

internal static class MathHelper
{
    /// <summary>
    /// Polynomial Function.
    /// </summary>
    /// <param name="x">The Polynomial Variable.</param>
    /// <param name="coefficients">Coefficients are written in natural order [a,b,c] -> ax²+bx+c .</param>
    /// <returns>A Polynomial that has x as a variable.</returns>
    public static float ApplyPolynomialFunction(float x, float[] coefficients)
    {
        float r = 0;
        for (int degree = 0; degree < coefficients.Length; degree += 1)
        {
            int coefficientForDegreeIndex = coefficients.Length - 1 - degree;
            r += coefficients[coefficientForDegreeIndex] * (float)Math.Pow(x, degree);
        }

        return r;
    }

    // this is a solution for RecursivePolynomialFunction with firstTermsOfEachSequence.Length = 3
    // i have no intention of solving it for firstTermsOfEachSequence.Length = n
    public static float RecursivePolynomialFunctionOfDegree2(int level, float[] recursiveCoeffs)
    {
        float initialValue = recursiveCoeffs[0];
        float initialSpeed = recursiveCoeffs[1];
        float initialAcceleration = recursiveCoeffs[2];
        float[] coeffs = { initialAcceleration / 2f, initialSpeed - initialAcceleration / 2f, initialValue };
        return ApplyPolynomialFunction(level, coeffs);
    }

    /// <summary>
    /// This is a math function https://en.wikipedia.org/wiki/Generalized_mean.
    /// </summary>
    public static float PowerMean(List<float> numbers, float p)
    {
        double pSum = 0;
        foreach (float number in numbers)
        {
            pSum += Math.Pow(number, p);
        }

        return (float)Math.Pow(pSum / numbers.Count, 1.0 / p);
    }

    /// <summary>
    /// This is a math function https://en.wikipedia.org/wiki/Norm_(mathematics)#p-norm
    /// </summary>
    public static float PowerSum(List<float> numbers, float p)
    {
        double pSum = 0;
        foreach (float number in numbers)
        {
            pSum += Math.Pow(number, p);
        }

        return (float)Math.Pow(pSum, 1.0 / p);
    }
}
