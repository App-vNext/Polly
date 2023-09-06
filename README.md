> **Note**
>
> **Important Announcement: Architectural changes in v8**
>
> Major performance improvements are on the way! Please see our [blog post](https://www.thepollyproject.org/2023/03/03/we-want-your-feedback-introducing-polly-v8/) to learn more and provide feedback in the [related GitHub issue](https://github.com/App-vNext/Polly/issues/1048).
>
> :rotating_light::rotating_light: **Polly v8 feature-complete!** :rotating_light::rotating_light:
> - Polly v8 Beta 1 is now available on [NuGet.org](https://www.nuget.org/packages/Polly/8.0.0-beta.1).
> - The Beta 1 version is considered feature-complete and the public API surface is stable.
> - Explore the [v8 documentation](https://github.com/App-vNext/Polly/blob/main/README_V8.md).

# Polly

Polly is a .NET resilience and transient-fault-handling library that allows developers to express policies such as Retry, Circuit Breaker, Timeout, Bulkhead Isolation, Rate-limiting and Fallback in a fluent and thread-safe manner.

Polly targets .NET Standard 1.1 ([coverage](https://docs.microsoft.com/en-us/dotnet/standard/net-standard#net-implementation-support): .NET Core 1.0, Mono, Xamarin, UWP, WP8.1+) and .NET Standard 2.0+ ([coverage](https://docs.microsoft.com/en-us/dotnet/standard/net-standard#net-implementation-support): .NET Core 2.0+, .NET Core 3.0, and later Mono, Xamarin and UWP targets). The NuGet package also includes direct targets for .NET Framework 4.6.1 and 4.7.2.

For versions supporting earlier targets such as .NET4.0 and .NET3.5, see the [supported targets](https://github.com/App-vNext/Polly/wiki/Supported-targets) grid.

[<img align="right" src="https://github.com/dotnet/swag/raw/main/logo/dotnetfoundation_v4_small.png" width="100" />](https://www.dotnetfoundation.org/)
We are a member of the [.NET Foundation](https://www.dotnetfoundation.org/about)!

**Keep up to date with new feature announcements, tips & tricks, and other news through [www.thepollyproject.org](https://www.thepollyproject.org)**

[![Build status](https://github.com/App-vNext/Polly/workflows/build/badge.svg?branch=main&event=push)](https://github.com/App-vNext/Polly/actions?query=workflow%3Abuild+branch%3Amain+event%3Apush) [![Code coverage](https://codecov.io/gh/App-vNext/Polly/branch/main/graph/badge.svg)](https://codecov.io/gh/App-vNext/Polly) [![Slack Status](http://www.pollytalk.org/badge.svg)](http://www.pollytalk.org)

![Polly logo](https://raw.github.com/App-vNext/Polly/main/Polly-Logo.png)

## NuGet Packages

### Polly v7

[![NuGet vesion](https://buildstats.info/nuget/Polly?includePreReleases=false)](https://www.nuget.org/packages/Polly/)

### Polly v8 Pre-releases

| **Package** | **Latest Version** |
|:--|:--|
| Polly | [![NuGet](https://buildstats.info/nuget/Polly?includePreReleases=true)](https://www.nuget.org/packages/Polly/ "Download Polly from NuGet.org") |
| Polly.Core | [![NuGet](https://buildstats.info/nuget/Polly.Core?includePreReleases=true)](https://www.nuget.org/packages/Polly.Core/ "Download Polly.Core from NuGet.org") |
| Polly.Extensions | [![NuGet](https://buildstats.info/nuget/Polly.Extensions?includePreReleases=true)](https://www.nuget.org/packages/Polly.Extensions/ "Download Polly.Extensions from NuGet.org") |
| Polly.RateLimiting | [![NuGet](https://buildstats.info/nuget/Polly.RateLimiting?includePreReleases=true)](https://www.nuget.org/packages/Polly.RateLimiting/ "Download Polly.RateLimiting from NuGet.org") |
| Polly.Testing | [![NuGet](https://buildstats.info/nuget/Polly.Testing?includePreReleases=true)](https://www.nuget.org/packages/Polly.Testing/ "Download Polly.Testing from NuGet.org") |

## Get Started

### Installing via the .NET SDK

```sh
dotnet add package Polly
```

### Supported targets

For details of supported compilation targets by version, see the [supported targets](https://github.com/App-vNext/Polly/wiki/Supported-targets) grid.

### Role of the readme and the wiki

This ReadMe aims to give a quick overview of all Polly features - including enough to get you started with any policy.  For deeper detail on any policy, and many other aspects of Polly, be sure also to check out the [wiki documentation](https://github.com/App-vNext/Polly/wiki).

## Release notes

* The [changelog](https://github.com/App-vNext/Polly/blob/main/CHANGELOG.md) describes changes by release.
* We tag Pull Requests and Issues with [milestones](https://github.com/App-vNext/Polly/milestones) which match to NuGet package release numbers.
* Breaking changes are called out in the wiki ([v7](https://github.com/App-vNext/Polly/wiki/Polly-v7-breaking-changes); [v6](https://github.com/App-vNext/Polly/wiki/Polly-v6-breaking-changes)) with simple notes on any necessary steps to upgrade.

## Resilience policies

Polly offers multiple resilience policies.

In addition to the detailed pages on each policy, an [introduction to the role of each policy in resilience engineering](https://github.com/App-vNext/Polly/wiki/Transient-fault-handling-and-proactive-resilience-engineering)  is also provided in the wiki.

|Policy| Premise | Aka| How does the policy mitigate?|
| ------------- | ------------- |:-------------: |------------- |
|**Retry** <br/>(policy family)<br/><sub>([quickstart](#retry)&nbsp;;&nbsp;[deep](https://github.com/App-vNext/Polly/wiki/Retry))</sub>|Many faults are transient and may self-correct after a short delay.| "Maybe it's just a blip" |  Allows configuring automatic retries. |
|**Circuit-breaker**<br/>(policy family)<br/><sub>([quickstart](#circuit-breaker)&nbsp;;&nbsp;[deep](https://github.com/App-vNext/Polly/wiki/Circuit-Breaker))</sub>|When a system is seriously struggling, failing fast is better than making users/callers wait.  <br/><br/>Protecting a faulting system from overload can help it recover. | "Stop doing it if it hurts" <br/><br/>"Give that system a break" | Breaks the circuit (blocks executions) for a period, when faults exceed some pre-configured threshold. |
|**Timeout**<br/><sub>([quickstart](#timeout)&nbsp;;&nbsp;[deep](https://github.com/App-vNext/Polly/wiki/Timeout))</sub>|Beyond a certain wait, a success result is unlikely.| "Don't wait forever"  |Guarantees the caller won't have to wait beyond the timeout. |
|**Bulkhead Isolation**<br/><sub>([quickstart](#bulkhead)&nbsp;;&nbsp;[deep](https://github.com/App-vNext/Polly/wiki/Bulkhead))</sub>|When a process faults, multiple failing calls can stack up (if unbounded) and can easily swamp resource (threads/ CPU/ memory) in a host.  <br/><br/>This can affect performance more widely by starving other operations of resource, bringing down the host, or causing cascading failures upstream. | "One fault shouldn't sink the whole ship"  |Constrains the governed actions to a fixed-size resource pool, isolating their potential  to affect others. |
|**Rate-limit**<br/><sub>([quickstart](#rate-limit)&nbsp;;&nbsp;[deep](https://github.com/App-vNext/Polly/wiki/Rate-Limit))</sub>|Limiting the rate a system handles requests is another way to control load. <br/><br/> This can apply to the way your system accepts incoming calls, and/or to the way you call downstream services. | "Slow down a bit, will you?"  |Constrains executions to not exceed a certain rate. |
|**Cache**<br/><sub>([quickstart](#cache)&nbsp;;&nbsp;[deep](https://github.com/App-vNext/Polly/wiki/Cache))</sub>|Some proportion of requests may be similar.| "You've asked that one before"  |Provides a response from cache if known. <br/><br/>Stores responses automatically in cache, when first retrieved. |
|**Fallback**<br/><sub>([quickstart](#fallback)&nbsp;;&nbsp;[deep](https://github.com/App-vNext/Polly/wiki/Fallback))</sub>|Things will still fail - plan what you will do when that happens.| "Degrade gracefully"  |Defines an alternative value to be returned (or action to be executed) on failure. |
|**PolicyWrap**<br/><sub>([quickstart](#policywrap)&nbsp;;&nbsp;[deep](https://github.com/App-vNext/Polly/wiki/PolicyWrap))</sub>|Different faults require different strategies; resilience means using a combination.| "Defence in depth"  |Allows any of the above policies to be combined flexibly. |

## Usage &ndash; fault-handling, reactive policies

Fault-handling policies handle specific exceptions thrown by, or results returned by, the delegates you execute through the policy.

### Step 1 : Specify the exceptions/faults you want the policy to handle

```csharp
// Single exception type
Policy
  .Handle<HttpRequestException>()

// Single exception type with condition
Policy
  .Handle<SqlException>(ex => ex.Number == 1205)

// Multiple exception types
Policy
  .Handle<HttpRequestException>()
  .Or<OperationCanceledException>()

// Multiple exception types with condition
Policy
  .Handle<SqlException>(ex => ex.Number == 1205)
  .Or<ArgumentException>(ex => ex.ParamName == "example")

// Inner exceptions of ordinary exceptions or AggregateException, with or without conditions
// (HandleInner matches exceptions at both the top-level and inner exceptions)
Policy
  .HandleInner<HttpRequestException>()
  .OrInner<OperationCanceledException>(ex => ex.CancellationToken != myToken)
```

### Step 1b: (optionally) Specify return results you want to handle

From Polly v4.3.0 onwards, policies wrapping calls returning a `TResult` can also handle `TResult` return values:

```csharp
// Handle return value with condition
Policy
  .HandleResult<HttpResponseMessage>(r => r.StatusCode == HttpStatusCode.NotFound)

// Handle multiple return values
Policy
  .HandleResult<HttpResponseMessage>(r => r.StatusCode == HttpStatusCode.InternalServerError)
  .OrResult(r => r.StatusCode == HttpStatusCode.BadGateway)

// Handle primitive return values (implied use of .Equals())
Policy
  .HandleResult<HttpStatusCode>(HttpStatusCode.InternalServerError)
  .OrResult(HttpStatusCode.BadGateway)

// Handle both exceptions and return values in one policy
HttpStatusCode[] httpStatusCodesWorthRetrying = {
   HttpStatusCode.RequestTimeout, // 408
   HttpStatusCode.InternalServerError, // 500
   HttpStatusCode.BadGateway, // 502
   HttpStatusCode.ServiceUnavailable, // 503
   HttpStatusCode.GatewayTimeout // 504
};
HttpResponseMessage result = await Policy
  .Handle<HttpRequestException>()
  .OrResult<HttpResponseMessage>(r => httpStatusCodesWorthRetrying.Contains(r.StatusCode))
  .RetryAsync(...)
  .ExecuteAsync( /* some Func<Task<HttpResponseMessage>> */ )
```

For more information, see [Handling Return Values](#handing-return-values-and-policytresult) at foot of this readme.

### Step 2 : Specify how the policy should handle those faults

#### Retry

```csharp
// Retry once
Policy
  .Handle<SomeExceptionType>()
  .Retry()

// Retry multiple times
Policy
  .Handle<SomeExceptionType>()
  .Retry(3)

// Retry multiple times, calling an action on each retry
// with the current exception and retry count
Policy
    .Handle<SomeExceptionType>()
    .Retry(3, onRetry: (exception, retryCount) =>
    {
        // Add logic to be executed before each retry, such as logging
    });

// Retry multiple times, calling an action on each retry
// with the current exception, retry count and context
// provided to Execute()
Policy
    .Handle<SomeExceptionType>()
    .Retry(3, onRetry: (exception, retryCount, context) =>
    {
        // Add logic to be executed before each retry, such as logging
    });
```

#### Retry forever (until succeeds)

```csharp

// Retry forever
Policy
  .Handle<SomeExceptionType>()
  .RetryForever()

// Retry forever, calling an action on each retry with the
// current exception
Policy
  .Handle<SomeExceptionType>()
  .RetryForever(onRetry: exception =>
  {
        // Add logic to be executed before each retry, such as logging
  });

// Retry forever, calling an action on each retry with the
// current exception and context provided to Execute()
Policy
  .Handle<SomeExceptionType>()
  .RetryForever(onRetry: (exception, context) =>
  {
        // Add logic to be executed before each retry, such as logging
  });
```

`RetryForever` does not actually retry forever; it will retry up to `int.MaxValue` (2147483647) times. Depending on what is done in the policy delegate this may take an exceedingly long time, but the policy will eventually hit `int.MaxValue` retries, get the last exception and stop retrying.

#### Wait and retry

```csharp
// Retry, waiting a specified duration between each retry.
// (The wait is imposed on catching the failure, before making the next try.)
Policy
  .Handle<SomeExceptionType>()
  .WaitAndRetry(new[]
  {
    TimeSpan.FromSeconds(1),
    TimeSpan.FromSeconds(2),
    TimeSpan.FromSeconds(3)
  });

// Retry, waiting a specified duration between each retry,
// calling an action on each retry with the current exception
// and duration
Policy
  .Handle<SomeExceptionType>()
  .WaitAndRetry(new[]
  {
    TimeSpan.FromSeconds(1),
    TimeSpan.FromSeconds(2),
    TimeSpan.FromSeconds(3)
  }, (exception, timeSpan) => {
    // Add logic to be executed before each retry, such as logging
  });

// Retry, waiting a specified duration between each retry,
// calling an action on each retry with the current exception,
// duration and context provided to Execute()
Policy
  .Handle<SomeExceptionType>()
  .WaitAndRetry(new[]
  {
    TimeSpan.FromSeconds(1),
    TimeSpan.FromSeconds(2),
    TimeSpan.FromSeconds(3)
  }, (exception, timeSpan, context) => {
    // Add logic to be executed before each retry, such as logging
  });

// Retry, waiting a specified duration between each retry,
// calling an action on each retry with the current exception,
// duration, retry count, and context provided to Execute()
Policy
  .Handle<SomeExceptionType>()
  .WaitAndRetry(new[]
  {
    TimeSpan.FromSeconds(1),
    TimeSpan.FromSeconds(2),
    TimeSpan.FromSeconds(3)
  }, (exception, timeSpan, retryCount, context) => {
    // Add logic to be executed before each retry, such as logging
  });

// Retry a specified number of times, using a function to
// calculate the duration to wait between retries based on
// the current retry attempt (allows for exponential back-off)
// In this case will wait for
//  2 ^ 1 = 2 seconds then
//  2 ^ 2 = 4 seconds then
//  2 ^ 3 = 8 seconds then
//  2 ^ 4 = 16 seconds then
//  2 ^ 5 = 32 seconds
Policy
  .Handle<SomeExceptionType>()
  .WaitAndRetry(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

// Retry a specified number of times, using a function to
// calculate the duration to wait between retries based on
// the current retry attempt, calling an action on each retry
// with the current exception, duration and context provided
// to Execute()
Policy
  .Handle<SomeExceptionType>()
  .WaitAndRetry(
    5,
    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
    (exception, timeSpan, context) => {
      // Add logic to be executed before each retry, such as logging
    }
  );

// Retry a specified number of times, using a function to
// calculate the duration to wait between retries based on
// the current retry attempt, calling an action on each retry
// with the current exception, duration, retry count, and context
// provided to Execute()
Policy
  .Handle<SomeExceptionType>()
  .WaitAndRetry(
    5,
    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
    (exception, timeSpan, retryCount, context) => {
      // Add logic to be executed before each retry, such as logging
    }
  );
```

The above code demonstrates how to build common wait-and-retry patterns from scratch, but our community also came up with an awesome contrib to wrap the common cases in helper methods: see [Polly.Contrib.WaitAndRetry](https://github.com/Polly-Contrib/Polly.Contrib.WaitAndRetry/).

For `WaitAndRetry` policies handling Http Status Code 429 Retry-After, see [wiki documentation](https://github.com/App-vNext/Polly/wiki/Retry#retryafter-when-the-response-specifies-how-long-to-wait).

#### Wait and retry forever (until succeeds)

```csharp

// Wait and retry forever
Policy
  .Handle<SomeExceptionType>()
  .WaitAndRetryForever(retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

// Wait and retry forever, calling an action on each retry with the
// current exception and the time to wait
Policy
  .Handle<SomeExceptionType>()
  .WaitAndRetryForever(
    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
    (exception, timespan) =>
    {
        // Add logic to be executed before each retry, such as logging
    });

// Wait and retry forever, calling an action on each retry with the
// current exception, time to wait, and context provided to Execute()
Policy
  .Handle<SomeExceptionType>()
  .WaitAndRetryForever(
    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
    (exception, timespan, context) =>
    {
        // Add logic to be executed before each retry, such as logging
    });
```

Similarly to `RetryForever`, `WaitAndRetryForever` only actually retries `int.MaxValue` times.

If all retries fail, a retry policy rethrows the final exception back to the calling code.

For more depth see also: [Retry policy documentation on wiki](https://github.com/App-vNext/Polly/wiki/Retry).

#### Circuit Breaker

```csharp
// Break the circuit after the specified number of consecutive exceptions
// and keep circuit broken for the specified duration.
Policy
    .Handle<SomeExceptionType>()
    .CircuitBreaker(2, TimeSpan.FromMinutes(1));

// Break the circuit after the specified number of consecutive exceptions
// and keep circuit broken for the specified duration,
// calling an action on change of circuit state.
Action<Exception, TimeSpan> onBreak = (exception, timespan) => { ... };
Action onReset = () => { ... };
CircuitBreakerPolicy breaker = Policy
    .Handle<SomeExceptionType>()
    .CircuitBreaker(2, TimeSpan.FromMinutes(1), onBreak, onReset);

// Break the circuit after the specified number of consecutive exceptions
// and keep circuit broken for the specified duration,
// calling an action on change of circuit state,
// passing a context provided to Execute().
Action<Exception, TimeSpan, Context> onBreak = (exception, timespan, context) => { ... };
Action<Context> onReset = context => { ... };
CircuitBreakerPolicy breaker = Policy
    .Handle<SomeExceptionType>()
    .CircuitBreaker(2, TimeSpan.FromMinutes(1), onBreak, onReset);

// Monitor the circuit state, for example for health reporting.
CircuitState state = breaker.CircuitState;

/*
CircuitState.Closed - Normal operation. Execution of actions allowed.
CircuitState.Open - The automated controller has opened the circuit. Execution of actions blocked.
CircuitState.HalfOpen - Recovering from open state, after the automated break duration has expired. Execution of actions permitted. Success of subsequent action/s controls onward transition to Open or Closed state.
CircuitState.Isolated - Circuit held manually in an open state. Execution of actions blocked.
*/

// Manually open (and hold open) a circuit breaker - for example to manually isolate a downstream service.
breaker.Isolate();
// Reset the breaker to closed state, to start accepting actions again.
breaker.Reset();
```

Circuit-breaker policies block exceptions by throwing `BrokenCircuitException` when the circuit is broken. See: [Circuit-Breaker documentation on wiki](https://github.com/App-vNext/Polly/wiki/Circuit-Breaker).

Note that circuit-breaker policies [rethrow all exceptions](https://github.com/App-vNext/Polly/wiki/Circuit-Breaker#exception-handling), even handled ones. A circuit-breaker exists to measure faults and break the circuit when too many faults occur, but does not orchestrate retries.  Combine a circuit-breaker with a retry policy as needed.

#### Advanced Circuit Breaker

```csharp
// Break the circuit if, within any period of duration samplingDuration,
// the proportion of actions resulting in a handled exception exceeds failureThreshold,
// provided also that the number of actions through the circuit in the period
// is at least minimumThroughput.

Policy
    .Handle<SomeExceptionType>()
    .AdvancedCircuitBreaker(
        failureThreshold: 0.5, // Break on >=50% actions result in handled exceptions...
        samplingDuration: TimeSpan.FromSeconds(10), // ... over any 10 second period
        minimumThroughput: 8, // ... provided at least 8 actions in the 10 second period.
        durationOfBreak: TimeSpan.FromSeconds(30) // Break for 30 seconds.
                );

// Configuration overloads taking state-change delegates are
// available as described for CircuitBreaker above.

// Circuit state monitoring and manual controls are
// available as described for CircuitBreaker above.
```

For more detail see: [Advanced Circuit-Breaker documentation](https://github.com/App-vNext/Polly/wiki/Advanced-Circuit-Breaker) on wiki.

For more information on the Circuit Breaker pattern in general see:

+ [Making the Netflix API More Resilient](http://techblog.netflix.com/2011/12/making-netflix-api-more-resilient.html)
+ [Circuit Breaker (Martin Fowler)](http://martinfowler.com/bliki/CircuitBreaker.html)
+ [Circuit Breaker Pattern (Microsoft)](https://msdn.microsoft.com/en-us/library/dn589784.aspx)
+ [Original Circuit Breaking Link](https://web.archive.org/web/20160106203951/http://thatextramile.be/blog/2008/05/the-circuit-breaker)

#### Fallback

```csharp
// Provide a substitute value, if an execution faults.
Policy<UserAvatar>
   .Handle<FooException>()
   .OrResult(null)
   .Fallback<UserAvatar>(UserAvatar.Blank);

// Specify a func to provide a substitute value, if execution faults.
Policy<UserAvatar>
   .Handle<FooException>()
   .OrResult(null)
   .Fallback<UserAvatar>(() => UserAvatar.GetRandomAvatar()); // where: public UserAvatar GetRandomAvatar() { ... }

// Specify a substitute value or func, calling an action (eg for logging) if the fallback is invoked.
Policy<UserAvatar>
   .Handle<FooException>()
   .Fallback<UserAvatar>(UserAvatar.Blank, onFallback: (exception, context) =>
    {
        // Add extra logic to be called when the fallback is invoked, such as logging
    });
```

For more detail see: [Fallback policy documentation](https://github.com/App-vNext/Polly/wiki/Fallback) on wiki.

### Step 3 : Execute code through the policy

Execute an `Action`, `Func`, or lambda delegate equivalent, through the policy.  The policy governs execution of the code passed to the `.Execute()` (or similar) method.

> **Note**
>
> The code examples below show defining the policy and executing code through it in the same scope, for simplicity. See the notes after the code examples for other usage patterns.

```csharp
// Execute an action
var policy = Policy
              .Handle<SomeExceptionType>()
              .Retry();

policy.Execute(() => DoSomething());

// Execute an action passing arbitrary context data
var policy = Policy
    .Handle<SomeExceptionType>()
    .Retry(3, (exception, retryCount, context) =>
    {
        var methodThatRaisedException = context["methodName"];
        Log(exception, methodThatRaisedException);
    });

policy.Execute(
  () => DoSomething(),
  new Dictionary<string, object>() {{ "methodName", "some method" }}
);

// Execute a function returning a result
var policy = Policy
              .Handle<SomeExceptionType>()
              .Retry();

var result = policy.Execute(() => DoSomething());

// Execute a function returning a result passing arbitrary context data
var policy = Policy
    .Handle<SomeExceptionType>()
    .Retry(3, (exception, retryCount, context) =>
    {
        object methodThatRaisedException = context["methodName"];
        Log(exception, methodThatRaisedException)
    });

var result = policy.Execute(
    () => DoSomething(),
    new Dictionary<string, object>() {{ "methodName", "some method" }}
);

// You can of course chain it all together
Policy
  .Handle<SqlException>(ex => ex.Number == 1205)
  .Or<ArgumentException>(ex => ex.ParamName == "example")
  .Retry()
  .Execute(() => DoSomething());
```

#### Richer policy consumption patterns

Defining and consuming the policy in the same scope, as shown above, is the most immediate way to use Polly.  Consider also:

+ Separate policy definition from policy consumption, and inject policies into the code which will consume them. This [enables many unit-testing scenarios](https://github.com/App-vNext/Polly/wiki/Unit-testing-with-Polly---with-examples).
+ If your application uses Polly in a number of locations, define all policies at start-up, and place them in a [`PolicyRegistry`](https://github.com/App-vNext/Polly/wiki/PolicyRegistry).  This is a common pattern in .NET Core applications.  For instance, you might define your own extension method on `IServiceCollection` to configure the policies you will consume elsewhere in the application.  PolicyRegistry also [combines well with DI to support unit-testing](https://github.com/App-vNext/Polly/wiki/Unit-testing-with-Polly---with-examples#use-policyregistry-with-di-to-work-with-policy-collections).

```csharp
public static ConfigurePollyPolicies(this IServiceCollection services)
{
    PolicyRegistry registry = new PolicyRegistry()
    {
        { "RepositoryResilienceStrategy", /* define Policy or PolicyWrap strategy */ },
        { "CollaboratingMicroserviceResilienceStrategy", /* define Policy or PolicyWrap strategy */ },
        { "ThirdPartyApiResilienceStrategy", /* define Policy or PolicyWrap strategy */ },
        /* etc */
    };

    services.AddSingleton<IReadOnlyPolicyRegistry<string>>(registry);
}
```

## Usage &ndash; proactive policies

The proactive policies add resilience strategies that are not based on handling faults which the governed code may throw or return.

### Step 1 : Configure

#### Optimistic timeout

Optimistic timeout [operates via CancellationToken](https://github.com/App-vNext/Polly/wiki/Timeout#optimistic-timeout) and assumes delegates you execute support [co-operative cancellation](https://msdn.microsoft.com/en-us/library/dd997364.aspx).  You must use `Execute/Async(...)` overloads taking a `CancellationToken`, and the executed delegate must honor that `CancellationToken`.

```csharp
// Timeout and return to the caller after 30 seconds, if the executed delegate has not completed.  Optimistic timeout: Delegates should take and honour a CancellationToken.
Policy
  .Timeout(30)

// Configure timeout as timespan.
Policy
  .Timeout(TimeSpan.FromMilliseconds(2500))

// Configure variable timeout via a func provider.
Policy
  .Timeout(myTimeoutProvider) // Func<TimeSpan> myTimeoutProvider

// Timeout, calling an action if the action times out
Policy
  .Timeout(30, onTimeout: (context, timespan, task) =>
    {
        // Add extra logic to be invoked when a timeout occurs, such as logging
    });

// Eg timeout, logging that the execution timed out:
Policy
  .Timeout(30, onTimeout: (context, timespan, task) =>
    {
        logger.Warn($"{context.PolicyKey} at {context.OperationKey}: execution timed out after {timespan.TotalSeconds} seconds.");
    });

// Eg timeout, capturing any exception from the timed-out task when it completes:
Policy
  .Timeout(30, onTimeout: (context, timespan, task) =>
    {
        task.ContinueWith(t => {
            if (t.IsFaulted) logger.Error($"{context.PolicyKey} at {context.OperationKey}: execution timed out after {timespan.TotalSeconds} seconds, with: {t.Exception}.");
        });
    });
```

Example execution:

```csharp
Policy timeoutPolicy = Policy.TimeoutAsync(30);
HttpResponseMessage httpResponse = await timeoutPolicy
    .ExecuteAsync(
      async ct => await httpClient.GetAsync(endpoint, ct), // Execute a delegate which responds to a CancellationToken input parameter.
      CancellationToken.None // In this case, CancellationToken.None is passed into the execution, indicating you have no independent cancellation control you wish to add to the cancellation provided by TimeoutPolicy.  Your own independent CancellationToken can also be passed - see wiki for examples.
      );
```

Timeout policies throw `TimeoutRejectedException` when a timeout occurs.

For more detail see [Timeout policy documentation](https://github.com/App-vNext/Polly/wiki/Timeout) in the wiki.

#### Pessimistic timeout

Pessimistic timeout allows calling code to 'walk away' from waiting for an executed delegate to complete, even if it does not support cancellation.  In synchronous executions this is at the expense of an extra thread; see [deep documentation on wiki](https://github.com/App-vNext/Polly/wiki/Timeout#pessimistic-timeout) for more detail.

```csharp
// Timeout after 30 seconds, if the executed delegate has not completed.  Enforces this timeout even if the executed code has no cancellation mechanism.
Policy
  .Timeout(30, TimeoutStrategy.Pessimistic)

// (All syntax variants outlined for optimistic timeout above also exist for pessimistic timeout.)
```

Example execution:

```csharp
Policy timeoutPolicy = Policy.TimeoutAsync(30, TimeoutStrategy.Pessimistic);
var response = await timeoutPolicy
    .ExecuteAsync(
      async () => await FooNotHonoringCancellationAsync(), // Execute a delegate which takes no CancellationToken and does not respond to cancellation.
      );
```

Timeout policies throw `TimeoutRejectedException` when timeout occurs.

For more detail see: [Timeout policy documentation](https://github.com/App-vNext/Polly/wiki/Timeout) on wiki.

#### Bulkhead

```csharp
// Restrict executions through the policy to a maximum of twelve concurrent actions.
Policy
  .Bulkhead(12)

// Restrict executions through the policy to a maximum of twelve concurrent actions,
// with up to two actions waiting for an execution slot in the bulkhead if all slots are taken.
Policy
  .Bulkhead(12, 2)

// Restrict concurrent executions, calling an action if an execution is rejected
Policy
  .Bulkhead(12, context =>
    {
        // Add callback logic for when the bulkhead rejects execution, such as logging
    });

// Monitor the bulkhead available capacity, for example for health/load reporting.
var bulkhead = Policy.Bulkhead(12, 2);
// ...
int freeExecutionSlots = bulkhead.BulkheadAvailableCount;
int freeQueueSlots     = bulkhead.QueueAvailableCount;

```

Bulkhead policies throw `BulkheadRejectedException` if items are queued to the bulkhead when the bulkhead execution and queue are both full.

For more detail see: [Bulkhead policy documentation](https://github.com/App-vNext/Polly/wiki/Bulkhead) on wiki.

#### Rate-Limit

```csharp
// Allow up to 20 executions per second.
Policy.RateLimit(20, TimeSpan.FromSeconds(1));

// Allow up to 20 executions per second with a burst of 10 executions.
Policy.RateLimit(20, TimeSpan.FromSeconds(1), 10);

// Allow up to 20 executions per second, with a delegate to return the
// retry-after value to use if the rate limit is exceeded.
Policy.RateLimit(20, TimeSpan.FromSeconds(1), (retryAfter, context) =>
{
    return retryAfter.Add(TimeSpan.FromSeconds(2));
});

// Allow up to 20 executions per second with a burst of 10 executions,
// with a delegate to return the retry-after value to use if the rate
// limit is exceeded.
Policy.RateLimit(20, TimeSpan.FromSeconds(1), 10, (retryAfter, context) =>
{
    return retryAfter.Add(TimeSpan.FromSeconds(2));
});
```

Example execution:

```csharp
public async Task SearchAsync(string query, HttpContext httpContext)
{
    var rateLimit = Policy.RateLimitAsync(20, TimeSpan.FromSeconds(1), 10);

    try
    {
        var result = await rateLimit.ExecuteAsync(() => TextSearchAsync(query));

        var json = JsonConvert.SerializeObject(result);

        httpContext.Response.ContentType = "application/json";
        await httpContext.Response.WriteAsync(json);
    }
    catch (RateLimitRejectedException ex)
    {
        string retryAfter = DateTimeOffset.UtcNow
            .Add(ex.RetryAfter)
            .ToUnixTimeSeconds()
            .ToString(CultureInfo.InvariantCulture);

        httpContext.Response.StatusCode = 429;
        httpContext.Response.Headers["Retry-After"] = retryAfter;
    }
}
```

Rate-limit policies throw `RateLimitRejectedException` if too many requests are executed within the configured timespan.

For more detail see: [Rate-limit policy documentation](https://github.com/App-vNext/Polly/wiki/Rate-Limit) in the wiki.

#### Cache

```csharp
var memoryCache = new MemoryCache(new MemoryCacheOptions());
var memoryCacheProvider = new MemoryCacheProvider(memoryCache);
var cachePolicy = Policy.Cache(memoryCacheProvider, TimeSpan.FromMinutes(5));

// For .NET Core DI examples see the CacheProviders linked to from https://github.com/App-vNext/Polly/wiki/Cache#working-with-cacheproviders :
// - https://github.com/App-vNext/Polly.Caching.MemoryCache
// - https://github.com/App-vNext/Polly.Caching.IDistributedCache

// Define a cache policy with absolute expiration at midnight tonight.
var cachePolicy = Policy.Cache(memoryCacheProvider, new AbsoluteTtl(DateTimeOffset.Now.Date.AddDays(1)));

// Define a cache policy with sliding expiration: items remain valid for another 5 minutes each time the cache item is used.
var cachePolicy = Policy.Cache(memoryCacheProvider, new SlidingTtl(TimeSpan.FromMinutes(5)));

// Define a cache Policy, and catch any cache provider errors for logging.
var cachePolicy = Policy.Cache(myCacheProvider, TimeSpan.FromMinutes(5),
   (context, key, ex) => {
       logger.Error($"Cache provider, for key {key}, threw exception: {ex}."); // (for example)
   }
);

// Execute through the cache as a read-through cache: check the cache first; if not found, execute underlying delegate and store the result in the cache.
// The key to use for caching, for a particular execution, is specified by setting the OperationKey (before v6: ExecutionKey) on a Context instance passed to the execution. Use an overload of the form shown below (or a richer overload including the same elements).
// Example: "FooKey" is the cache key that will be used in the below execution.
TResult result = cachePolicy.Execute(context => getFoo(), new Context("FooKey"));
```

For richer options and details of using further cache providers see: [Cache policy documentation](https://github.com/App-vNext/Polly/wiki/Cache) on wiki.

#### PolicyWrap

```csharp
// Define a combined policy strategy, built of previously-defined policies.
var policyWrap = Policy
  .Wrap(fallback, cache, retry, breaker, timeout, bulkhead);
// (wraps the policies around any executed delegate: fallback outermost ... bulkhead innermost)
policyWrap.Execute(...)

// Define a standard resilience strategy ...
PolicyWrap commonResilience = Policy.Wrap(retry, breaker, timeout);

// ... then wrap in extra policies specific to a call site, at that call site:
Avatar avatar = Policy<UserAvatar>
   .Handle<FooException>()
   .Fallback<Avatar>(Avatar.Blank)
   .Wrap(commonResilience)
   .Execute(() => { /* get avatar */ });

// Share commonResilience, but wrap different policies in at another call site:
Reputation reps = Policy
   .Handle<FooException>()
   .Fallback<Reputation>(Reputation.NotAvailable)
   .Wrap(commonResilience)
   .Execute(() => { /* get reputation */ });

```

For more detail see: [PolicyWrap documentation](https://github.com/App-vNext/Polly/wiki/PolicyWrap) on wiki.

#### NoOp

```csharp
// Define a policy which will simply cause delegates passed for execution to be executed 'as is'.
// This is useful for stubbing-out Polly in unit tests,
// or in application situations where your code architecture might expect a policy
// but you simply want to pass the execution through without policy intervention.
NoOpPolicy noOp = Policy.NoOp();
```

For more detail see: [NoOp documentation](https://github.com/App-vNext/Polly/wiki/NoOp) on wiki.

### Step 2 : Execute the policy

As for fault-handling policies [above](#step-3--execute-code-through-the-policy).

## Usage – results and exceptions

### Getting execution results as a PolicyResult

Using the `ExecuteAndCapture(...)` methods you can capture the outcome of an execution: the methods return a `PolicyResult` instance which describes whether the outcome was a successful execution or a fault.

```csharp
var policyResult = await Policy
              .Handle<HttpRequestException>()
              .RetryAsync()
              .ExecuteAndCaptureAsync(() => DoSomethingAsync());
/*
policyResult.Outcome - whether the call succeeded or failed
policyResult.FinalException - the final exception captured, will be null if the call succeeded
policyResult.ExceptionType - was the final exception an exception the policy was defined to handle (like HttpRequestException above) or an unhandled one (say Exception). Will be null if the call succeeded.
policyResult.Result - if executing a func, the result if the call succeeded or the type's default value
*/
```

### Getting execution results and return values with a HttpResponseMessage

As described at step 1b, from Polly v4.3.0 onwards, policies can handle return values and exceptions in combination:

```csharp
// Handle both exceptions and return values in one policy
HttpStatusCode[] httpStatusCodesWorthRetrying = {
   HttpStatusCode.RequestTimeout, // 408
   HttpStatusCode.InternalServerError, // 500
   HttpStatusCode.BadGateway, // 502
   HttpStatusCode.ServiceUnavailable, // 503
   HttpStatusCode.GatewayTimeout // 504
};
HttpResponseMessage result = await Policy
  .Handle<HttpRequestException>()
  .OrResult<HttpResponseMessage>(r => httpStatusCodesWorthRetrying.Contains(r.StatusCode))
  .RetryAsync(...)
  .ExecuteAsync( /* some Func<Task<HttpResponseMessage>> */ )
```

The exceptions and return results to handle can be expressed fluently in any order.

### Getting execution results and return values with a Policy&lt;TResult&gt;

Configuring a policy with `.HandleResult<TResult>(...)` or `.OrResult<TResult>(...)` generates a strongly-typed `Policy<TResult>` of the specific policy type, eg `Retry<TResult>`, `AdvancedCircuitBreaker<TResult>`.

These policies must be used to execute delegates returning `TResult`, i.e.:

+ `Execute(Func<TResult>)` (and related overloads)
+ `ExecuteAsync(Func<CancellationToken, Task<TResult>>)` (and related overloads)

### Getting strongly-typed results with ExecuteAndCapture&lt;TResult&gt;()

`.ExecuteAndCapture(...)` on non-generic policies returns a `PolicyResult` with properties:

```txt
policyResult.Outcome - whether the call succeeded or failed
policyResult.FinalException - the final exception captured; will be null if the call succeeded
policyResult.ExceptionType - was the final exception an exception the policy was defined to handle (like HttpRequestException above) or an unhandled one (say Exception)? Will be null if the call succeeded.
policyResult.Result - if executing a func, the result if the call succeeded; otherwise, the type's default value
```

`.ExecuteAndCapture<TResult>(Func<TResult>)` on strongly-typed policies adds two properties:

```txt
policyResult.FaultType - was the final fault handled an exception or a result handled by the policy? Will be null if the delegate execution succeeded.
policyResult.FinalHandledResult - the final fault result handled; will be null or the type's default value, if the call succeeded
```

### State-change delegates on Policy&lt;TResult&gt; policies

In non-generic policies handling only exceptions, state-change delegates such as `onRetry` and `onBreak` take an `Exception` parameter.

In generic-policies handling `TResult` return values, state-change delegates are identical except they take a `DelegateResult<TResult>` parameter in place of `Exception.` `DelegateResult<TResult>` has two properties:

+ `Exception // The exception just thrown if policy is in process of handling an exception (otherwise null)`
+ `Result // The TResult just raised, if policy is in process of handling a result (otherwise default(TResult))`

### BrokenCircuitException&lt;TResult&gt;

Non-generic CircuitBreaker policies throw a `BrokenCircuitException` when the circuit is broken.  This `BrokenCircuitException` contains the last exception (the one which caused the circuit to break) as the `InnerException`.

For `CircuitBreakerPolicy<TResult>` policies:

+ A circuit broken due to an exception throws a `BrokenCircuitException` with `InnerException` set to the exception which triggered the break (as previously).
+ A circuit broken due to handling a result throws a `BrokenCircuitException<TResult>` with the `Result` property set to the result which caused the circuit to break.

## Policy Keys and Context data

```csharp
// Identify policies with a PolicyKey, using the WithPolicyKey() extension method
// (for example, for correlation in logs or metrics)

var policy = Policy
    .Handle<DataAccessException>()
    .Retry(3, onRetry: (exception, retryCount, context) =>
       {
           logger.Error($"Retry {retryCount} of {context.PolicyKey} at {context.OperationKey}, due to: {exception}.");
       })
    .WithPolicyKey("MyDataAccessPolicy");

// Identify call sites with an OperationKey, by passing in a Context
var customerDetails = policy.Execute(myDelegate, new Context("GetCustomerDetails"));

// "MyDataAccessPolicy" -> context.PolicyKey
// "GetCustomerDetails  -> context.OperationKey


// Pass additional custom information from call site into execution context
var policy = Policy
    .Handle<DataAccessException>()
    .Retry(3, onRetry: (exception, retryCount, context) =>
       {
           logger.Error($"Retry {retryCount} of {context.PolicyKey} at {context.OperationKey}, getting {context["Type"]} of id {context["Id"]}, due to: {exception}.");
       })
    .WithPolicyKey("MyDataAccessPolicy");

int id = ... // customer id from somewhere
var customerDetails = policy.Execute(context => GetCustomer(id),
    new Context("GetCustomerDetails", new Dictionary<string, object>() {{"Type","Customer"},{"Id",id}}
    ));
```

For more detail see: [Keys and Context Data](https://github.com/App-vNext/Polly/wiki/Keys-And-Context-Data) on wiki.

## PolicyRegistry

```csharp
// Create a policy registry (for example on application start-up)
PolicyRegistry registry = new PolicyRegistry();

// Populate the registry with policies
registry.Add("StandardHttpResilience", myStandardHttpResiliencePolicy);
// Or:
registry["StandardHttpResilience"] = myStandardHttpResiliencePolicy;

// Pass the registry instance to usage sites by DI, perhaps
public class MyServiceGateway
{
    public void MyServiceGateway(..., IReadOnlyPolicyRegistry<string> registry, ...)
    {
       ...
    }
}
// (Or if you prefer ambient-context pattern, use a thread-safe singleton)

// Use a policy from the registry
registry.Get<IAsyncPolicy<HttpResponseMessage>>("StandardHttpResilience")
    .ExecuteAsync<HttpResponseMessage>(...)
```

`PolicyRegistry` has a range of further dictionary-like semantics such as `.ContainsKey(...)`, `.TryGet<TPolicy>(...)`, `.Count`, `.Clear()`, and `Remove(...)`.

Available from v5.2.0.  For more detail see: [PolicyRegistry](https://github.com/App-vNext/Polly/wiki/PolicyRegistry) on wiki.

## Asynchronous Support

Polly fully supports asynchronous executions, using the asynchronous methods:

+ `RetryAsync`
+ `WaitAndRetryAsync`
+ `CircuitBreakerAsync`
+ (etc)
+ `ExecuteAsync`
+ `ExecuteAndCaptureAsync`

In place of their synchronous counterparts:

+ `Retry`
+ `WaitAndRetry`
+ `CircuitBreaker`
+ (etc)
+ `Execute`
+ `ExecuteAndCapture`

Async overloads exist for all policy types and for all `Execute()` and `ExecuteAndCapture()` overloads.

Usage example:

```csharp
await Policy
  .Handle<SqlException>(ex => ex.Number == 1205)
  .Or<ArgumentException>(ex => ex.ParamName == "example")
  .RetryAsync()
  .ExecuteAsync(() => DoSomethingAsync());

```

### SynchronizationContext

Async continuations and retries by default do not run on a captured synchronization context. To change this, use `.ExecuteAsync(...)` overloads taking a boolean `continueOnCapturedContext` parameter.

### Cancellation support

Async policy execution supports cancellation via `.ExecuteAsync(...)` overloads taking a `CancellationToken`.

The token you pass as the `cancellationToken` parameter to the `ExecuteAsync(...)` call serves three purposes:

+ It cancels Policy actions such as further retries, waits between retries or waits for a bulkhead execution slot.
+ It is passed by the policy as the `CancellationToken` input parameter to any delegate executed through the policy, to support cancellation during delegate execution.
+ In common with the Base Class Library implementation in `Task.Run(…)` and elsewhere, if the cancellation token is cancelled before execution begins, the user delegate is not executed at all.

```csharp
// Try several times to retrieve from a URI, but support cancellation at any time.
CancellationToken cancellationToken = // ...
var policy = Policy
    .Handle<HttpRequestException>()
    .WaitAndRetryAsync(new[] {
        TimeSpan.FromSeconds(1),
        TimeSpan.FromSeconds(2),
        TimeSpan.FromSeconds(4)
    });
var response = await policy.ExecuteAsync(ct => httpClient.GetAsync(uri, ct), cancellationToken);
```

From Polly v5.0, synchronous executions also support cancellation via `CancellationToken`.

## Thread safety

All Polly policies are fully thread-safe.  You can safely re-use policies at multiple call sites, and execute through policies concurrently on different threads.

While the internal operation of the policy is thread-safe, this does not magically make delegates you execute through the policy thread-safe: if delegates you execute through the policy are not thread-safe, they remain not thread-safe.

## Interfaces

Polly v5.2.0 adds interfaces intended to support [`PolicyRegistry`](https://github.com/App-vNext/Polly/wiki/PolicyRegistry) and to group Policy functionality by the [interface segregation principle](https://en.wikipedia.org/wiki/Interface_segregation_principle).  Polly's interfaces are not intended for coding your own policy implementations against.

### Execution interfaces: `ISyncPolicy` etc

Execution interfaces [`ISyncPolicy`](https://github.com/App-vNext/Polly/tree/main/src/Polly/ISyncPolicy.cs), [`IAsyncPolicy`](https://github.com/App-vNext/Polly/tree/main/src/Polly/IAsyncPolicy.cs), [`ISyncPolicy<TResult>`](https://github.com/App-vNext/Polly/tree/main/src/Polly/ISyncPolicyTResult.cs) and [`IAsyncPolicy<TResult>`](https://github.com/App-vNext/Polly/tree/main/src/Polly/IAsyncPolicyTResult.cs)  define the execution overloads available to policies targeting sync/async, and non-generic / generic calls respectively.

### Policy-kind interfaces: `ICircuitBreakerPolicy` etc

Orthogonal to the execution interfaces, interfaces specific to the kind of Policy define properties and methods common to that type of policy.

For example, [`ICircuitBreakerPolicy`](https://github.com/App-vNext/Polly/tree/main/src/Polly/CircuitBreaker/ICircuitBreakerPolicy.cs) defines

+ `CircuitState CircuitState`
+ `Exception LastException`
+ `void Isolate()`
+ `void Reset()`

with `ICircuitBreakerPolicy<TResult> : ICircuitBreakerPolicy` adding:

+ `TResult LastHandledResult`.

This allows collections of similar kinds of policy to be treated as one - for example, for monitoring all your circuit-breakers as [described here](https://github.com/App-vNext/Polly/pull/205).

For more detail see: [Polly and interfaces](https://github.com/App-vNext/Polly/wiki/Polly-and-interfaces) on wiki.

## Sample Projects

- [Polly-Samples](https://github.com/App-vNext/Polly-Samples) contains practical examples for using various implementations of Polly. Please feel free to contribute to the Polly-Samples repository in order to assist others who are either learning Polly for the first time, or are seeking advanced examples and novel approaches provided by our generous community.
- Microsoft's [eShopOnContainers project](https://github.com/dotnet-architecture/eShopOnContainers) is a sample project demonstrating a .NET Microservices architecture and using Polly for resilience

## License

Licensed under the terms of the [New BSD License](http://opensource.org/licenses/BSD-3-Clause)

## Resources

Visit the [documentation](docs/README.md) to explore more Polly-related resources.
