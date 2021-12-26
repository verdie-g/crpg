using System.Diagnostics;

namespace Crpg.Common;

/// <summary>
/// <see cref="Stopwatch"/> but as a struct.
/// </summary>
public readonly struct ValueStopwatch
{
    private static readonly double TimestampTicksToMachineTicksRatio = TimeSpan.TicksPerSecond / (double)Stopwatch.Frequency;

    public static ValueStopwatch StartNew() => new(Stopwatch.GetTimestamp());

    private readonly long _startTimestamp;

    public TimeSpan Elapsed => new((long)((Stopwatch.GetTimestamp() - _startTimestamp) * TimestampTicksToMachineTicksRatio));

    private ValueStopwatch(long startTimestamp) => _startTimestamp = startTimestamp;
}
