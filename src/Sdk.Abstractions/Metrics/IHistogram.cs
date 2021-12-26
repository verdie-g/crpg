namespace Crpg.Sdk.Abstractions.Metrics;

public interface IHistogram : IMetric
{
    public void Record(double value);
}
