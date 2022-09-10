namespace Crpg.Sdk.Abstractions;

/// <summary>
/// Abstracts the system clock to facilitate testing.
/// </summary>
public interface IDateTime
{
    /// <summary>
    /// Gets a <see cref="DateTime" /> object that is set to the current date and time on this computer, expressed as
    /// the Coordinated Universal Time (UTC).
    /// </summary>
    DateTime UtcNow { get; }
}
