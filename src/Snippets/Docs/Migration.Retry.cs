using System.Runtime.Versioning;
using Polly.Retry;
using Snippets.Docs.Utils;

namespace Snippets.Docs;

internal static partial class Migration
{
    [RequiresPreviewFeatures]
    public static void Retry_V7()
    {
        #region migration-retry-v7

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
            // Its API should be familiar to v7 way of configuring what exceptions to handle.
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
    }
}
