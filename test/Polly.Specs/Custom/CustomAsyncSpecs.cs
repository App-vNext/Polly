namespace Polly.Specs.Custom;

public class CustomAsyncSpecs
{
    [Fact]
    public void Should_be_able_to_construct_active_policy()
    {
        Action construct = () =>
        {
            AsyncPreExecutePolicy policy = AsyncPreExecutePolicy.CreateAsync(async () =>
            {
                Console.WriteLine("Do something");
                await Task.CompletedTask;
            });
        };

        Should.NotThrow(construct);
    }

    [Fact]
    public async Task Active_policy_should_execute()
    {
        bool preExecuted = false;
        AsyncPreExecutePolicy policy = AsyncPreExecutePolicy.CreateAsync(() => { preExecuted = true; return Task.CompletedTask; });

        bool executed = false;

        await Should.NotThrowAsync(() => policy.ExecuteAsync(() => { executed = true; return Task.CompletedTask; }));

        executed.ShouldBeTrue();
        preExecuted.ShouldBeTrue();
    }

    [Fact]
    public void Should_be_able_to_construct_reactive_policy()
    {
        Action construct = () =>
        {
            AsyncAddBehaviourIfHandlePolicy policy = Policy.Handle<Exception>().WithBehaviourAsync(async ex =>
            {
                Console.WriteLine("Handling " + ex.Message);
                await Task.CompletedTask;
            });
        };

        Should.NotThrow(construct);
    }

    [Fact]
    public async Task Reactive_policy_should_handle_exception()
    {
        Exception? handled = null;
        AsyncAddBehaviourIfHandlePolicy policy = Policy.Handle<InvalidOperationException>().WithBehaviourAsync(async ex => { handled = ex; await Task.CompletedTask; });

        Exception toThrow = new InvalidOperationException();
        bool executed = false;

        var ex = await Should.ThrowAsync<Exception>(() => policy.ExecuteAsync(() =>
        {
            executed = true;
            throw toThrow;
        }));
        ex.ShouldBe(toThrow);

        executed.ShouldBeTrue();
        handled.ShouldBe(toThrow);
    }

    [Fact]
    public async Task Reactive_policy_should_be_able_to_ignore_unhandled_exception()
    {
        Exception? handled = null;
        AsyncAddBehaviourIfHandlePolicy policy = Policy.Handle<InvalidOperationException>().WithBehaviourAsync(async ex => { handled = ex; await Task.CompletedTask; });

        Exception toThrow = new NotImplementedException();
        bool executed = false;

        var ex = await Should.ThrowAsync<Exception>(() => policy.ExecuteAsync(() =>
        {
            executed = true;
            throw toThrow;
        }));
        ex.ShouldBe(toThrow);

        executed.ShouldBeTrue();
        handled.ShouldBe(null);
    }
}
