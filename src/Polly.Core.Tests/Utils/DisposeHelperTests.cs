using System;
using System.Threading.Tasks;
using Moq;
using Polly.Utils;

namespace Polly.Core.Tests.Utils;

public class DisposeHelperTests
{
    [Fact]
    public void Dispose_Object_Ok()
    {
        DisposeHelper.TryDisposeSafeAsync(new object()).AsTask().IsCompleted.Should().BeTrue();
    }

    [Fact]
    public async Task Dispose_Disposable_Ok()
    {
        using var disposable = new DisposableResult();
        await DisposeHelper.TryDisposeSafeAsync(disposable);
        disposable.IsDisposed.Should().BeTrue();
    }

    [Fact]
    public async Task Dispose_AsyncDisposable_Ok()
    {
        var disposable = new Mock<IAsyncDisposable>();
        bool disposed = false;
        disposable.Setup(v => v.DisposeAsync()).Returns(default(ValueTask)).Callback(() => disposed = true);

        await DisposeHelper.TryDisposeSafeAsync(disposable.Object);

        disposed.Should().BeTrue();
    }

    [Fact]
    public async Task Dispose_DisposeThrows_ExceptionHandled()
    {
        var disposable = new Mock<IAsyncDisposable>();
        disposable.Setup(v => v.DisposeAsync()).Throws(new InvalidOperationException());

        await disposable.Object.Invoking(async _ => await DisposeHelper.TryDisposeSafeAsync(disposable.Object)).Should().NotThrowAsync();
    }
}
