using System;

namespace Microsoft.ApplicationInsights.Kubernetes.Utilities;

/// <summary>
/// A very simple implementation for emitting delay exponentially.
/// </summary>
internal class ExponentialDelaySource
{
    private TimeSpan _initial;
    private int _emitCount = 1;
    private readonly TimeSpan _maximum;

    public ExponentialDelaySource(TimeSpan initial, TimeSpan maximum)
    {
        _initial = initial;
        _maximum = maximum;
    }

    public TimeSpan GetNext()
    {
        TimeSpan nextTimeSpan = TimeSpan.FromSeconds(Math.Pow(_initial.TotalSeconds, _emitCount));

        if (nextTimeSpan > _maximum)
        {
            // Stopped at the maximum.
            return _maximum;
        }
        _emitCount++;
        return nextTimeSpan;
    }
}
