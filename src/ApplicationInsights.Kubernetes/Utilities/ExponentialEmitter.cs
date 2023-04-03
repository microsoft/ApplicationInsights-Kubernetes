using System;

namespace Microsoft.ApplicationInsights.Kubernetes.Utilities;

/// <summary>
/// Responsible for emit delay.
/// </summary>
internal class ExponentialEmitter
{
    private TimeSpan _current;
    private readonly TimeSpan _maximum;

    public ExponentialEmitter(TimeSpan initial, TimeSpan maximum)
    {
        _current = initial;
        _maximum = maximum;
    }

    public TimeSpan GetNext()
    {
        TimeSpan nextTimeSpan = Min(_current * 2, _maximum);
        _current = nextTimeSpan;
        return _current;
    }

    private TimeSpan Min(TimeSpan item1, TimeSpan item2)
    {
        if (item1.TotalSeconds >= item2.TotalSeconds)
        {
            return item2;
        }
        return item1;
    }
}
