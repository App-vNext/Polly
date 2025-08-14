using Polly.Utils;

namespace Polly.Core.Tests.Utils;

public class TaskHelperTests
{
    public TaskHelperTests() => Trace.Listeners.Clear();

    [Fact]
    public void GetResult_ValueTaskT_Ok()
    {
        var result = TaskHelper.GetResult(new ValueTask<int>(42));
        result.ShouldBe(42);

        result = TaskHelper.GetResult(GetValue());

        result.ShouldBe(42);

        static async ValueTask<int> GetValue()
        {
            await Task.Delay(20);
            return 42;
        }
    }
}
