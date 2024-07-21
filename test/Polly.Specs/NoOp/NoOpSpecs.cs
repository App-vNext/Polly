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
        var func = () => generic.Invoke(instance, [action, new Context(), CancellationToken.None]);

        var exceptionAssertions = func.Should().Throw<TargetInvocationException>();
        exceptionAssertions.And.Message.Should().Be("Exception has been thrown by the target of an invocation.");
        exceptionAssertions.WithInnerException<ArgumentNullException>("action");

        instance = Activator.CreateInstance(typeof(NoOpPolicy<EmptyStruct>), true)!;
        instanceType = instance.GetType();
        methods = instanceType.GetMethods(flags);
        methodInfo = methods.First(method => method is { Name: "Implementation", ReturnType.Name: "EmptyStruct" });
        func = () => methodInfo.Invoke(instance, [action, new Context(), CancellationToken.None]);

        exceptionAssertions = func.Should().Throw<TargetInvocationException>();
        exceptionAssertions.And.Message.Should().Be("Exception has been thrown by the target of an invocation.");
        exceptionAssertions.WithInnerException<ArgumentNullException>("action");
    }

    [Fact]
    public void Should_execute_user_delegate()
    {
        NoOpPolicy policy = Policy.NoOp();
        bool executed = false;

        policy.Invoking(x => x.Execute(() => { executed = true; }))
            .Should().NotThrow();

        executed.Should().BeTrue();
    }

    [Fact]
    public void Should_execute_user_delegate_without_adding_extra_cancellation_behaviour()
    {
        NoOpPolicy policy = Policy.NoOp();
        bool executed = false;

        using (var cts = new CancellationTokenSource())
        {
            cts.Cancel();

            policy.Invoking(p => p.Execute(_ => { executed = true; }, cts.Token))
                .Should().NotThrow();
        }

        executed.Should().BeTrue();
    }
}
