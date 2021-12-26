namespace Crpg.Sdk.Abstractions.Metrics;

public interface ICount : IMetric
{
    public void Increment(long delta = 1);
    public void Decrement(long delta = 1);
}
