using Microsoft.Extensions.Options;
using Moq;
using Polly.Utils;

namespace Polly.Extensions.Tests.Utils;

public class OptionsReloadHelperTests
{
    [Fact]
    public void Ctor_NamedOptions()
    {
        var monitor = new Mock<IOptionsMonitor<string>>();

        monitor
            .Setup(m => m.OnChange(It.IsAny<Action<string, string?>>()))
            .Returns(() => Mock.Of<IDisposable>());

        var helper = new OptionsReloadHelper<string>(monitor.Object, "name");

        monitor.VerifyAll();
    }
}
