namespace Polly.Core.Tests;

public class PredicateResultTests
{
    [Fact]
    public async Task True_Ok() =>
        (await PredicateResult.True()).Should().BeTrue();

    [Fact]
    public async Task False_Ok() =>
        (await PredicateResult.False()).Should().BeFalse();
}
