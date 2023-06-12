using Microsoft.Extensions.Options;

namespace Polly.Extensions.Utils;

internal sealed class OptionsReloadHelper<T> : IDisposable
{
    private readonly IDisposable? _listener;
    private CancellationTokenSource _cancellation = new();

    public OptionsReloadHelper(IOptionsMonitor<T> monitor, string name) => _listener = monitor.OnChange((_, changedNamed) =>
    {
        if (name == changedNamed)
        {
            HandleChange();
        }
    });

    public CancellationToken GetCancellationToken() => _cancellation.Token;

    public void Dispose()
    {
        _cancellation.Dispose();
        _listener?.Dispose();
    }

    private void HandleChange()
    {
        var oldCancellation = _cancellation;
        _cancellation = new CancellationTokenSource();
        oldCancellation.Cancel();
        oldCancellation.Dispose();
    }
}
