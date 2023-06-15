// Assembly 'Polly.Core'

using System;
using System.Runtime.CompilerServices;

namespace Polly;

public readonly struct ResiliencePropertyKey<TValue> : IEquatable<ResiliencePropertyKey<TValue>>
{
    public string Key { get; }
    public ResiliencePropertyKey(string key);
    public override string ToString();
    public override bool Equals(object? obj);
    public bool Equals(ResiliencePropertyKey<TValue> other);
    public override int GetHashCode();
    public static bool operator ==(ResiliencePropertyKey<TValue> left, ResiliencePropertyKey<TValue> right);
    public static bool operator !=(ResiliencePropertyKey<TValue> left, ResiliencePropertyKey<TValue> right);
}
