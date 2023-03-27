using Polly.Utils;

namespace Polly.Core.Tests.Utils;

public class SynchronousExecutionHelperTests
{
    public SynchronousExecutionHelperTests() => Trace.Listeners.Clear();

    [Fact]
    public void GetResult_ValueTaskT_Ok()
    {
        var result = SynchronousExecutionHelper.GetResult(new ValueTask<int>(42));
        result.Should().Be(42);

        result = SynchronousExecutionHelper.GetResult(GetValue());

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
        SynchronousExecutionHelper.GetResult(default);

        this.Invoking(_ => SynchronousExecutionHelper.GetResult(GetValue())).Should().NotThrow();

        static async ValueTask<VoidResult> GetValue()
        {
            await Task.Delay(20);
            return VoidResult.Instance;
        }
    }
}
