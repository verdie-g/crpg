namespace Crpg.Module.Helpers;

internal static class MathHelper
{
    public static float ApplyPolynomialFunction(float x, float[] coefficients)
    {
        float r = 0;
        for (int degree = 0; degree < coefficients.Length; degree += 1)
        {
            r += coefficients[coefficients.Length - degree - 1] * (float)Math.Pow(x, degree);
        }

        return r;
    }

    public static float GeneralizedMean(int n, float[] numbers)
    {
        float normN = (float)numbers.Sum(x => Math.Pow(x, n)) / n;
        return (float)Math.Pow(normN, 1f / n);
    }

    // this is a solution for RecursivePolynomialFunction with firstTermsOfEachSequence.Length = 3
    // i have no intention of solving it for firstTermsOfEachSequence.Length = n
    public static float RecursivePolynomialFunctionOfDegree2(int level, float[] recursiveCoeffs)
    {
        float initialValue = recursiveCoeffs[0];
        float initialSpeed = recursiveCoeffs[1];
        float initialAcceleration = recursiveCoeffs[2];
        float[] coeffs = new float[] { initialAcceleration / 2f, initialSpeed - initialAcceleration / 2f, initialValue };
        return ApplyPolynomialFunction(level, coeffs);
    }
}
