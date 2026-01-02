namespace Polly.Specs.Helpers.Bulkhead;

public class SilentOutputHelper : ITestOutputHelper
{
    public string Output => string.Empty;

    public void Write(string message)
    {
        // Do nothing: intentionally silent.
    }

    public void Write(string format, params object[] args)
    {
        // Do nothing: intentionally silent.
    }

    public void WriteLine(string message)
    {
        // Do nothing: intentionally silent.
    }

    public void WriteLine(string format, params object[] args)
    {
        // Do nothing: intentionally silent.
    }
}
