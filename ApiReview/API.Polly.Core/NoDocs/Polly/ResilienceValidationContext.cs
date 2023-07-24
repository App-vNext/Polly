// Assembly 'Polly.Core'

using System.Runtime.CompilerServices;

namespace Polly;

public sealed class ResilienceValidationContext
{
    public object Instance { get; }
    public string PrimaryMessage { get; }
    public ResilienceValidationContext(object instance, string primaryMessage);
}
