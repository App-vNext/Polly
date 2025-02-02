namespace Polly.Specs.Utilities;

public class LockTimeoutExceptionTests
{
    [Fact]
    public void Ctor_Ok()
    {
        const string Dummy = "dummy";
        var exception = new InvalidOperationException();

        new LockTimeoutException().Message.ShouldBe("Timeout waiting for lock");
        new LockTimeoutException(Dummy).Message.ShouldBe(Dummy);

        var rate = new LockTimeoutException(Dummy, exception);
        rate.Message.ShouldBe(Dummy);
        rate.InnerException.ShouldBe(exception);
    }
}
