using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Polly.Fallback;

namespace Polly.Core.Tests.Fallback;
public class FallbackResilienceStrategyBuilderExtensionsTests
{
    private readonly ResilienceStrategyBuilder _builder = new();

    public static readonly TheoryData<Action<ResilienceStrategyBuilder>> FallbackCases = new()
    {
        builder =>
        {
            builder.AddFallback(new FallbackStrategyOptions());
        },
        builder =>
        {
            builder.AddFallback(new FallbackStrategyOptions<double>{ FallbackAction = (_, _) =>  new ValueTask<double>(0) });
        },
        builder =>
        {
            builder.AddFallback<double>(handle => { }, (_, _) =>  new ValueTask<double>(0));
        },
    };

    [MemberData(nameof(FallbackCases))]
    [Theory]
    public void AddFallback_Ok(Action<ResilienceStrategyBuilder> configure)
    {
        configure(_builder);
        _builder.Build().Should().BeOfType<FallbackResilienceStrategy>();
    }

    [Fact]
    public void AddFallback_Generic_Ok()
    {
        var strategy = _builder
            .AddFallback<int>(
                handler => handler.HandleResult(-1).HandleException<InvalidOperationException>(),
                (_, args) =>
                {
                    args.Context.Should().NotBeNull();
                    return new ValueTask<int>(1);
                })
            .Build();

        strategy.Execute(_ => -1).Should().Be(1);
        strategy.Execute<int>(_ => throw new InvalidOperationException()).Should().Be(1);
    }

    [Fact]
    public void AddFallback_InvalidOptions_Throws()
    {
        _builder
            .Invoking(b => b.AddFallback(new FallbackStrategyOptions { Handler = null! }))
            .Should()
            .Throw<ValidationException>()
            .WithMessage("The fallback strategy options are invalid.*");
    }

    [Fact]
    public void AddFallbackT_InvalidOptions_Throws()
    {
        _builder
            .Invoking(b => b.AddFallback(new FallbackStrategyOptions<double> { ShouldHandle = null! }))
            .Should()
            .Throw<ValidationException>()
            .WithMessage("The fallback strategy options are invalid.*");
    }
}
