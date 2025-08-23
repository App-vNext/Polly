namespace Polly.Specs.NoOp;

public class NoOpSpecs
{
    [Fact]
    public void Should_throw_when_action_is_null()
    {
        var flags = BindingFlags.NonPublic | BindingFlags.Instance;
        Func<Context, CancellationToken, EmptyStruct> action = null!;

        var instance = Activator.CreateInstance(typeof(NoOpPolicy), true)!;
        var instanceType = instance.GetType();
        var methods = instanceType.GetMethods(flags);

        var methodInfo = methods.First(method => method is { Name: "Implementation", ReturnType.Name: "TResult" });
        var generic = methodInfo.MakeGenericMethod(typeof(EmptyStruct));
        var func = () => generic.Invoke(instance, [action, new Context(), TestCancellation.Token]);

        var exceptionAssertions = Should.Throw<TargetInvocationException>(func);
        exceptionAssertions.Message.ShouldBe("Exception has been thrown by the target of an invocation.");
        exceptionAssertions.InnerException.ShouldBeOfType<ArgumentNullException>()
            .ParamName.ShouldBe("action");
    }

    [Fact]
    public void Should_execute_user_delegate()
    {
        NoOpPolicy policy = Policy.NoOp();
        bool executed = false;

        Should.NotThrow(() => policy.Execute(() => executed = true));

        executed.ShouldBeTrue();
    }

    [Fact]
    public void Should_execute_user_delegate_without_adding_extra_cancellation_behaviour()
    {
        var policy = Policy.NoOp();
        bool executed = false;

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        Should.NotThrow(
            () => policy.Execute((token) =>
            {
                token.ShouldBe(cts.Token);
                executed = true;
            }, cts.Token));

        executed.ShouldBeTrue();
    }

    [Fact]
    public void Should_execute_user_delegate_with_specified_context_and_token()
    {
        var policy = Policy.NoOp();

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var context = new Context();

        bool executed = false;

        policy.Execute((ctx, token) =>
        {
            ctx.ShouldBeSameAs(context);
            token.ShouldBe(cts.Token);

            executed = true;
        }, context, cts.Token);

        executed.ShouldBeTrue();
    }
}
