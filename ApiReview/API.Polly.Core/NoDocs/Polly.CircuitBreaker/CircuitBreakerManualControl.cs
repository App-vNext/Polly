// Assembly 'Polly.Core'

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Polly.CircuitBreaker;

public sealed class CircuitBreakerManualControl : IDisposable
{
    public Task IsolateAsync(CancellationToken cancellationToken = default(CancellationToken));
    public Task CloseAsync(CancellationToken cancellationToken = default(CancellationToken));
    public void Dispose();
    public CircuitBreakerManualControl();
}
