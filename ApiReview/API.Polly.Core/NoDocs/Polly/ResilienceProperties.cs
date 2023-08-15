// Assembly 'Polly.Core'

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Polly;

public sealed class ResilienceProperties
{
    public bool TryGetValue<TValue>(ResiliencePropertyKey<TValue> key, [MaybeNullWhen(false)] out TValue value);
    public TValue GetValue<TValue>(ResiliencePropertyKey<TValue> key, TValue defaultValue);
    public void Set<TValue>(ResiliencePropertyKey<TValue> key, TValue value);
    public ResilienceProperties();
}