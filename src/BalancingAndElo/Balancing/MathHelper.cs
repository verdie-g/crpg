using System;
using System.Collections.Generic;

namespace Crpg.BalancingAndRating.Balancing
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
        public static int PowerMean(List<int> numbers, int p)
        {
            double pSum = 0;
            foreach (int number in numbers)
            {
                pSum += Math.Pow(number, p);
            }

            return (int)Math.Pow(pSum / numbers.Count, 1d / (double)p);
        }

        /// <summary>
        /// This is a math function https://en.wikipedia.org/wiki/Norm_(mathematics)#p-norm
        /// </summary>
        public static int PowerSum(List<int> numbers, int p)
        {
            double pSum = 0;
            foreach (int number in numbers)
            {
                pSum += Math.Pow(number, p);
            }

            return (int)Math.Pow(pSum, 1d / (double)p);
        }

        /// <summary>
        /// This is a math function https://www.moserware.com/assets/computing-your-skill/The%20Math%20Behind%20TrueSkill.pdf
        /// </summary>
        public static double N(double x, double mu,double sigma)
        {
            return (1 / (Math.Sqrt(2 * Math.PI) * sigma)) * Math.Exp(-1 / (2 * sigma * sigma) * (x - mu) * (x - mu));
        }

        /// <summary>
        /// This is a math function https://www.moserware.com/assets/computing-your-skill/The%20Math%20Behind%20TrueSkill.pdf
        /// </summary>
        public static double Phi(double t)
        {
            return 1 / 2 + t / Math.Sqrt(2 * Math.PI);
        }
    }
}
