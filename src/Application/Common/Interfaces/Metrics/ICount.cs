namespace Crpg.Application.Common.Interfaces.Metrics
{
    public interface ICount : IMetric
    {
        public void Increment(long delta = 1);
        public void Decrement(long delta = 1);
    }
}