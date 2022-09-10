namespace Crpg.Sdk.Abstractions;

/// <summary>
/// Represents a random number generator.
/// </summary>
public interface IRandom
{
    /// <summary>
    /// Returns a random integer.
    /// </summary>
    /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
    /// <param name="maxValue">
    /// The exclusive upper bound of the random number returned. <paramref name="maxValue"/> must be greater than
    /// or equal to <paramref name="minValue"/>.
    /// </param>
    /// <returns>
    /// A 32-bit signed integer greater than or equal to <paramref name="minValue"/> and less than <paramref name="maxValue"/>;
    /// that is, the range of return values includes <paramref name="minValue"/> but not maxValue. If <paramref name="minValue"/>
    /// equals <paramref name="maxValue"/>, minValue is returned.
    /// </returns>
    int Next(int minValue, int maxValue);

    /// <summary>
    /// Returns a random floating-point number that is greater than or equal to 0.0, and less than 1.0.
    /// </summary>
    /// <returns>A double-precision floating point number that is greater than or equal to 0.0, and less than 1.0.</returns>
    double NextDouble();
}
