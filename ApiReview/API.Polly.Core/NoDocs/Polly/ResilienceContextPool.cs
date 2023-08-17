// Assembly 'Polly.Core'

using System.Runtime.CompilerServices;
using System.Threading;
using Polly.Utils;

namespace Polly;

public abstract class ResilienceContextPool
{
    public static ResilienceContextPool Shared { get; }
    public ResilienceContext Get(CancellationToken cancellationToken = default(CancellationToken));
    public ResilienceContext Get(string? operationKey, CancellationToken cancellationToken = default(CancellationToken));
    public ResilienceContext Get(string? operationKey, bool? continueOnCapturedContext, CancellationToken cancellationToken = default(CancellationToken));
    public ResilienceContext Get(bool continueOnCapturedContext, CancellationToken cancellationToken = default(CancellationToken));
    public abstract ResilienceContext Get(ResilienceContextCreationArguments arguments);
    public abstract void Return(ResilienceContext context);
    protected ResilienceContextPool();
}
