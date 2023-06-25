using Microsoft.Extensions.Time.Testing;

namespace Polly.Core.Tests.Issues;

public partial class IssuesTests
{
    private FakeTimeProvider TimeProvider { get; } = new FakeTimeProvider();
}
