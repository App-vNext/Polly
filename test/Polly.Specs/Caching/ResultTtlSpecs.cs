namespace Polly.Specs.Caching;

public class ResultTtlSpecs
{
    [Fact]
    public void Should_throw_when_func_is_null()
    {
        Action configure = () => _ = new ResultTtl<object>((Func<object?, Ttl>)null!);

        Should.Throw<ArgumentNullException>(configure).ParamName.ShouldBe("ttlFunc");
    }

    [Fact]
    public void Should_throw_when_func_is_null_using_context()
    {
        Action configure = () => _ = new ResultTtl<object>((Func<Context, object?, Ttl>)null!);

        Should.Throw<ArgumentNullException>(configure).ParamName.ShouldBe("ttlFunc");
    }

    [Fact]
    public void Should_not_throw_when_func_is_set()
    {
        Action configure = () => _ = new ResultTtl<object>(_ => default);

        Should.NotThrow(configure);
    }

    [Fact]
    public void Should_not_throw_when_func_is_set_using_context()
    {
        Action configure = () => _ = new ResultTtl<object>((_, _) => default);

        Should.NotThrow(configure);
    }

    [Fact]
    public void Should_return_func_result()
    {
        TimeSpan ttl = TimeSpan.FromMinutes(1);
        Func<dynamic?, Ttl> func = result => new Ttl(result!.Ttl);

        ResultTtl<dynamic> ttlStrategy = new ResultTtl<dynamic>(func);

        Ttl retrieved = ttlStrategy.GetTtl(new Context("someOperationKey"), new { Ttl = ttl });
        retrieved.Timespan.ShouldBe(ttl);
        retrieved.SlidingExpiration.ShouldBeFalse();
    }

    [Fact]
    public void Should_return_func_result_using_context()
    {
        const string SpecialKey = "specialKey";

        TimeSpan ttl = TimeSpan.FromMinutes(1);
        Func<Context, dynamic?, Ttl> func = (context, result) => context.OperationKey == SpecialKey ? new Ttl(TimeSpan.Zero) : new Ttl(result!.Ttl);

        ResultTtl<dynamic> ttlStrategy = new ResultTtl<dynamic>(func);

        ttlStrategy.GetTtl(new Context("someOperationKey"), new { Ttl = ttl }).Timespan.ShouldBe(ttl);
        ttlStrategy.GetTtl(new Context(SpecialKey), new { Ttl = ttl }).Timespan.ShouldBe(TimeSpan.Zero);
    }
}
