using Xunit.Abstractions;

namespace Polly.Specs.Helpers.Bulkhead
{
    public class SilentOutputHelper : ITestOutputHelper
    {
        public void WriteLine(string message)
        {
            // Do nothing: intentionally silent.
        }

        public void WriteLine(string format, params object[] args)
        {
            // Do nothing: intentionally silent.
        }
    }
}
