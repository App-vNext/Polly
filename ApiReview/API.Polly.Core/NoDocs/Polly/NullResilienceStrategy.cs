// Assembly 'Polly.Core'

using System;
using System.Threading.Tasks;

namespace Polly;

public sealed class NullResilienceStrategy : ResilienceStrategy
{
    public static readonly NullResilienceStrategy Instance;
}
