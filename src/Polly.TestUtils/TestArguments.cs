using Polly.Strategy;

namespace Polly.TestUtils;

public class TestArguments : IResilienceArguments
{
    public TestArguments(ResilienceContext? context = null) => Context = context ?? ResilienceContext.Get();

    public ResilienceContext Context { get; }
}
