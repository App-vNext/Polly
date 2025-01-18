using NSubstitute;
using Polly.Utils;

namespace Polly.Core.Tests.Utils;

public class DisposeHelperTests
{
    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public void Dispose_Object_Ok(bool synchronous) =>
        DisposeHelper.TryDisposeSafeAsync(new object(), synchronous).AsTask().IsCompleted.ShouldBeTrue();

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public async Task Dispose_Disposable_Ok(bool synchronous)
    {
        using var disposable = new DisposableResult();
        await DisposeHelper.TryDisposeSafeAsync(disposable, synchronous);
        disposable.IsDisposed.ShouldBeTrue();
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public async Task Dispose_AsyncDisposable_Ok(bool synchronous)
    {
        var disposable = Substitute.For<IAsyncDisposable>();
        bool disposed = false;

        disposable.When(async (p) => await p.DisposeAsync())
                  .Do((_) => disposed = true);

        await DisposeHelper.TryDisposeSafeAsync(disposable, synchronous);

        disposed.ShouldBeTrue();
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public async Task Dispose_DisposeThrows_ExceptionHandled(bool synchronous)
    {
        var disposable = Substitute.For<IAsyncDisposable>();
        disposable.When(async v => await v.DisposeAsync()).Do((_) => throw new InvalidOperationException());

        await Should.NotThrowAsync(async () => await DisposeHelper.TryDisposeSafeAsync(disposable, synchronous));
    }

    [InlineData(true)]
    [InlineData(false)]
    [Theory]
    public async Task Dispose_EnsureCorrectOrder(bool synchronous)
    {
        using var disposable = new MyDisposable();

        await DisposeHelper.TryDisposeSafeAsync(disposable, synchronous);

        disposable.IsDisposedAsync.ShouldBe(!synchronous);
        disposable.IsDisposed.ShouldBe(synchronous);
    }

    private class MyDisposable : DisposableResult, IAsyncDisposable
    {
        public bool IsDisposedAsync { get; set; }

        public ValueTask DisposeAsync()
        {
            IsDisposedAsync = true;
            return default;
        }
    }
}
