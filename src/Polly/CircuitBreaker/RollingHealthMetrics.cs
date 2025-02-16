#nullable enable
namespace Polly.CircuitBreaker;

internal sealed class RollingHealthMetrics : IHealthMetrics
{
    internal const short WindowCount = 10;

    private readonly long _samplingDuration;
    private readonly long _windowDuration;
    private readonly Queue<HealthCount> _windows;

    private HealthCount? _currentWindow;

    public RollingHealthMetrics(TimeSpan samplingDuration)
    {
        _samplingDuration = samplingDuration.Ticks;

        _windowDuration = _samplingDuration / WindowCount;
        _windows = new(WindowCount + 1);
    }

    public void IncrementSuccess_NeedsLock()
    {
        var currentWindow = ActualiseCurrentMetric_NeedsLock();

        currentWindow.Successes++;
    }

    public void IncrementFailure_NeedsLock()
    {
        var currentWindow = ActualiseCurrentMetric_NeedsLock();

        currentWindow.Failures++;
    }

    public void Reset_NeedsLock()
    {
        _currentWindow = null;
        _windows.Clear();
    }

    public HealthCount GetHealthCount_NeedsLock()
    {
        ActualiseCurrentMetric_NeedsLock();

        int successes = 0;
        int failures = 0;
        foreach (var window in _windows)
        {
            successes += window.Successes;
            failures += window.Failures;
        }

        return new()
        {
            Successes = successes,
            Failures = failures,
            StartedAt = _windows.Peek().StartedAt
        };
    }

    private HealthCount ActualiseCurrentMetric_NeedsLock()
    {
        var now = SystemClock.UtcNow().Ticks;
        var currentWindow = _currentWindow;

        if (currentWindow == null || now - currentWindow.StartedAt >= _windowDuration)
        {
            _currentWindow = currentWindow = new()
            {
                StartedAt = now
            };
            _windows.Enqueue(currentWindow);
        }

        while (_windows.Count > 0 && now - _windows.Peek().StartedAt >= _samplingDuration)
        {
            _windows.Dequeue();
        }

        return currentWindow;
    }
}
