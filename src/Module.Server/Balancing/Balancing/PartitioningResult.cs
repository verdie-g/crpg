using System.Collections.Generic;

namespace Crpg.Module.Balancing
{
    /// <summary>
    /// Represents a partition of a given number set.
    /// </summary>
    public class PartitioningResult<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PartitioningResult"/>
        /// class with a given partition and partition sizes.
        /// </summary>
        /// <param name="partition">The partition as a list of arrays of integers. The
        /// integers represent the indices in a number set.</param>
        /// <param name="sizes">The sums of the values in the partition.</param>
        public PartitioningResult(List<T>[] partition, double[] sizes)
        {
            Partition = partition;
            Sizes = sizes;
        }

        /// <summary>
        /// The partition as a array of list of integers. The
        /// integers represent the indices in a number set.
        /// </summary>
        public List<T>[] Partition { get; }

        /// <summary>
        /// The sums of the values in the partition.
        /// </summary>
        public double[] Sizes { get; }

        public void Deconstruct(out List<T>[] partition, out double[] sizes)
        {
            partition = Partition;
            sizes = Sizes;
        }
    }
}
