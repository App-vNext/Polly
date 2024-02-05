namespace Polly.Specs.NoOp;

public class NoOpAsyncSpecs
{
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
