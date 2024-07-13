namespace Polly.Specs.Utilities;

public class LockTimeoutExceptionTests
{
    [Fact]
    public void Ctor_Ok()
    {
        const string Dummy = "dummy";
        var exception = new InvalidOperationException();

        new LockTimeoutException().Message.Should().Be("Timeout waiting for lock");
        new LockTimeoutException(Dummy).Message.Should().Be(Dummy);

        var rate = new LockTimeoutException(Dummy, exception);
        rate.Message.Should().Be(Dummy);
        rate.InnerException.Should().Be(exception);
    }
}
