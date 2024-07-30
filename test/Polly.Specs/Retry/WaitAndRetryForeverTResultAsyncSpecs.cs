namespace Polly.Specs.Retry;

[Collection(Constants.SystemClockDependentTestCollection)]
public class WaitAndRetryForeverTResultAsyncSpecs : IDisposable
{
    public WaitAndRetryForeverTResultAsyncSpecs() => SystemClock.SleepAsync = (_, _) => TaskHelper.EmptyTask;

    [Fact]
    public async Task Should_be_able_to_calculate_retry_timespans_based_on_the_handled_fault()
    {
        Dictionary<ResultPrimitive, TimeSpan> expectedRetryWaits = new Dictionary<ResultPrimitive, TimeSpan>
        {
            {ResultPrimitive.Fault, 2.Seconds()},
            {ResultPrimitive.FaultAgain, 4.Seconds()},
        };

        var actualRetryWaits = new List<TimeSpan>();

        var policy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .OrResult(ResultPrimitive.FaultAgain)
            .WaitAndRetryForeverAsync(
                (_, outcome, _) => expectedRetryWaits[outcome.Result],
                (_, timeSpan, _) =>
                {
                    actualRetryWaits.Add(timeSpan);
                    return TaskHelper.EmptyTask;
                });

        using (var enumerator = expectedRetryWaits.GetEnumerator())
        {
            await policy.ExecuteAsync(async () =>
            {
                await TaskHelper.EmptyTask;
                return enumerator.MoveNext()
                    ? enumerator.Current.Key
                    : ResultPrimitive.Undefined;
            });
        }

        actualRetryWaits.Should().ContainInOrder(expectedRetryWaits.Values);
    }

    public void Dispose() =>
        SystemClock.Reset();
}
