namespace Polly.Specs.Caching;

public class DefaultCacheKeyStrategySpecs
{
    [Fact]
    public void Should_throw_when_context_is_null()
    {
        Context context = null!;
        Action action = () => DefaultCacheKeyStrategy.Instance.GetCacheKey(context);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe("context");
    }

    [Fact]
    public void Should_return_Context_OperationKey_as_cache_key()
    {
        string operationKey = "SomeKey";

        Context context = [with(operationKey)];

        DefaultCacheKeyStrategy.Instance.GetCacheKey(context)
            .ShouldBe(operationKey);
    }
}
