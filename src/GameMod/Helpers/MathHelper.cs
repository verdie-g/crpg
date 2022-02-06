namespace Crpg.GameMod.Helpers;

public static class MathHelper
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
}
