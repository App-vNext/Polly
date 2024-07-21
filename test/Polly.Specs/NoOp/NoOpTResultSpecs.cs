namespace Polly.Specs.NoOp;

public class NoOpTResultSpecs
{
    [Fact]
    public void Should_throw_when_action_is_null()
    {
        var flags = BindingFlags.NonPublic | BindingFlags.Instance;
        Func<Context, CancellationToken, EmptyStruct> action = null!;

        var instance = Activator.CreateInstance(typeof(NoOpPolicy<EmptyStruct>), true)!;
        var instanceType = instance.GetType();
        var methods = instanceType.GetMethods(flags);
        var methodInfo = methods.First(method => method is { Name: "Implementation", ReturnType.Name: "EmptyStruct" });
        var func = () => methodInfo.Invoke(instance, [action, new Context(), CancellationToken.None]);

        var exceptionAssertions = func.Should().Throw<TargetInvocationException>();
        exceptionAssertions.And.Message.Should().Be("Exception has been thrown by the target of an invocation.");
        exceptionAssertions.WithInnerException<ArgumentNullException>("action");
    }

    [Fact]
    public void Should_execute_user_delegate()
    {
        NoOpPolicy<int> policy = Policy.NoOp<int>();
        int? result = null;

        policy.Invoking(x => result = x.Execute(() => 10))
            .Should().NotThrow();

        result.HasValue.Should().BeTrue();
        result.Should().Be(10);
    }

    [Fact]
    public void Should_execute_user_delegate_without_adding_extra_cancellation_behaviour()
    {
        NoOpPolicy<int> policy = Policy.NoOp<int>();
        int? result = null;

        using (var cts = new CancellationTokenSource())
        {
            cts.Cancel();

            policy.Invoking(p => result = p.Execute(_ => 10, cts.Token))
               .Should().NotThrow();
        }

        result.HasValue.Should().BeTrue();
        result.Should().Be(10);
    }
}
