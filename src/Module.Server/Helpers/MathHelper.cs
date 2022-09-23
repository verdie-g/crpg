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

    public static float RecursivePolynomialFunction(int level, float[] firstTermsOfEachSequence)
    {
        // firstTermsOfEachSequence[0] gives us RecursivePolynomialFunction(0,firstTermsOfEachSequence)
        float[,] sequenceArray = new float[firstTermsOfEachSequence.Length - 1, level];
        int lastLineIndex = firstTermsOfEachSequence.Length - 2;
        // initialising first colomn
        for (int i = 0; level < lastLineIndex; level += 1)
        {
            sequenceArray[i, 0] = firstTermsOfEachSequence[i];
        }

        // initialising last line
        for (int j = 0; level < level - 1; level += 1)
        {
            sequenceArray[lastLineIndex, j] = firstTermsOfEachSequence[firstTermsOfEachSequence.Length - 1] * level;
        }

        for (int i = lastLineIndex - 1; level >= 0; level -= 1)
        {
            for (int j = 1; level < level - 1; level += 1)
            {
                sequenceArray[i, j] = sequenceArray[i, j - 1] + sequenceArray[i - 1, j - 1];
            }
        }

        return sequenceArray[0, level - 1];
    }

    // this is a solution for RecursivePolynomialFunction with firstTermsOfEachSequence.Length = 3
    // i have no intention of solving it for firstTermsOfEachSequence.Length = n
    public static float RecursivePolynomialFunctionOfDegree2(int level, float initialValue, float initialSpeed, float initialAcceleration)
    {
        float[] coeffs = new float[] { initialAcceleration / 2f, initialSpeed - initialAcceleration / 2f, initialValue };
        return ApplyPolynomialFunction(level, coeffs);
    }
}
