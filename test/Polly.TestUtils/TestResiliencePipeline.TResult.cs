namespace Polly.TestUtils;

public class TestResiliencePipeline<T> : ResiliencePipeline<T>
{
    public TestResiliencePipeline()
        : base(new TestResilienceStrategy().AsPipeline())
    {
    }
}
