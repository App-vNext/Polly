using Polly.Utils;

namespace Polly.Core.Tests.Utils;

public class TaskHelperTests
{
    public TaskHelperTests() => Trace.Listeners.Clear();

    [Fact]
    public void GetResult_ValueTaskT_Ok()
    {
        var result = TaskHelper.GetResult(new ValueTask<int>(42));
        result.Should().Be(42);

        result = TaskHelper.GetResult(GetValue());

        result.Should().Be(42);

        static async ValueTask<int> GetValue()
        {
            await Task.Delay(20);
            return 42;
        }
    }

    [Fact]
    public void GetResult_ValueTask_Ok()
    {
        TaskHelper.GetResult(default);

        this.Invoking(_ => TaskHelper.GetResult(GetValue())).Should().NotThrow();

        static async ValueTask<VoidResult> GetValue()
        {
            await Task.Delay(20);
            return VoidResult.Instance;
        }
    }
}
