// Assembly 'Polly.Core'

using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Polly.Simmy.Behavior;

public class BehaviorStrategyOptions : MonkeyStrategyOptions
{
    public Func<OnBehaviorInjectedArguments, ValueTask>? OnBehaviorInjected { get; set; }
    [Required]
    public Func<BehaviorActionArguments, ValueTask>? BehaviorAction { get; set; }
    public BehaviorStrategyOptions();
}
