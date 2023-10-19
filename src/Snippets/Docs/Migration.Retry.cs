using System.Net;
using System.Net.Http;
using System.Runtime.Versioning;
using System.Text;
using Polly.Retry;
using Snippets.Docs.Utils;

namespace Snippets.Docs;

internal static partial class Migration
{
    [RequiresPreviewFeatures]
    public static void Retry_V7()
    {
        #region migration-retry-v7

        System.Convert.ToBase64String(Encoding.UTF8.GetBytes());

        // Retry once
        Policy
          .Handle<SomeExceptionType>()
          .Retry();

        // Retry multiple times
        Policy
          .Handle<SomeExceptionType>()
          .Retry(3);

        // Retry multiple times with callback
        Policy
            .Handle<SomeExceptionType>()
            .Retry(3, onRetry: (exception, retryCount) =>
            {
                // Add logic to be executed before each retry, such as logging
            });

        #endregion

        #region migration-retry-wait-v7

        // Retry forever
        Policy
            .Handle<SomeExceptionType>()
            .WaitAndRetryForever(_ => TimeSpan.FromSeconds(1));

        // Wait and retry multiple times
        Policy
          .Handle<SomeExceptionType>()
          .WaitAndRetry(3, _ => TimeSpan.FromSeconds(1));

        // Wait and retry multiple times with callback
        Policy
            .Handle<SomeExceptionType>()
            .WaitAndRetry(3, _ => TimeSpan.FromSeconds(1), onRetry: (exception, retryCount) =>
            {
                // Add logic to be executed before each retry, such as logging
            });

        // Wait and retry forever
        Policy
            .Handle<SomeExceptionType>()
            .WaitAndRetryForever(_ => TimeSpan.FromSeconds(1));

        #endregion

        #region migration-retry-reactive-v7

        // Wait and retry with result handling
        Policy
          .Handle<SomeExceptionType>()
          .OrResult<HttpResponseMessage>(response => response.StatusCode == HttpStatusCode.InternalServerError)
          .WaitAndRetry(3, _ => TimeSpan.FromSeconds(1));

        #endregion
    }

    public static void Retry_V8()
    {
        #region migration-retry-v8

        // Retry once
        //
        // Because we are adding retries to a non-generic pipeline,
        // we use the non-generic RetryStrategyOptions.
        new ResiliencePipelineBuilder().AddRetry(new RetryStrategyOptions
        {
            // PredicateBuilder is used to simplify the initialization of predicates.
            // Its API should be familiar to the v7 way of configuring what exceptions to handle.
            ShouldHandle = new PredicateBuilder().Handle<SomeExceptionType>(),
            MaxRetryAttempts = 1,
            // To disable waiting between retries, set the Delay property to TimeSpan.Zero.
            Delay = TimeSpan.Zero,
        })
        .Build();

        // Retry multiple times
        new ResiliencePipelineBuilder().AddRetry(new RetryStrategyOptions
        {
            ShouldHandle = new PredicateBuilder().Handle<SomeExceptionType>(),
            MaxRetryAttempts = 3,
            Delay = TimeSpan.Zero,
        })
        .Build();

        // Retry multiple times with callback
        new ResiliencePipelineBuilder().AddRetry(new RetryStrategyOptions
        {
            ShouldHandle = new PredicateBuilder().Handle<SomeExceptionType>(),
            MaxRetryAttempts = 3,
            Delay = TimeSpan.Zero,
            OnRetry = args =>
            {
                // Add logic to be executed before each retry, such as logging
                return default;
            }
        })
        .Build();

        // Retry forever
        new ResiliencePipelineBuilder().AddRetry(new RetryStrategyOptions
        {
            ShouldHandle = new PredicateBuilder().Handle<SomeExceptionType>(),
            // To retry forever, set the MaxRetryAttempts property to int.MaxValue.
            MaxRetryAttempts = int.MaxValue,
            Delay = TimeSpan.Zero,
        })
        .Build();

        #endregion

        #region migration-retry-wait-v8

        // Wait and retry multiple times
        new ResiliencePipelineBuilder().AddRetry(new RetryStrategyOptions
        {
            ShouldHandle = new PredicateBuilder().Handle<SomeExceptionType>(),
            MaxRetryAttempts = 3,
            Delay = TimeSpan.FromSeconds(1),
            BackoffType = DelayBackoffType.Constant
        })
        .Build();

        // Wait and retry multiple times with callback
        new ResiliencePipelineBuilder().AddRetry(new RetryStrategyOptions
        {
            ShouldHandle = new PredicateBuilder().Handle<SomeExceptionType>(),
            MaxRetryAttempts = 3,
            Delay = TimeSpan.FromSeconds(1),
            BackoffType = DelayBackoffType.Constant,
            OnRetry = args =>
            {
                // Add logic to be executed before each retry, such as logging
                return default;
            }
        })
        .Build();

        // Wait and retry forever
        new ResiliencePipelineBuilder().AddRetry(new RetryStrategyOptions
        {
            ShouldHandle = new PredicateBuilder().Handle<SomeExceptionType>(),
            MaxRetryAttempts = int.MaxValue,
            Delay = TimeSpan.FromSeconds(1),
            BackoffType = DelayBackoffType.Constant
        })
        .Build();

        #endregion

        #region migration-retry-reactive-v8

        // Shows how to add a retry strategy that also retries particular results.
        new ResiliencePipelineBuilder<HttpResponseMessage>().AddRetry(new RetryStrategyOptions<HttpResponseMessage>
        {
            // PredicateBuilder is a convenience API that can used to configure the ShouldHandle predicate.
            ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                .Handle<SomeExceptionType>()
                .HandleResult(result => result.StatusCode == HttpStatusCode.InternalServerError),
            MaxRetryAttempts = 3,
        })
        .Build();

        // The same as above, but using the switch expressions for best performance.
        new ResiliencePipelineBuilder<HttpResponseMessage>().AddRetry(new RetryStrategyOptions<HttpResponseMessage>
        {
            // Determine what results to retry using switch expressions.
            // Note that PredicateResult.True() is just a shortcut for "new ValueTask<bool>(true)".
            ShouldHandle = args => args.Outcome switch
            {
                { Exception: SomeExceptionType } => PredicateResult.True(),
                { Result: { StatusCode: HttpStatusCode.InternalServerError } } => PredicateResult.True(),
                _ => PredicateResult.False()
            },
            MaxRetryAttempts = 3,
        })
        .Build();

        #endregion
    }
}
