namespace Polly.Specs.Helpers.Bulkhead;

public class AssertionFailure
{
    public AssertionFailure(int expected, int actual, string measure)
    {
        if (string.IsNullOrWhiteSpace(measure))
        {
            throw new ArgumentNullException(nameof(measure));
        }

        Expected = expected;
        Actual = actual;
        Measure = measure;
    }

    public int Expected { get; }
    public int Actual { get; }
    public string Measure { get; }
}
