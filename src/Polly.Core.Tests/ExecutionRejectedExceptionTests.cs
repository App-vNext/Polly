namespace Polly.Core.Tests.Timeout;

public class ExecutionRejectedExceptionTests
{
    [Fact]
    public void Ctor_Ok()
    {
        new CustomException().Message.Should().Be("Exception of type 'Polly.Core.Tests.Timeout.ExecutionRejectedExceptionTests+CustomException' was thrown.");
        new CustomException("Dummy").Message.Should().Be("Dummy");
    }

    private class CustomException : ExecutionRejectedException
    {
        public CustomException()
        {
        }

        public CustomException(string message)
            : base(message)
        {
        }
    }
}
