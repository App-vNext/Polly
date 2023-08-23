// Assembly 'Polly.Core'

using System.Runtime.CompilerServices;
using System.Threading;

namespace Polly;

public readonly struct ResilienceContextCreationArguments
{
    public string? OperationKey { get; }
    public bool? ContinueOnCapturedContext { get; }
    public CancellationToken CancellationToken { get; }
    public ResilienceContextCreationArguments(string? operationKey, bool? continueOnCapturedContext, CancellationToken cancellationToken);
}
