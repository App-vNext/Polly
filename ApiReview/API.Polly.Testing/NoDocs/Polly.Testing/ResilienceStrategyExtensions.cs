// Assembly 'Polly.Testing'

using System;
using System.Collections.Generic;

namespace Polly.Testing;

public static class ResilienceStrategyExtensions
{
    public static InnerStrategiesDescriptor GetInnerStrategies<TResult>(this ResilienceStrategy<TResult> strategy);
    public static InnerStrategiesDescriptor GetInnerStrategies(this ResilienceStrategy strategy);
}
