namespace Polly.Extensions.Tests;

[CollectionDefinition("NonParallelizableTests", DisableParallelization = true)]
public class NonParallelizableCollection : ICollectionFixture<NonParallelizableCollectionFixture>
{
}

