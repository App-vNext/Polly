namespace Polly.CircuitBreaker
{
    internal class HealthCount
    {
        public int Successes { get; set; }

        public int Failures { get; set; }

        public int Total { get { return Successes + Failures; } }

        public long StartedAt { get; set; }
    }
}
