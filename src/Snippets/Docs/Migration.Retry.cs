using Polly.Retry;
using Snippets.Docs.Utils;

namespace Snippets.Docs;

internal static partial class Migration
{
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
        new ResiliencePipelineBuilder().AddRetry(new RetryStrategyOptions
        {
            ShouldHandle = new PredicateBuilder().Handle<SomeExceptionType>(),
            MaxRetryAttempts = 1,
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
            MaxRetryAttempts = int.MaxValue,
            Delay = TimeSpan.Zero,
        })
        .Build();

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
