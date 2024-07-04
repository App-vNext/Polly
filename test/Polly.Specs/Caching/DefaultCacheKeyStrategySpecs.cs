namespace Polly.Specs.Caching;

public class DefaultCacheKeyStrategySpecs
{
    [Fact]
    public void Should_return_Context_OperationKey_as_cache_key()
    {
        string operationKey = "SomeKey";

        Context context = new Context(operationKey);

        DefaultCacheKeyStrategy.Instance.GetCacheKey(context)
            .Should().Be(operationKey);
    }

    [Fact]
    public void Should_throw_when_context_is_null()
    {
        Context context = null!;
        Action action = () => DefaultCacheKeyStrategy.Instance.GetCacheKey(context);
        action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("context");
    }
}
