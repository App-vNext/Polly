namespace Polly.Specs.NoOp;

public class NoOpAsyncSpecs
{
    [Fact]
    public void Should_throw_when_action_is_null()
    {
        var flags = BindingFlags.NonPublic | BindingFlags.Instance;
        Func<Context, CancellationToken, Task<EmptyStruct>> action = null!;

        var instance = Activator.CreateInstance(typeof(AsyncNoOpPolicy), true)!;
        var instanceType = instance.GetType();
        var methods = instanceType.GetMethods(flags);
        var methodInfo = methods.First(method => method is { Name: "ImplementationAsync", ReturnType.Name: "Task`1" });
        var generic = methodInfo.MakeGenericMethod(typeof(EmptyStruct));

        var func = () => generic.Invoke(instance, [action, new Context(), CancellationToken.None, false]);

        var exceptionAssertions = func.Should().Throw<TargetInvocationException>();
        exceptionAssertions.And.Message.Should().Be("Exception has been thrown by the target of an invocation.");
        exceptionAssertions.WithInnerException<ArgumentNullException>("action");
    }

    [Fact]
    public async Task Should_execute_user_delegate()
    {
        var policy = Policy.NoOpAsync();
        bool executed = false;

        await policy.Awaiting(p => p.ExecuteAsync(() => { executed = true; return TaskHelper.EmptyTask; }))
            .Should().NotThrowAsync();

        executed.Should().BeTrue();
    }

    [Fact]
    public async Task Should_execute_user_delegate_without_adding_extra_cancellation_behaviour()
    {
        var policy = Policy.NoOpAsync();

        bool executed = false;

        using (var cts = new CancellationTokenSource())
        {
            cts.Cancel();

            await policy.Awaiting(p => p.ExecuteAsync(
                _ => { executed = true; return TaskHelper.EmptyTask; }, cts.Token))
                .Should().NotThrowAsync();
        }

        executed.Should().BeTrue();
    }
}
