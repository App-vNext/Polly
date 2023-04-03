using Polly.Strategy;

namespace Polly.Core.Tests.Utils;

public class TestArguments : IResilienceArguments
{
    public TestArguments() => Context = ResilienceContext.Get();

    public ResilienceContext Context { get; }
}
