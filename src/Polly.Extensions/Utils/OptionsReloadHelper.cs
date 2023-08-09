using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Options;

namespace Polly.Utils;

#pragma warning disable CA1001 // we can get away of not disposing this class because it's active for a lifetime of app
#pragma warning disable S2931

internal sealed class OptionsReloadHelper<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T>
{
    private CancellationTokenSource _cancellation = new();

    public OptionsReloadHelper(IOptionsMonitor<T> monitor, string name) => monitor.OnChange((_, changedNamed) =>
    {
        if (name == changedNamed)
        {
            HandleChange();
        }
    });

    public CancellationToken GetCancellationToken() => _cancellation.Token;

    private void HandleChange()
    {
        var oldCancellation = _cancellation;
        _cancellation = new CancellationTokenSource();
        oldCancellation.Cancel();
        oldCancellation.Dispose();
    }
}
