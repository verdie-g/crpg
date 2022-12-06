using System;
using System.Collections.Generic;

namespace Crpg.Module.Balancing
{
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

            return (float)Math.Pow(pSum / (double)numbers.Count, 1d / (double)p);
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

            return (float)Math.Pow(pSum, 1d / (double)p);
        }

        public static bool Within(float value, float bound1 , float bound2)
        {
            if (bound1 < bound2)
            {
                return (value >= bound1) && (value <= bound2);
            }
            else if (bound2 < bound1)
            {
                return (value >= bound2) && (value <= bound1);
            }
            else
            {
                return value == bound1;
            }
        }
    }
}
