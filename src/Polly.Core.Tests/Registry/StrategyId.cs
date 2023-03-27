using System;
using System.Collections.Generic;

namespace Polly.Core.Tests.Registry;

public record StrategyId(Type Type, string BuilderName, string InstanceName = "")
{
    public static readonly ResiliencePropertyKey<StrategyId> ResilienceKey = new("Polly.StrategyId");

    public static StrategyId Create<T>(string builderName, string instanceName = "")
        => new(typeof(T), builderName, instanceName);
    public static StrategyId Create(string builderName, string instanceName = "")
        => new(typeof(StrategyId), builderName, instanceName);

    public static readonly IEqualityComparer<StrategyId> Comparer = EqualityComparer<StrategyId>.Default;

    public static readonly IEqualityComparer<StrategyId> BuilderComparer = new BuilderResilienceKeyComparer();

    private sealed class BuilderResilienceKeyComparer : IEqualityComparer<StrategyId>
    {
        public bool Equals(StrategyId? x, StrategyId? y) => x?.Type == y?.Type && x?.BuilderName == y?.BuilderName;

        public int GetHashCode(StrategyId obj) => (obj.Type, obj.BuilderName).GetHashCode();
    }
}
