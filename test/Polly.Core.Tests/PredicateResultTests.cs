namespace Polly.Core.Tests;

public class PredicateResultTests
{
    [Fact]
    public async Task True_Ok() =>
        (await PredicateResult.True()).ShouldBeTrue();

    [Fact]
    public async Task False_Ok() =>
        (await PredicateResult.False()).ShouldBeFalse();
}
