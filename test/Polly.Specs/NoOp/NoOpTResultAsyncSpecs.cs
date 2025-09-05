namespace Polly.Specs.NoOp;

public class NoOpTResultAsyncSpecs
{
    [Fact]
    public void Should_throw_when_action_is_null()
    {
        var flags = BindingFlags.NonPublic | BindingFlags.Instance;
        Func<Context, CancellationToken, Task<EmptyStruct>> action = null!;

        var instance = Activator.CreateInstance(typeof(AsyncNoOpPolicy<EmptyStruct>), true)!;
        var instanceType = instance.GetType();
        var methods = instanceType.GetMethods(flags);
        var methodInfo = methods.First(method => method is { Name: "ImplementationAsync", ReturnType.Name: "Task`1" });

        var func = () => methodInfo.Invoke(instance, [action, new Context(), TestCancellation.Token, false]);

        var exceptionAssertions = Should.Throw<TargetInvocationException>(func);
        exceptionAssertions.Message.ShouldBe("Exception has been thrown by the target of an invocation.");
        exceptionAssertions.InnerException.ShouldBeOfType<ArgumentNullException>()
            .ParamName.ShouldBe("action");
    }

    [Fact]
    public async Task Should_execute_user_delegate()
    {
        var policy = Policy.NoOpAsync<int?>();
        int? result = null;

        await Should.NotThrowAsync(async () => result = await policy.ExecuteAsync(() => Task.FromResult((int?)10)));

        result.HasValue.ShouldBeTrue();
        result.ShouldBe(10);
    }

    [Fact]
    public async Task Should_execute_user_delegate_without_adding_extra_cancellation_behaviour()
    {
        var policy = Policy.NoOpAsync<int?>();
        int? result = null;

        using (var cts = new CancellationTokenSource())
        {
            cts.Cancel();

            await Should.NotThrowAsync(async () => result = await policy.ExecuteAsync(_ => Task.FromResult((int?)10), cts.Token));
        }

        result.HasValue.ShouldBeTrue();
        result.ShouldBe(10);
    }
}
