namespace Polly.Core.Tests.Issues;

public partial class IssuesTests
{
    private MockTimeProvider TimeProvider { get; } = new MockTimeProvider().SetupUtcNow().SetupAnyDelay().SetupGetTimestamp().SetupTimestampFrequency();
}
