namespace Polly.Specs.CircuitBreaker;

public class ICircuitBreakerTResultPolicySpecs
{
    [Fact]
    public void Should_be_able_to_use_LastHandledResult_via_interface()
    {
        var breaker = Policy
            .HandleResult(ResultPrimitive.Fault)
            .CircuitBreaker(2, TimeSpan.FromMinutes(1));

        breaker.LastHandledResult.ShouldBe(default);
    }
}
