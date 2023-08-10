using System.Diagnostics.CodeAnalysis;
using Polly.Registry;

namespace Polly.Core.Tests.Registry;

public class ResilienceStrategyProviderTests
{
    [Fact]
    public void Get_DoesNotExist_Throws()
    {
        new Provider()
            .Invoking(o => o.GetStrategy("not-exists"))
            .Should()
            .Throw<KeyNotFoundException>()
            .WithMessage("Unable to find a resilience strategy associated with the key 'not-exists'. Please ensure that either the resilience strategy or the builder is registered.");
    }

    [Fact]
    public void Get_GenericDoesNotExist_Throws()
    {
        new Provider()
            .Invoking(o => o.GetStrategy<string>("not-exists"))
            .Should()
            .Throw<KeyNotFoundException>()
            .WithMessage("Unable to find a generic resilience strategy of 'String' associated with the key 'not-exists'. " +
            "Please ensure that either the generic resilience strategy or the generic builder is registered.");
    }

    [Fact]
    public void Get_Exist_Ok()
    {
        var provider = new Provider { Strategy = new TestResilienceStrategy().AsStrategy() };

        provider.GetStrategy("exists").Should().Be(provider.Strategy);
    }

    [Fact]
    public void Get_GenericExist_Ok()
    {
        var provider = new Provider { GenericStrategy = new TestResilienceStrategy<string>() };

        provider.GetStrategy<string>("exists").Should().Be(provider.GenericStrategy);
    }

    private class Provider : ResilienceStrategyProvider<string>
    {
        public ResilienceStrategy? Strategy { get; set; }

        public object? GenericStrategy { get; set; }

        public override bool TryGetStrategy(string key, [NotNullWhen(true)] out ResilienceStrategy? strategy)
        {
            strategy = Strategy;
            return Strategy != null;
        }

        public override bool TryGetStrategy<TResult>(string key, [NotNullWhen(true)] out ResilienceStrategy<TResult>? strategy)
        {
            strategy = (ResilienceStrategy<TResult>?)GenericStrategy;
            return GenericStrategy != null;
        }
    }
}
