using static Polly.Specs.DictionaryHelpers;

namespace Polly.Specs.Retry;

[Collection(Constants.SystemClockDependentTestCollection)]
public class WaitAndRetryTResultAsyncSpecs : IDisposable
{
    public WaitAndRetryTResultAsyncSpecs() => SystemClock.SleepAsync = (_, _) => TaskHelper.EmptyTask;

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
            .WaitAndRetryAsync(2,
                (_, outcome, _) => expectedRetryWaits[outcome.Result],
                (_, timeSpan, _, _) =>
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

    [Fact]
    public void Should_throw_when_retry_count_is_less_than_zero()
    {
        var expectedRetryWaits = new Dictionary<ResultPrimitive, TimeSpan>
        {
            {ResultPrimitive.Fault, 2.Seconds()},
            {ResultPrimitive.FaultAgain, 4.Seconds()},
        };

        var actualRetryWaits = new List<TimeSpan>();

        Action configure = () => Policy
            .HandleResult(ResultPrimitive.Fault)
            .WaitAndRetryAsync(-1,
                (_, outcome, _) => expectedRetryWaits[outcome.Result],
                (_, timeSpan, _, _) =>
                {
                    actualRetryWaits.Add(timeSpan);
                    return TaskHelper.EmptyTask;
                });

        configure.Should().Throw<ArgumentOutOfRangeException>().And.ParamName.Should().Be("retryCount");
    }

    [Fact]
    public void Should_throw_when_onRetryAsync_is_null()
    {
        var expectedRetryWaits = new Dictionary<ResultPrimitive, TimeSpan>
        {
            {ResultPrimitive.Fault, 2.Seconds()},
            {ResultPrimitive.FaultAgain, 4.Seconds()},
        };

        Action configure = () => Policy
            .HandleResult(ResultPrimitive.Fault)
            .WaitAndRetryAsync(2,
                (_, outcome, _) => expectedRetryWaits[outcome.Result],
                null);

        configure.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("onRetryAsync");
    }

    public void Dispose() =>
        SystemClock.Reset();
}
