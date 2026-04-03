#if UNION_TYPES
#pragma warning disable CS1591
#pragma warning disable RS0016
#pragma warning disable SA1600
#pragma warning disable SA1649

namespace System.Runtime.CompilerServices;

internal interface IUnion
{
    object? Value { get; }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
internal sealed class UnionAttribute : Attribute;

#endif
