using System;
using Microsoft.Extensions.Options;

namespace Polly.Extensions.Utils;

internal class OptionsReloadHelper<T> : IDisposable
{
    private readonly string? _name;
    private readonly IDisposable? _listener;
    private CancellationTokenSource _cancellation = new();

    public OptionsReloadHelper(IOptionsMonitor<T> monitor, string? name)
    {
        _name = name;

        if (!string.IsNullOrEmpty(name))
        {
            _listener = monitor.OnChange((_, name) =>
            {
                if (name == _name)
                {
                    HandleChange();
                }
            });
        }
        else
        {
            _listener = monitor.OnChange(_ => HandleChange());
        }
    }

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
