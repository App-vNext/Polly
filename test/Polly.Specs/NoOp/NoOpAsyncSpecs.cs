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

        var func = () => generic.Invoke(instance, [action, new Context(), TestCancellation.Token, false]);

        var exceptionAssertions = Should.Throw<TargetInvocationException>(func);
        exceptionAssertions.Message.ShouldBe("Exception has been thrown by the target of an invocation.");
        exceptionAssertions.InnerException.ShouldBeOfType<ArgumentNullException>().ParamName.ShouldBe("action");
    }

    [Fact]
    public async Task Should_execute_user_delegate()
    {
        var policy = Policy.NoOpAsync();
        bool executed = false;

        await Should.NotThrowAsync(() => policy.ExecuteAsync(() => { executed = true; return TaskHelper.EmptyTask; }));

        executed.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_execute_user_delegate_without_adding_extra_cancellation_behaviour()
    {
        var policy = Policy.NoOpAsync();

        bool executed = false;

        using (var cts = new CancellationTokenSource())
        {
            cts.Cancel();

            await Should.NotThrowAsync(() => policy.ExecuteAsync(
                _ => { executed = true; return TaskHelper.EmptyTask; }, cts.Token));
        }

        executed.ShouldBeTrue();
    }
}
