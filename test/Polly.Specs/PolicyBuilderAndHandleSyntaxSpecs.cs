namespace Polly.Specs;

public static class PolicyBuilderAndHandleSyntaxSpecs
{
    [Fact]
    public static void Policy_builders_should_support_object_members()
    {
        PolicyBuilder builder = Policy.Handle<InvalidOperationException>();
        PolicyBuilder<ResultPrimitive> genericBuilder = Policy.HandleResult(ResultPrimitive.Fault);

        builder.ToString().ShouldNotBeNull();
        builder.Equals(null).ShouldBeFalse();
        builder.GetHashCode();
        builder.GetType().ShouldBe(typeof(PolicyBuilder));

        genericBuilder.ToString().ShouldNotBeNull();
        genericBuilder.Equals(null).ShouldBeFalse();
        genericBuilder.GetHashCode();
        genericBuilder.GetType().ShouldBe(typeof(PolicyBuilder<ResultPrimitive>));
    }

    [Fact]
    public static void Generic_handle_overloads_should_match_direct_and_inner_exceptions()
    {
        var handled = Policy<ResultPrimitive>
            .Handle<InvalidOperationException>(exception => exception.Message == "handled")
            .Retry();

        Should.Throw<InvalidOperationException>(() => handled.Execute(() => throw new InvalidOperationException("handled")));

        var handledInner = Policy<ResultPrimitive>
            .HandleInner<InvalidOperationException>(exception => exception.Message == "inner")
            .Retry();

        Should.Throw<InvalidOperationException>(() => handledInner.Execute(() => throw new AggregateException(new InvalidOperationException("inner"))))
            .Message.ShouldBe("inner");
    }
}
