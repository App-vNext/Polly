namespace Polly.Specs;

public static class PolicyOverloadSmokeSpecs
{
    [Fact]
    public static void Sync_policy_execute_overloads_with_cancellation_and_context_data_should_delegate_correctly()
    {
        Policy policy = Policy.NoOp();
        using var cancellationTokenSource = new CancellationTokenSource();
        CancellationToken cancellationToken = cancellationTokenSource.Token;
        IDictionary<string, object> contextData = CreateDictionary("key", "value");

        Context? capturedContext = null;
        CancellationToken capturedToken = default;

        policy.Execute((context, token) =>
        {
            capturedContext = context;
            capturedToken = token;
        }, contextData, cancellationToken);

        capturedContext.ShouldNotBeNull();
        capturedContext["key"].ShouldBe("value");
        capturedToken.ShouldBe(cancellationToken);

        policy.Execute(token => capturedToken = token, cancellationToken);
        capturedToken.ShouldBe(cancellationToken);

        policy.Execute((context, token) =>
        {
            capturedContext = context;
            capturedToken = token;
            return 17;
        }, contextData, cancellationToken).ShouldBe(17);

        policy.Execute(token =>
        {
            capturedToken = token;
            return 23;
        }, cancellationToken).ShouldBe(23);

        policy.ExecuteAndCapture(token => capturedToken = token, cancellationToken)
            .Outcome.ShouldBe(OutcomeType.Successful);
        capturedToken.ShouldBe(cancellationToken);

        policy.ExecuteAndCapture((context, token) =>
        {
            capturedContext = context;
            capturedToken = token;
        }, contextData, cancellationToken).Context["key"].ShouldBe("value");

        policy.ExecuteAndCapture(token =>
        {
            capturedToken = token;
            return 29;
        }, cancellationToken).Result.ShouldBe(29);

        policy.ExecuteAndCapture((context, token) =>
        {
            capturedContext = context;
            capturedToken = token;
            return 31;
        }, contextData, cancellationToken).Result.ShouldBe(31);

        Policy<ResultPrimitive> genericPolicy = Policy.NoOp<ResultPrimitive>();

        genericPolicy.Execute((context, token) =>
        {
            capturedContext = context;
            capturedToken = token;
            return ResultPrimitive.Good;
        }, contextData, cancellationToken).ShouldBe(ResultPrimitive.Good);

        genericPolicy.ExecuteAndCapture(token =>
        {
            capturedToken = token;
            return ResultPrimitive.Good;
        }, cancellationToken).Outcome.ShouldBe(OutcomeType.Successful);

        genericPolicy.ExecuteAndCapture((context, token) =>
        {
            capturedContext = context;
            capturedToken = token;
            return ResultPrimitive.Good;
        }, contextData, cancellationToken).Context["key"].ShouldBe("value");
    }

    [Fact]
    public static async Task Async_policy_execute_overloads_with_cancellation_and_context_data_should_delegate_correctly()
    {
        AsyncPolicy policy = Policy.NoOpAsync();
        using var cancellationTokenSource = new CancellationTokenSource();
        CancellationToken cancellationToken = cancellationTokenSource.Token;
        IDictionary<string, object> contextData = CreateDictionary("key", "value");

        Context? capturedContext = null;
        CancellationToken capturedToken = default;

        await policy.ExecuteAsync((context, token) =>
        {
            capturedContext = context;
            capturedToken = token;
            return TaskHelper.EmptyTask;
        }, contextData, cancellationToken, true);

        capturedContext.ShouldNotBeNull();
        capturedContext["key"].ShouldBe("value");
        capturedToken.ShouldBe(cancellationToken);

        await policy.ExecuteAsync(token =>
        {
            capturedToken = token;
            return TaskHelper.EmptyTask;
        }, cancellationToken, true);

        capturedToken.ShouldBe(cancellationToken);

        (await policy.ExecuteAsync((context, token) =>
        {
            capturedContext = context;
            capturedToken = token;
            return Task.FromResult(37);
        }, contextData, cancellationToken)).ShouldBe(37);

        (await policy.ExecuteAsync(token =>
        {
            capturedToken = token;
            return Task.FromResult(41);
        }, cancellationToken, true)).ShouldBe(41);

        (await policy.ExecuteAndCaptureAsync(token =>
        {
            capturedToken = token;
            return TaskHelper.EmptyTask;
        }, cancellationToken)).Outcome.ShouldBe(OutcomeType.Successful);

        (await policy.ExecuteAndCaptureAsync((context, token) =>
        {
            capturedContext = context;
            capturedToken = token;
            return TaskHelper.EmptyTask;
        }, contextData, cancellationToken)).Context["key"].ShouldBe("value");

        (await policy.ExecuteAndCaptureAsync(token =>
        {
            capturedToken = token;
            return TaskHelper.EmptyTask;
        }, cancellationToken, true)).Outcome.ShouldBe(OutcomeType.Successful);

        (await policy.ExecuteAndCaptureAsync(token =>
        {
            capturedToken = token;
            return Task.FromResult(43);
        }, cancellationToken)).Result.ShouldBe(43);

        (await policy.ExecuteAndCaptureAsync((context, token) =>
        {
            capturedContext = context;
            capturedToken = token;
            return Task.FromResult(47);
        }, contextData, cancellationToken)).Result.ShouldBe(47);

        AsyncPolicy<ResultPrimitive> genericPolicy = Policy.NoOpAsync<ResultPrimitive>();

        (await genericPolicy.ExecuteAsync(token =>
        {
            capturedToken = token;
            return Task.FromResult(ResultPrimitive.Good);
        }, cancellationToken, true)).ShouldBe(ResultPrimitive.Good);

        (await genericPolicy.ExecuteAsync((context, token) =>
        {
            capturedContext = context;
            capturedToken = token;
            return Task.FromResult(ResultPrimitive.Good);
        }, contextData, cancellationToken, true)).ShouldBe(ResultPrimitive.Good);

        (await genericPolicy.ExecuteAndCaptureAsync(token =>
        {
            capturedToken = token;
            return Task.FromResult(ResultPrimitive.Good);
        }, cancellationToken)).Outcome.ShouldBe(OutcomeType.Successful);

        (await genericPolicy.ExecuteAndCaptureAsync(token =>
        {
            capturedToken = token;
            return Task.FromResult(ResultPrimitive.Good);
        }, cancellationToken, true)).Outcome.ShouldBe(OutcomeType.Successful);

        (await genericPolicy.ExecuteAndCaptureAsync((context, token) =>
        {
            capturedContext = context;
            capturedToken = token;
            return Task.FromResult(ResultPrimitive.Good);
        }, contextData, cancellationToken)).Context["key"].ShouldBe("value");

        (await genericPolicy.ExecuteAndCaptureAsync((context, token) =>
        {
            capturedContext = context;
            capturedToken = token;
            return Task.FromResult(ResultPrimitive.Good);
        }, contextData, cancellationToken, true)).Context["key"].ShouldBe("value");
    }
}
