namespace Polly.Core.Tests.Issues;

public partial class IssuesTests
{
    private FakeTimeProvider TimeProvider { get; } = new FakeTimeProvider().SetupUtcNow().SetupAnyDelay().SetupGetTimestamp();
}
