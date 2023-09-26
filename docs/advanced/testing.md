# Testing

This document explains how to test Polly’s resilience pipelines. You should not test how the resilience pipelines operate internally, but rather test your own settings or custom delegates.

To make the testing process simpler, Polly offers the [`Polly.Testing`](https://www.nuget.org/packages/Polly.Testing/) package. This package has a range of APIs designed to help you test the setup and combination of resilience pipelines in your user code.

## Usage

Begin by adding the `Polly.Testing` package to your test project:

```sh
dotnet add package Polly.Testing
```

Use the `GetPipelineDescriptor` extension method to get the [`ResiliencePipelineDescriptor`](xref:Polly.Testing.ResiliencePipelineDescriptor) which provides details on the pipeline's composition:

<!-- snippet: get-pipeline-descriptor -->
```cs
// Build your resilience pipeline.
ResiliencePipeline pipeline = new ResiliencePipelineBuilder()
    .AddRetry(new RetryStrategyOptions
    {
        MaxRetryAttempts = 4
    })
    .AddTimeout(TimeSpan.FromSeconds(1))
    .Build();

// Retrieve the descriptor.
ResiliencePipelineDescriptor descriptor = pipeline.GetPipelineDescriptor();

// Check the pipeline's composition with the descriptor.
Assert.Equal(2, descriptor.Strategies.Count);

// Verify the retry settings.
var retryOptions = Assert.IsType<RetryStrategyOptions>(descriptor.Strategies[0].Options);
Assert.Equal(4, retryOptions.MaxRetryAttempts);

// Confirm the timeout settings.
var timeoutOptions = Assert.IsType<TimeoutStrategyOptions>(descriptor.Strategies[1].Options);
Assert.Equal(TimeSpan.FromSeconds(1), timeoutOptions.Timeout);
```
<!-- endSnippet -->

The `GetPipelineDescriptor` extension method is also available for the generic `ResiliencePipeline<T>`:

<!-- snippet: get-pipeline-descriptor-generic -->
```cs
// Construct your resilience pipeline.
ResiliencePipeline<string> pipeline = new ResiliencePipelineBuilder<string>()
    .AddRetry(new RetryStrategyOptions<string>
    {
        MaxRetryAttempts = 4
    })
    .AddTimeout(TimeSpan.FromSeconds(1))
    .Build();

// Obtain the descriptor.
ResiliencePipelineDescriptor descriptor = pipeline.GetPipelineDescriptor();

// Check the pipeline's composition with the descriptor.
// ...
```
<!-- endSnippet -->


## Patterns and anti-patterns

Over the years, many developers have used Polly in various ways. Some of these recurring patterns may not be ideal. This section highlights the recommended practices and those to avoid.

### 1 - Recreating the strategies inside unit tests

Imagine that you have a class that receives a `ResiliencePipeline<T>` as a constructor's parameter for testability.

❌ DON'T

Recreate the strategy inside the unit test:

<!-- snippet: testing-anti-pattern-1 -->
```cs
// Arrange
var timeout = new ResiliencePipelineBuilder<HttpResponseMessage>()
    .AddTimeout(TimeSpan.FromSeconds(1))
    .Build();

var downstreamMock = Substitute.For<IDownstream>();
downstreamMock.CallDownstreamAsync(Arg.Any<CancellationToken>())
        .Returns((_) => { Thread.Sleep(5_000); return new HttpResponseMessage(); }, null);

// Act
var sut = new SUT(downstreamMock, timeout);
await sut.RetrieveData();
```
<!-- endSnippet -->

**Reasoning**:

- Depending on the your strategy setup, the execution time might be greatly increased (from milliseconds to seconds).
  - For example if you have a retry with 3 `MaxRetryAttempts` and 1 seconds `Delay`.
- This violates one of the F.I.R.S.T testing principles, namely the **Fast** one.

✅ DO

Use `Empty` or mock `ExecuteAsync`:

- If you want to test your to-be-decorated code like there is no strategy applied:

<!-- snippet: testing-pattern-1-1 -->
```cs
// Arrange
var timeoutMock = ResiliencePipeline<HttpResponseMessage>.Empty;
var downstreamMock = Substitute.For<IDownstream>();
// setup the to-be-decorated method in a way that suits for your test case

// Act
var sut = new SUT(downstreamMock, timeoutMock);
await sut.RetrieveData();
```
<!-- endSnippet -->

- If you want to verify that your code handles success cases as expected:

<!-- snippet: testing-pattern-1-2 -->
```cs
// Arrange
var downstreamMock = Substitute.For<IDownstream>();

var timeoutMock = Substitute.For<MockableResilienceStrategy>();
timeoutMock.ExecuteAsync(Arg.Any<Func<CancellationToken, ValueTask<HttpResponseMessage>>>(), Arg.Any<CancellationToken>())
        .Returns(new HttpResponseMessage(HttpStatusCode.Accepted));

var optionsMock = Substitute.For<ResilienceStrategyOptions>();

var pipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
    .AddStrategy(_ => timeoutMock, optionsMock)
    .Build();

// Act
var sut = new SUT(downstreamMock, pipeline);
await sut.RetrieveData();
```
<!-- endSnippet -->

- If you want to verify that your code handles failure cases as expected:

<!-- snippet: testing-pattern-1-3 -->
```cs
// Arrange
var downstreamMock = Substitute.For<IDownstream>();

var timeoutMock = Substitute.For<MockableResilienceStrategy>();
timeoutMock.ExecuteAsync(Arg.Any<Func<CancellationToken, ValueTask<HttpResponseMessage>>>(), Arg.Any<CancellationToken>())
        .ThrowsAsync(new TimeoutRejectedException());

var optionsMock = Substitute.For<ResilienceStrategyOptions>();

var pipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
    .AddStrategy(_ => timeoutMock, optionsMock)
    .Build();

// Act
var sut = new SUT(downstreamMock, pipeline);
await sut.RetrieveData();
```
<!-- endSnippet -->

**Reasoning**:

- Polly currently does not support direct mocking of `ResiliencePipeline`.
  - As a workaround you can derive from the `ResilienceStrategy` class.
  - You can expose an `ExecuteAsync` method with the same signature as you it inside the system-under-test.
  - Then you add that strategy to a pipeline.
- This approach allows you to define the expected behaviour for your resilience pipeline which meets your test case's needs.

> [!NOTE]
> The above presented solution is a workaround and later Polly might add support for this use case as well in the `Polly.Testing` package.
