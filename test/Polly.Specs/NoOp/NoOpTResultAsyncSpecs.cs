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

        var func = () => methodInfo.Invoke(instance, [action, new Context(), CancellationToken.None, false]);

        var exceptionAssertions = func.Should().Throw<TargetInvocationException>();
        exceptionAssertions.And.Message.Should().Be("Exception has been thrown by the target of an invocation.");
        exceptionAssertions.WithInnerException<ArgumentNullException>("action");
    }

    [Fact]
    public async Task Should_execute_user_delegate()
    {
        var policy = Policy.NoOpAsync<int?>();
        int? result = null;

        Func<AsyncNoOpPolicy<int?>, Task> action = async p => result = await p.ExecuteAsync(() => Task.FromResult((int?)10));
        await policy.Awaiting(action)
            .Should().NotThrowAsync();

        result.HasValue.Should().BeTrue();
        result.Should().Be(10);
    }

    [Fact]
    public async Task Should_execute_user_delegate_without_adding_extra_cancellation_behaviour()
    {
        var policy = Policy.NoOpAsync<int?>();
        int? result = null;

        using (var cts = new CancellationTokenSource())
        {
            cts.Cancel();

            Func<AsyncNoOpPolicy<int?>, Task> action = async p => result = await p.ExecuteAsync(_ => Task.FromResult((int?)10), cts.Token);
            await policy.Awaiting(action)
                .Should().NotThrowAsync();
        }

        result.HasValue.Should().BeTrue();
        result.Should().Be(10);
    }
}
