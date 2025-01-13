using System.ComponentModel.DataAnnotations;
using Polly.Utils;

namespace Polly.Core.Tests.Utils;

public class ValidationHelperTests
{
    [Fact]
    public void GetMemberName_Ok()
    {
        ValidationContext? context = null;
        context.GetMemberName().Should().BeNull();

        context = new ValidationContext(new object());
        context.GetMemberName().Should().BeNull();

        context = new ValidationContext(new object()) { MemberName = "X" };
        context.GetMemberName().Should().NotBeNull();
        context.GetMemberName()![0].Should().Be("X");
    }

    [Fact]
    public void GetDisplayName_Ok()
    {
        ValidationContext? context = null;
        context.GetDisplayName().Should().Be("");

        context = new ValidationContext(new object());
        context.GetDisplayName().Should().Be("Object");

        context = new ValidationContext(new object()) { DisplayName = "X" };
        context.GetDisplayName().Should().Be("X");
    }

    /// <summary>
    /// Test for <see href="https://github.com/App-vNext/Polly/issues/2412">validator concurrency issue</see>.
    /// </summary>
    [Fact]
    public void ValidateObject_SynchronizesValidation()
    {
        var detector = new ConcurrencyDetector();
        Parallel.For(0, 2, _ => ValidationHelper.ValidateObject(new(new TestOptions(detector), string.Empty)));

        detector.InvokedConcurrently.Should().BeFalse();
    }

    private sealed class TestOptions(ConcurrencyDetector detector) : IValidatableObject
    {
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            detector.Execute();
            yield break;
        }
    }

    private sealed class ConcurrencyDetector
    {
        private readonly object _sync = new();

        public bool InvokedConcurrently { get; private set; }

        public void Execute()
        {
            if (Monitor.TryEnter(_sync))
            {
                Thread.Sleep(1);
                Monitor.Exit(_sync);
            }
            else
            {
                InvokedConcurrently = true;
            }
        }
    }
}
