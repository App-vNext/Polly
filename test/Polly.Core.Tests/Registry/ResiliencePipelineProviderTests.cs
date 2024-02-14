using System.Diagnostics.CodeAnalysis;
using Polly.Registry;

namespace Polly.Core.Tests.Registry;

public class ResiliencePipelineProviderTests
{
    [Fact]
    public void Get_DoesNotExist_Throws() =>
        new Provider()
            .Invoking(o => o.GetPipeline("not-exists"))
            .Should()
            .Throw<KeyNotFoundException>()
            .WithMessage("Unable to find a resilience pipeline associated with the key 'not-exists'. Please ensure that either the resilience pipeline or the builder is registered.");

    [Fact]
    public void Get_GenericDoesNotExist_Throws() =>
        new Provider()
            .Invoking(o => o.GetPipeline<string>("not-exists"))
            .Should()
            .Throw<KeyNotFoundException>()
            .WithMessage("Unable to find a generic resilience pipeline of 'String' associated with the key 'not-exists'. " +
            "Please ensure that either the generic resilience pipeline or the generic builder is registered.");

    [Fact]
    public void Get_Exist_Ok()
    {
        var provider = new Provider { Strategy = new TestResilienceStrategy().AsPipeline() };

        provider.GetPipeline("exists").Should().Be(provider.Strategy);
    }

    [Fact]
    public void Get_GenericExist_Ok()
    {
        var provider = new Provider { GenericStrategy = ResiliencePipeline<string>.Empty };

        provider.GetPipeline<string>("exists").Should().Be(provider.GenericStrategy);
    }

    private class Provider : ResiliencePipelineProvider<string>
    {
        public ResiliencePipeline? Strategy { get; set; }

        public object? GenericStrategy { get; set; }

        public override bool TryGetPipeline(string key, [NotNullWhen(true)] out ResiliencePipeline? strategy)
        {
            strategy = Strategy;
            return Strategy != null;
        }

        public override bool TryGetPipeline<TResult>(string key, [NotNullWhen(true)] out ResiliencePipeline<TResult>? strategy)
        {
            strategy = (ResiliencePipeline<TResult>?)GenericStrategy;
            return GenericStrategy != null;
        }
    }
}
