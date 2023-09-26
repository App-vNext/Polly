using System.Net;
using System.Net.Http;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Polly.Retry;
using Polly.Testing;
using Polly.Timeout;
using Xunit;

namespace Snippets.Docs;

internal static class Testing
{
    public static void GetPipelineDescriptor()
    {
        #region get-pipeline-descriptor

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

        #endregion
    }

    public static void GetPipelineDescriptorGeneric()
    {
        #region get-pipeline-descriptor-generic

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

        #endregion
    }

    /// <summary>
    /// A dummy interface to represent a downstream call.
    /// </summary>
    private interface IDownstream
    {
        /// <summary>
        /// The method.
        /// </summary>
        /// <param name="token">The parameter.</param>
        /// <returns>The result.</returns>
        ValueTask<HttpResponseMessage> CallDownstream(CancellationToken token);
    }

    private class SUT
    {
        private readonly ResiliencePipeline<HttpResponseMessage> _pipeline;
        public SUT(IDownstream downstream, ResiliencePipeline<HttpResponseMessage> pipeline)
        {
            _pipeline = pipeline;
        }

        public Task RetrieveData()
        {
            _ = _pipeline.ToString(); // Just to make it an instance method
            return Task.CompletedTask;
        }
    }

    public static async Task AntiPattern_1()
    {
        #region testing-anti-pattern-1
        // Arrange
        var timeout = new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddTimeout(TimeSpan.FromSeconds(1))
            .Build();

        var mockDownstream = Substitute.For<IDownstream>();
        mockDownstream.CallDownstream(Arg.Any<CancellationToken>())
        .Returns((_) => { Thread.Sleep(5_000); return new HttpResponseMessage(); }, null);

        // Act
        var sut = new SUT(mockDownstream, timeout);
        await sut.RetrieveData();
        #endregion
    }

    public static async Task Pattern_11()
    {
        #region testing-pattern-1-1
        // Arrange
        var timeoutMock = ResiliencePipeline<HttpResponseMessage>.Empty;
        var mockDownstream = Substitute.For<IDownstream>();
        // setup the to-be-decorated method in a way that suits for your test case

        // Act
        var sut = new SUT(mockDownstream, timeoutMock);
        await sut.RetrieveData();
        #endregion
    }

    public static async Task Pattern_12()
    {
        #region testing-pattern-1-2
        // Arrange
        var mockDownstream = Substitute.For<IDownstream>();

        var timeoutMock = Substitute.For<MockableResilienceStrategy>();
        timeoutMock.ExecuteAsync(Arg.Any<Func<CancellationToken, ValueTask<HttpResponseMessage>>>(), Arg.Any<CancellationToken>())
                .Returns(new HttpResponseMessage(HttpStatusCode.Accepted));

        var optionsMock = Substitute.For<ResilienceStrategyOptions>();

        var pipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddStrategy(_ => timeoutMock, optionsMock)
            .Build();

        // Act
        var sut = new SUT(mockDownstream, pipeline);
        await sut.RetrieveData();
        #endregion
    }

    public static async Task Pattern_13()
    {
        #region testing-pattern-1-3
        // Arrange
        var mockDownstream = Substitute.For<IDownstream>();

        var timeoutMock = Substitute.For<MockableResilienceStrategy>();
        timeoutMock.ExecuteAsync(Arg.Any<Func<CancellationToken, ValueTask<HttpResponseMessage>>>(), Arg.Any<CancellationToken>())
                .ThrowsAsync(new TimeoutRejectedException());

        var optionsMock = Substitute.For<ResilienceStrategyOptions>();

        var pipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddStrategy(_ => timeoutMock, optionsMock)
            .Build();

        // Act
        var sut = new SUT(mockDownstream, pipeline);
        await sut.RetrieveData();
        #endregion
    }

    public abstract class MockableResilienceStrategy : ResilienceStrategy
    {
        public virtual async ValueTask<TResult?> ExecuteAsync<TResult>(Func<CancellationToken, ValueTask<TResult>> callback, CancellationToken token)
            => default;

        protected override ValueTask<Outcome<TResult>> ExecuteCore<TResult, TState>(Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback, ResilienceContext context, TState state)
            => default;
    }
}
