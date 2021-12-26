using Crpg.Sdk.Abstractions;

namespace Crpg.Sdk;

public class ThreadSafeRandom : IRandom
{
    public int Next(int minValue, int maxValue)
    {
        return Random.Shared.Next(minValue, maxValue);
    }

    public double NextDouble()
    {
        return Random.Shared.NextDouble();
    }
}
