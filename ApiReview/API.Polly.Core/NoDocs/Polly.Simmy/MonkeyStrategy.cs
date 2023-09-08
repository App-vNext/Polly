// Assembly 'Polly.Core'

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Polly.Simmy;

public abstract class MonkeyStrategy : ResilienceStrategy
{
    protected MonkeyStrategy(MonkeyStrategyOptions options);
    protected ValueTask<bool> ShouldInjectAsync(ResilienceContext context);
}
