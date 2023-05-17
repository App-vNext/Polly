namespace Polly.Specs.Helpers.Bulkhead;

public class AssertionFailure
{
    public AssertionFailure(int expected, int actual, string measure)
    {
        Expected = expected;
        Actual = actual;
        Measure = Guard.NotNullOrEmpty(measure);
    }

    public int Expected { get; }
    public int Actual { get; }
    public string Measure { get; }
}
