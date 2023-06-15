// Assembly 'Polly.Core'

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace Polly.Registry;

public class ResilienceStrategyRegistryOptions<TKey>
{
    [Required]
    public Func<ResilienceStrategyBuilder> BuilderFactory { get; set; }
    [Required]
    public IEqualityComparer<TKey> StrategyComparer { get; set; }
    [Required]
    public IEqualityComparer<TKey> BuilderComparer { get; set; }
    [Required]
    public Func<TKey, string> StrategyKeyFormatter { get; set; }
    [Required]
    public Func<TKey, string> BuilderNameFormatter { get; set; }
    public ResilienceStrategyRegistryOptions();
}
