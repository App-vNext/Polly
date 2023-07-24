// Assembly 'Polly.Core'

using System.Runtime.CompilerServices;
using System.Threading;
using Polly.Utils;

namespace Polly;

public abstract class ResilienceContextPool
{
    public static ResilienceContextPool Shared { get; }
    public ResilienceContext Get(CancellationToken cancellationToken = default(CancellationToken));
    public abstract ResilienceContext Get(string? operationKey, CancellationToken cancellationToken = default(CancellationToken));
    public abstract void Return(ResilienceContext context);
    protected ResilienceContextPool();
}
