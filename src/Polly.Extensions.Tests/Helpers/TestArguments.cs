using Polly.Strategy;

namespace Polly.Extensions.Tests.Helpers;

public record class TestArguments(ResilienceContext Context) : IResilienceArguments;
