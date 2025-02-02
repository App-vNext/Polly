using Polly.Simmy.Fault;

namespace Polly.Core.Tests.Simmy.Fault;

public static class OnFaultInjectedArgumentsTests
{
    [Fact]
    public static void Ctor_Ok()
    {
        // Arrange
        var context = ResilienceContextPool.Shared.Get();
        var fault = new InvalidCastException();

        // Act
        var args = new OnFaultInjectedArguments(context, fault);

        // Assert
        args.Context.ShouldBe(context);
        args.Fault.ShouldBe(fault);
    }
}
