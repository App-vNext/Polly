// See https://aka.ms/new-console-template for more information

using Polly;
using Resilience;

public sealed class EmptyResilienceStrategy : DelegatingResilienceStrategy
{
}
