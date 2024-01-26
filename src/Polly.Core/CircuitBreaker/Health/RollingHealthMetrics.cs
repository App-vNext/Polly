namespace Polly.CircuitBreaker.Health;

/// <inheritdoc/>
internal sealed class RollingHealthMetrics : HealthMetrics
{
    private readonly TimeSpan _samplingDuration;
    private readonly TimeSpan _windowDuration;
    private readonly Queue<HealthWindow> _windows;

    private int _successes;
    private int _failures;

    private HealthWindow? _currentWindow;

    public RollingHealthMetrics(TimeSpan samplingDuration, short numberOfWindows, TimeProvider timeProvider)
        : base(timeProvider)
    {
        _samplingDuration = samplingDuration;
        _windowDuration = TimeSpan.FromTicks(_samplingDuration.Ticks / numberOfWindows);
        _windows = new Queue<HealthWindow>();
    }

    public override void IncrementSuccess()
    {
        _successes++;

        UpdateCurrentWindow().Successes++;
    }

    public override void IncrementFailure()
    {
        _failures++;

        UpdateCurrentWindow().Failures++;
    }

    public override void Reset()
    {
        _failures = 0;
        _successes = 0;
        _currentWindow = null;
        _windows.Clear();
    }

    public override HealthInfo GetHealthInfo()
    {
        UpdateCurrentWindow();

        return HealthInfo.Create(_successes, _failures);
    }

    private HealthWindow UpdateCurrentWindow()
    {
        var now = TimeProvider.GetUtcNow();
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
            var window = _windows.Dequeue();

            // Update global counters
            _successes -= window.Successes;
            _failures -= window.Failures;
        }

        return _currentWindow;
    }

    private sealed class HealthWindow
    {
        public int Successes { get; set; }

        public int Failures { get; set; }

        public DateTimeOffset StartedAt { get; set; }
    }
}
