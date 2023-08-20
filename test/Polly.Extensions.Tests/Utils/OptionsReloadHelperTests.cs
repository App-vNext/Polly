using Microsoft.Extensions.Options;
using NSubstitute;
using Polly.Utils;

namespace Polly.Extensions.Tests.Utils;

public class OptionsReloadHelperTests
{
    [Fact]
    public void Ctor_NamedOptions()
    {
        var disposable = Substitute.For<IDisposable>();
        var monitor = Substitute.For<IOptionsMonitor<string>>();

        monitor.OnChange(Arg.Any<Action<string, string?>>())
               .Returns(disposable);

        var helper = new OptionsReloadHelper<string>(monitor, "name");

        monitor.Received().OnChange(Arg.Any<Action<string, string?>>());
    }
}
