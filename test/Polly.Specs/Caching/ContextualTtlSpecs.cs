namespace Polly.Specs.Caching;

public class ContextualTtlSpecs
{
    [Fact]
    public void Should_throw_when_context_is_null()
    {
        Context context = null!;
        Action action = () => new ContextualTtl().GetTtl(context, null);
        Should.Throw<ArgumentNullException>(action).ParamName.ShouldBe("context");
    }

    [Fact]
    public void Should_return_zero_if_no_value_set_on_context() =>
        new ContextualTtl().GetTtl(new Context("someOperationKey"), null).Timespan.ShouldBe(TimeSpan.Zero);

    [Fact]
    public void Should_return_zero_if_invalid_value_set_on_context()
    {
        Dictionary<string, object> contextData = new Dictionary<string, object>
        {
            [ContextualTtl.TimeSpanKey] = new object()
        };

        Context context = new Context(string.Empty, contextData);
        new ContextualTtl().GetTtl(context, null).Timespan.ShouldBe(TimeSpan.Zero);
    }

    [Fact]
    public void Should_return_value_set_on_context()
    {
        TimeSpan ttl = TimeSpan.FromSeconds(30);
        Dictionary<string, object> contextData = new Dictionary<string, object>
        {
            [ContextualTtl.TimeSpanKey] = ttl
        };

        Context context = new Context(string.Empty, contextData);
        Ttl gotTtl = new ContextualTtl().GetTtl(context, null);
        gotTtl.Timespan.ShouldBe(ttl);
        gotTtl.SlidingExpiration.ShouldBeFalse();
    }

    [Fact]
    public void Should_return_negative_value_set_on_context()
    {
        TimeSpan ttl = TimeSpan.FromTicks(-1);
        Dictionary<string, object> contextData = new Dictionary<string, object>
        {
            [ContextualTtl.TimeSpanKey] = ttl
        };

        Context context = new Context(string.Empty, contextData);
        Ttl gotTtl = new ContextualTtl().GetTtl(context, null);
        gotTtl.Timespan.ShouldBe(ttl);
        gotTtl.SlidingExpiration.ShouldBeFalse();
    }

    [Fact]
    public void Should_return_zero_if_non_timespan_value_set_on_context()
    {
        var contextData = new Dictionary<string, object>
        {
            [ContextualTtl.TimeSpanKey] = "non-timespan value"
        };

        Context context = new Context(string.Empty, contextData);
        new ContextualTtl().GetTtl(context, null).Timespan.ShouldBe(TimeSpan.Zero);
    }

    [Fact]
    public void Should_return_sliding_expiration_if_set_on_context()
    {
        var ttl = TimeSpan.FromSeconds(30);
        var contextData = new Dictionary<string, object>
        {
            [ContextualTtl.TimeSpanKey] = ttl,
            [ContextualTtl.SlidingExpirationKey] = true
        };

        var context = new Context(string.Empty, contextData);
        var gotTtl = new ContextualTtl().GetTtl(context, null);
        gotTtl.Timespan.ShouldBe(ttl);
        gotTtl.SlidingExpiration.ShouldBeTrue();
    }

    [Fact]
    public void Should_return_no_sliding_expiration_if_set_to_false_on_context()
    {
        var ttl = TimeSpan.FromSeconds(30);
        var contextData = new Dictionary<string, object>
        {
            [ContextualTtl.TimeSpanKey] = ttl,
            [ContextualTtl.SlidingExpirationKey] = false
        };

        var context = new Context(string.Empty, contextData);
        var gotTtl = new ContextualTtl().GetTtl(context, null);
        gotTtl.Timespan.ShouldBe(ttl);
        gotTtl.SlidingExpiration.ShouldBeFalse();
    }

    [Fact]
    public void Should_return_no_sliding_expiration_if_non_boolean_value_set_on_context()
    {
        var ttl = TimeSpan.FromSeconds(30);
        var contextData = new Dictionary<string, object>
        {
            [ContextualTtl.TimeSpanKey] = ttl,
            [ContextualTtl.SlidingExpirationKey] = "non-boolean value"
        };

        var context = new Context(string.Empty, contextData);
        var gotTtl = new ContextualTtl().GetTtl(context, null);
        gotTtl.Timespan.ShouldBe(ttl);
        gotTtl.SlidingExpiration.ShouldBeFalse();
    }

    [Fact]
    public void TimeSpanKey_is_correct()
    {
        // Assert
        ContextualTtl.TimeSpanKey.ShouldBe("ContextualTtlTimeSpan");
    }

    [Fact]
    public void SlidingExpirationKey_is_correct()
    {
        // Assert
        ContextualTtl.SlidingExpirationKey.ShouldBe("ContextualTtlSliding");
    }

    [Fact]
    public void ContextualTtl_behaves_correctly_for_no_ttl()
    {
        // Arrange
        var target = new ContextualTtl();

        // Act
        var actual = target.GetTtl([], null);

        // Assert
        actual.Timespan.ShouldBe(TimeSpan.Zero);
        actual.SlidingExpiration.ShouldBeFalse();
    }
}
