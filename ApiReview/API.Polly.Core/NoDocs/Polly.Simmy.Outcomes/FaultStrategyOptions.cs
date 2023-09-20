// Assembly 'Polly.Core'

using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Polly.Simmy.Outcomes;

public class FaultStrategyOptions : MonkeyStrategyOptions
{
    public Func<OnFaultInjectedArguments, ValueTask>? OnFaultInjected { get; set; }
    public Func<FaultGeneratorArguments, ValueTask<Exception?>>? FaultGenerator { get; set; }
    public Exception? Fault { get; set; }
    public FaultStrategyOptions();
}
