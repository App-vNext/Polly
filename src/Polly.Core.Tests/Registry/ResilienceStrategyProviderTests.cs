using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Polly.Core.Tests.Utils;
using Polly.Registry;
using Xunit;

namespace Polly.Core.Tests.Registry;
public class ResilienceStrategyProviderTests
{
    [Fact]
    public void Get_DoesNotExist_Throws()
    {
        new Provider()
            .Invoking(o => o.Get("not-exists"))
            .Should()
            .Throw<KeyNotFoundException>()
            .WithMessage("Unable to find a resilience strategy associated with the key 'not-exists'. Please ensure the either resilience strategy or builder is registered.");
    }

    [Fact]
    public void Get_Exist_Ok()
    {
        var provider = new Provider { Strategy = new TestResilienceStrategy() };

        provider.Get("exists").Should().Be(provider.Strategy);
    }

    private class Provider : ResilienceStrategyProvider<string>
    {
        public ResilienceStrategy? Strategy { get; set; }

        public override bool TryGet(string key, [NotNullWhen(true)] out ResilienceStrategy? strategy)
        {
            strategy = Strategy;
            return Strategy != null;
        }
    }
}
