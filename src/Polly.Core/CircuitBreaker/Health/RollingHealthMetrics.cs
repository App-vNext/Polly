namespace Polly.CircuitBreaker.Health;

internal sealed class RollingHealthMetrics : HealthMetrics
{
    private readonly TimeSpan _samplingDuration;
    private readonly TimeSpan _windowDuration;
    private readonly Queue<HealthWindow> _windows;

    private HealthWindow? _currentWindow;

    public RollingHealthMetrics(TimeSpan samplingDuration, short numberOfWindows, TimeProvider timeProvider)
        : base(timeProvider)
    {
        _samplingDuration = samplingDuration;
        _windowDuration = TimeSpan.FromTicks(_samplingDuration.Ticks / numberOfWindows);
        _windows = new Queue<HealthWindow>();
    }

    public override void IncrementSuccess() => UpdateCurrentWindow().Successes++;

    public override void IncrementFailure() => UpdateCurrentWindow().Failures++;

    public override void Reset()
    {
        _currentWindow = null;
        _windows.Clear();
    }

    public override HealthInfo GetHealthInfo()
    {
        UpdateCurrentWindow();

        var successes = 0;
        var failures = 0;
        foreach (var window in _windows)
        {
            successes += window.Successes;
            failures += window.Failures;
        }

        return HealthInfo.Create(successes, failures);
    }

    private HealthWindow UpdateCurrentWindow()
    {
        var now = TimeProvider.UtcNow;
        if (_currentWindow == null || now - _currentWindow.StartedAt >= _windowDuration)
        {
            _currentWindow = new()
            {
                StartedAt = now
            };
            _windows.Enqueue(_currentWindow);
        }

        while (now - _windows.Peek().StartedAt >= _samplingDuration)
        {
            _windows.Dequeue();
        }

        return _currentWindow;
    }

    private class HealthWindow
    {
        public int Successes { get; set; }

        public int Failures { get; set; }

        public DateTimeOffset StartedAt { get; set; }
    }
}
