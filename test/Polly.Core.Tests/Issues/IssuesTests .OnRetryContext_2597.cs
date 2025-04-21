namespace Polly.Core.Tests.Issues;

public partial class IssuesTests
{
    [Fact]
    public async Task OnRetry_Contains_User_Context_2597()
    {
        // Arrange
        var propertyKey = Guid.NewGuid().ToString();
        var propertyValue = Guid.NewGuid().ToString();

        var key = new ResiliencePropertyKey<string>(propertyKey);

        var pipeline = new ResiliencePipelineBuilder()
            .AddRetry(new()
            {
                MaxRetryAttempts = 2,
                OnRetry = (args) =>
                {
                    args.Context.Properties.TryGetValue(key, out var actualValue).ShouldBeTrue();
                    actualValue.ShouldBe(propertyValue);
                    return default;
                }
            })
            .Build();

        var executed = false;

        // Act
        var context = ResilienceContextPool.Shared.Get();
        context.Properties.Set(key, propertyValue);

        var actual = await Should.ThrowAsync<InvalidOperationException>(
            async () => await pipeline.ExecuteAsync(
                (context) =>
                {
                    context.Properties.TryGetValue(key, out var actualValue).ShouldBeTrue();
                    actualValue.ShouldBe(propertyValue);

                    executed = true;

                    throw new InvalidOperationException(propertyValue);
                },
                context));

        // Assert
        actual.Message.ShouldBe(propertyValue);
        executed.ShouldBeTrue();
    }
}
