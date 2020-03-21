namespace Crpg.Application.Common.Interfaces.Metrics
{
    public interface IHistogram : IMetric
    {
        public void Record(double value);
    }
}