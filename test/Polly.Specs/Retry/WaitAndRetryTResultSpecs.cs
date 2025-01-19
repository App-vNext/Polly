namespace Polly.Specs.Retry;

[Collection(Constants.SystemClockDependentTestCollection)]
public class WaitAndRetryTResultSpecs : IDisposable
{
    public WaitAndRetryTResultSpecs() => SystemClock.Sleep = (_, _) => { };

    [Fact]
    public void Should_be_able_to_calculate_retry_timespans_based_on_the_handled_fault()
    {
        Dictionary<ResultPrimitive, TimeSpan> expectedRetryWaits = new Dictionary<ResultPrimitive, TimeSpan>
        {
            {ResultPrimitive.Fault, TimeSpan.FromSeconds(2)},
            {ResultPrimitive.FaultAgain, TimeSpan.FromSeconds(4)},
        };

        var actualRetryWaits = new List<TimeSpan>();

        var policy = Policy
            .HandleResult(ResultPrimitive.Fault)
            .OrResult(ResultPrimitive.FaultAgain)
            .WaitAndRetry(2,
                (_, outcome, _) => expectedRetryWaits[outcome.Result],
                (_, timeSpan, _, _) => actualRetryWaits.Add(timeSpan));

        using (var enumerator = expectedRetryWaits.GetEnumerator())
        {
            policy.Execute(() => enumerator.MoveNext()
                ? enumerator.Current.Key
                : ResultPrimitive.Undefined);
        }

        actualRetryWaits.ShouldBeSubsetOf(expectedRetryWaits.Values);
    }

    public void Dispose() =>
        SystemClock.Reset();
}
