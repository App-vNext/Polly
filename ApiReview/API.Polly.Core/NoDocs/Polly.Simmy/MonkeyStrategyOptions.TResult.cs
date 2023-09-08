// Assembly 'Polly.Core'

using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Polly.Simmy;

public abstract class MonkeyStrategyOptions<TResult> : ResilienceStrategyOptions
{
    [Range(0.0, 1.0)]
    public double InjectionRate { get; set; }
    public Func<InjectionRateGeneratorArguments, ValueTask<double>>? InjectionRateGenerator { get; set; }
    public Func<EnabledGeneratorArguments, ValueTask<bool>>? EnabledGenerator { get; set; }
    public bool Enabled { get; set; }
    [Required]
    public Func<double> Randomizer { get; set; }
    protected MonkeyStrategyOptions();
}
