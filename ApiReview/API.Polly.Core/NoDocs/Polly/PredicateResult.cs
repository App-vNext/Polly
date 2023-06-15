// Assembly 'Polly.Core'

using System.Threading.Tasks;

namespace Polly;

public static class PredicateResult
{
    public static ValueTask<bool> True { get; }
    public static ValueTask<bool> False { get; }
}
