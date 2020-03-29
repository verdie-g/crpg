using System;
using System.Security.Cryptography;
using System.Threading;

namespace Crpg.Common
{
    public class ThreadSafeRandom
    {
        // https://devblogs.microsoft.com/pfxteam/getting-random-numbers-in-a-thread-safe-way/
        private static readonly RNGCryptoServiceProvider StrongRng = new RNGCryptoServiceProvider();

        public static readonly ThreadLocal<Random> Instance = new ThreadLocal<Random>(() =>
        {
            byte[] buffer = new byte[4];
            StrongRng.GetBytes(buffer);
            return new Random(BitConverter.ToInt32(buffer, 0));
        });
    }
}
