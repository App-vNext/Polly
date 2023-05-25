namespace Polly.TestUtils;

public class TestResilienceStrategy<T> : ResilienceStrategy<T>
{
    public TestResilienceStrategy()
        : base(new TestResilienceStrategy())
    {
    }
}
