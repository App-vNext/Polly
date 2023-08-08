namespace Polly.Extensions.Tests;

[CollectionDefinition(nameof(NonParallelizableCollection), DisableParallelization = true)]
public class NonParallelizableCollection
{
}

