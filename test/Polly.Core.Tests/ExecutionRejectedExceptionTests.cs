namespace Polly.Core.Tests;

public class ExecutionRejectedExceptionTests
{
    [Fact]
    public void Ctor_Ok()
    {
        new CustomException().Message.ShouldBe("Exception of type 'Polly.Core.Tests.ExecutionRejectedExceptionTests+CustomException' was thrown.");
        new CustomException("Dummy").Message.ShouldBe("Dummy");
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
