namespace Polly.Specs.Helpers.Bulkhead;

public class AssertionFailure
{
    public AssertionFailure(int expected, int actual, string measure)
    {
        Guard.NotNull(measure);

        Expected = expected;
        Actual = actual;
        Measure = measure;
    }

    public int Expected { get; }
    public int Actual { get; }
    public string Measure { get; }
}
