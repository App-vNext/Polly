using Polly.Utils;

namespace Polly.Core.Tests.Utils;

public class ExceptionUtilitiesTests
{
    [Fact]
    public void TrySetStackTrace_Ok()
    {
        var exception = new InvalidOperationException();

        ExceptionUtilities.TrySetStackTrace(exception);

        exception.StackTrace.ShouldNotBeNull();
        exception.StackTrace.ShouldContain("ExceptionUtilitiesTests");
    }

    [Fact]
    public void TrySetStackTrace_AlreadySet_NotOverwritten()
    {
        try
        {
            throw new InvalidOperationException();

        }
        catch (InvalidOperationException e)
        {
            var oldTrace = e.StackTrace;
            ExceptionUtilities.TrySetStackTrace(e);
            e.StackTrace.ShouldNotBeNull();
            e.StackTrace.ShouldBe(oldTrace);
        }
    }
}
