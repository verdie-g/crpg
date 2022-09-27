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
        float floatn = (float)n;
        float normN = (float)numbers.Sum(x => Math.Pow(x, n)) / floatn;
        float generalizedMean = (float)Math.Pow(normN, 1 / floatn);
        return generalizedMean;
    }
}
