namespace Polly.CircuitBreaker
{
    internal class HealthCount
    {
        public int Successes { get; set; }
        public int FailuresFromClosedState { get; set; }
        public int FailuresFromHalfOpenState { get; set; }

        public int Total => Successes + FailuresFromClosedState;

        public long StartedAt { get; set; }
    }
}
