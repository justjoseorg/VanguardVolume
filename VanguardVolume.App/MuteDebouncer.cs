using System.Diagnostics;

namespace VanguardVolume.App;

public sealed class MuteDebouncer
{
    private readonly object _lock = new();
    private readonly long _minimumTickInterval;
    private readonly Func<long> _getTimestamp;
    private long? _lastAcceptedTimestamp;

    public MuteDebouncer(TimeSpan minimumInterval, Func<long>? getTimestamp = null)
    {
        _minimumTickInterval = (long)(minimumInterval.TotalSeconds * Stopwatch.Frequency);
        _getTimestamp = getTimestamp ?? Stopwatch.GetTimestamp;
    }

    public bool TryAccept()
    {
        var timestamp = _getTimestamp();
        lock (_lock)
        {
            if (_lastAcceptedTimestamp is { } lastAccepted
                && timestamp - lastAccepted < _minimumTickInterval)
            {
                return false;
            }

            _lastAcceptedTimestamp = timestamp;
            return true;
        }
    }
}
