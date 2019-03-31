# Polly

Polly is a .NET resilience and transient-fault-handling library that allows developers to express policies such as Retry, Circuit Breaker, Timeout, Bulkhead Isolation, and Fallback in a fluent and thread-safe manner.  

Polly targets .NET Standard 1.1 ([coverage](https://docs.microsoft.com/en-us/dotnet/standard/net-standard#net-implementation-support): .NET Framework 4.5-4.6.1, .NET Core 1.0, Mono, Xamarin, UWP, WP8.1+) and .NET Standard 2.0+  ([coverage](https://docs.microsoft.com/en-us/dotnet/standard/net-standard#net-implementation-support): .NET Framework 4.6.1, .NET Core 2.0+, and later Mono, Xamarin and UWP targets).

For versions supporting earlier targets such as .NET4.0 and .NET3.5, see the [supported targets](https://github.com/App-vNext/Polly/wiki/Supported-targets) grid.

[<img align="right" src="https://github.com/dotnet/swag/raw/master/logo/dotnetfoundation_v4_small.png" width="100" />](https://www.dotnetfoundation.org/)
We are a member of the [.NET Foundation](https://www.dotnetfoundation.org/about)!

**Keep up to date with new feature announcements, tips & tricks, and other news through [www.thepollyproject.org](http://www.thepollyproject.org)**

[![NuGet version](https://badge.fury.io/nu/polly.svg)](https://badge.fury.io/nu/polly) [![Build status](https://ci.appveyor.com/api/projects/status/imt7dymt50346k5u?svg=true)](https://ci.appveyor.com/project/joelhulen/polly) [![Slack Status](http://www.pollytalk.org/badge.svg)](http://www.pollytalk.org)

![](https://raw.github.com/App-vNext/Polly/master/Polly-Logo.png)

# Installing via NuGet

    Install-Package Polly


# Resilience policies

Polly offers multiple resilience policies:

|Policy| Premise | Aka| How does the policy mitigate?|
| ------------- | ------------- |:-------------: |------------- |
|**Retry** <br/>(policy family)<br/><sub>([quickstart](#retry)&nbsp;;&nbsp;[deep](https://github.com/App-vNext/Polly/wiki/Retry))</sub>|Many faults are transient and may self-correct after a short delay.| "Maybe it's just a blip" |  Allows configuring automatic retries. |
|**Circuit-breaker**<br/>(policy family)<br/><sub>([quickstart](#circuit-breaker)&nbsp;;&nbsp;[deep](https://github.com/App-vNext/Polly/wiki/Circuit-Breaker))</sub>|When a system is seriously struggling, failing fast is better than making users/callers wait.  <br/><br/>Protecting a faulting system from overload can help it recover. | "Stop doing it if it hurts" <br/><br/>"Give that system a break" | Breaks the circuit (blocks executions) for a period, when faults exceed some pre-configured threshold. | 
|**Timeout**<br/><sub>([quickstart](#timeout)&nbsp;;&nbsp;[deep](https://github.com/App-vNext/Polly/wiki/Timeout))</sub>|Beyond a certain wait, a success result is unlikely.| "Don't wait forever"  |Guarantees the caller won't have to wait beyond the timeout. | 
|**Bulkhead Isolation**<br/><sub>([quickstart](#bulkhead)&nbsp;;&nbsp;[deep](https://github.com/App-vNext/Polly/wiki/Bulkhead))</sub>|When a process faults, multiple failing calls backing up can easily swamp resource (eg threads/CPU) in a host.<br/><br/>A faulting downstream system can also cause 'backed-up' failing calls upstream.<br/><br/>Both risk a faulting process bringing down a wider system. | "One fault shouldn't sink the whole ship"  |Constrains the governed actions to a fixed-size resource pool, isolating their potential  to affect others. |
|**Cache**<br/><sub>([quickstart](#cache)&nbsp;;&nbsp;[deep](https://github.com/App-vNext/Polly/wiki/Cache))</sub>|Some proportion of requests may be similar.| "You've asked that one before"  |Provides a response from cache if known. <br/><br/>Stores responses automatically in cache, when first retrieved. |
|**Fallback**<br/><sub>([quickstart](#fallback)&nbsp;;&nbsp;[deep](https://github.com/App-vNext/Polly/wiki/Fallback))</sub>|Things will still fail - plan what you will do when that happens.| "Degrade gracefully"  |Defines an alternative value to be returned (or action to be executed) on failure. | 
|**PolicyWrap**<br/><sub>([quickstart](#policywrap)&nbsp;;&nbsp;[deep](https://github.com/App-vNext/Polly/wiki/PolicyWrap))</sub>|Different faults require different strategies; resilience means using a combination.| "Defence in depth"  |Allows any of the above policies to be combined flexibly. | 

In addition to the detailed pages on each policy, an [introduction to the role of each policy in resilience engineering](https://github.com/App-vNext/Polly/wiki/Transient-fault-handling-and-proactive-resilience-engineering)  is also provided in the wiki.

### Using Polly with HttpClient factory from ASPNET Core 2.1

For using Polly with  HttpClient factory from ASPNET Core 2.1, see our [detailed wiki page](https://github.com/App-vNext/Polly/wiki/Polly-and-HttpClientFactory), then come back here or [explore the wiki](https://github.com/App-vNext/Polly/wiki) to learn more about the operation of each policy.

### Release notes

+ The [change log](https://github.com/App-vNext/Polly/blob/master/CHANGELOG.md) describes changes by release.
+ We tag Pull Requests and Issues with [milestones](https://github.com/App-vNext/Polly/milestones) which match to nuget package release numbers.
+ Breaking changes are called out in the wiki ([v7](https://github.com/App-vNext/Polly/wiki/Polly-v7-breaking-changes) ; [v6](https://github.com/App-vNext/Polly/wiki/Polly-v6-breaking-changes)) with simple notes on any necessary steps to upgrade.

### Supported targets

For details of supported compilation targets by version, see the [supported targets](https://github.com/App-vNext/Polly/wiki/Supported-targets) grid.

### Role of the readme and the wiki

This ReadMe aims to give a quick overview of all Polly features - including enough to get you started with any policy.  For deeper detail on any policy, and many other aspects of Polly, be sure also to check out the [wiki documentation](https://github.com/App-vNext/Polly/wiki).

# Usage &ndash; fault-handling, reactive policies

Fault-handling policies handle specific exceptions thrown by, or results returned by, the delegates you execute through the policy.

## Step 1 : Specify the  exceptions/faults you want the policy to handle 

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

## Step 1b: (optionally) Specify return results you want to handle

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

## Step 2 : Specify how the policy should handle those faults

### Retry 

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
        // do something 
    });

// Retry multiple times, calling an action on each retry 
// with the current exception, retry count and context 
// provided to Execute()
Policy
    .Handle<SomeExceptionType>()
    .Retry(3, onRetry: (exception, retryCount, context) =>
    {
        // do something 
    });
```

### Retry forever (until succeeds)

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
        // do something       
  });

// Retry forever, calling an action on each retry with the
// current exception and context provided to Execute()
Policy
  .Handle<SomeExceptionType>()
  .RetryForever(onRetry: (exception, context) =>
  {
        // do something       
  });
```

### Wait and retry 

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
    // do something    
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
    // do something    
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
    // do something    
  });

// Retry a specified number of times, using a function to 
// calculate the duration to wait between retries based on 
// the current retry attempt (allows for exponential backoff)
// In this case will wait for
//  2 ^ 1 = 2 seconds then
//  2 ^ 2 = 4 seconds then
//  2 ^ 3 = 8 seconds then
//  2 ^ 4 = 16 seconds then
//  2 ^ 5 = 32 seconds
Policy
  .Handle<SomeExceptionType>()
  .WaitAndRetry(5, retryAttempt => 
	TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) 
  );

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
      // do something
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
      // do something
    }
  );
```

For `WaitAndRetry` policies handling Http Status Code 429 Retry-After, see [wiki documentation](https://github.com/App-vNext/Polly/wiki/Retry#retryafter-when-the-response-specifies-how-long-to-wait).

### Wait and retry forever (until succeeds) 

```csharp

// Wait and retry forever
Policy
  .Handle<SomeExceptionType>()
  .WaitAndRetryForever(retryAttempt => 
	TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
    );

// Wait and retry forever, calling an action on each retry with the 
// current exception and the time to wait
Policy
  .Handle<SomeExceptionType>()
  .WaitAndRetryForever(
    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),    
    (exception, timespan) =>
    {
        // do something       
    });

// Wait and retry forever, calling an action on each retry with the
// current exception, time to wait, and context provided to Execute()
Policy
  .Handle<SomeExceptionType>()
  .WaitAndRetryForever(
    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),    
    (exception, timespan, context) =>
    {
        // do something       
    });
```

If all retries fail, a retry policy rethrows the final exception back to the calling code.

For more depth see also: [Retry policy documentation on wiki](https://github.com/App-vNext/Polly/wiki/Retry).

### Circuit Breaker
 
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



### Advanced Circuit Breaker 
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
* [Making the Netflix API More Resilient](http://techblog.netflix.com/2011/12/making-netflix-api-more-resilient.html)
* [Circuit Breaker (Martin Fowler)](http://martinfowler.com/bliki/CircuitBreaker.html)
* [Circuit Breaker Pattern (Microsoft)](https://msdn.microsoft.com/en-us/library/dn589784.aspx)
* [Original Circuit Breaking Link](https://web.archive.org/web/20160106203951/http://thatextramile.be/blog/2008/05/the-circuit-breaker)


### Fallback 
```csharp
// Provide a substitute value, if an execution faults.
Policy
   .Handle<Whatever>()
   .Fallback<UserAvatar>(UserAvatar.Blank)

// Specify a func to provide a substitute value, if execution faults.
Policy
   .Handle<Whatever>()
   .Fallback<UserAvatar>(() => UserAvatar.GetRandomAvatar()) // where: public UserAvatar GetRandomAvatar() { ... }

// Specify a substitute value or func, calling an action (eg for logging) if the fallback is invoked.
Policy
   .Handle<Whatever>()
   .Fallback<UserAvatar>(UserAvatar.Blank, onFallback: (exception, context) => 
    {
        // do something
    });

```

For more detail see: [Fallback policy documentation](https://github.com/App-vNext/Polly/wiki/Fallback) on wiki.

## Step 3 : Execute the policy

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

The above examples show policy definition immediately followed by policy execution, for simplicity.  Policy definition and execution may just as often be separated in the codebase and application lifecycle.  You may choose for example to define policies on start-up, then provide them to point-of-use by dependency injection (perhaps using [`PolicyRegistry`](#policyregistry)).

# Usage &ndash; proactive policies

The proactive policies add resilience strategies that not based on handling faults which delegates may throw or return.

## Step 1 : Configure

### Timeout

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
  .Timeout(() => myTimeoutProvider)) // Func<TimeSpan> myTimeoutProvider

// Timeout, calling an action if the action times out
Policy
  .Timeout(30, onTimeout: (context, timespan, task) => 
    {
        // do something 
    });

// Eg timeout, logging that the execution timed out:
Policy
  .Timeout(30, onTimeout: (context, timespan, task) => 
    {
        logger.Warn($"{context.PolicyKey} at {context.ExecutionKey}: execution timed out after {timespan.TotalSeconds} seconds.");
    });

// Eg timeout, capturing any exception from the timed-out task when it completes:
Policy
  .Timeout(30, onTimeout: (context, timespan, task) => 
    {
        task.ContinueWith(t => {
            if (t.IsFaulted) logger.Error($"{context.PolicyKey} at {context.ExecutionKey}: execution timed out after {timespan.TotalSeconds} seconds, with: {t.Exception}.");
        });
    });
```

Example execution:

```csharp
Policy timeoutPolicy = Policy.TimeoutAsync(30);
HttpResponseMessage httpResponse = await timeoutPolicy
    .ExecuteAsync(
      async ct => await httpClient.GetAsync(endpoint, ct), // Execute a delegate which responds to a CancellationToken input parameter.
      CancellationToken.None // In this case, CancellationToken.None is passed into the execution, indicating you have no independent cancellation control you wish to add to the cancellation provided by TimeoutPolicy.  Your own indepdent CancellationToken can also be passed - see wiki for examples.
      );
```

#### Pessimistic timeout

Pessimistic timeout allows calling code to 'walk away' from waiting for an executed delegate to complete, even if it does not support cancellation.  In synchronous executions this is at the expense of an extra thread; see [deep doco on wiki](https://github.com/App-vNext/Polly/wiki/Timeout#pessimistic-timeout) for more detail.

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

### Bulkhead

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
        // do something 
    });

// Monitor the bulkhead available capacity, for example for health/load reporting.
var bulkhead = Policy.Bulkhead(12, 2);
// ...
int freeExecutionSlots = bulkhead.BulkheadAvailableCount;
int freeQueueSlots     = bulkhead.QueueAvailableCount;

```

Bulkhead policies throw `BulkheadRejectedException` if items are queued to the bulkhead when the bulkhead execution and queue are both full. 


For more detail see: [Bulkhead policy documentation](https://github.com/App-vNext/Polly/wiki/Bulkhead) on wiki.

### Cache

```csharp
var memoryCache = new MemoryCache(new MemoryCacheOptions());
var memoryCacheProvider = new MemoryCacheProvider(memoryCache);
var cachePolicy = Policy.Cache(memoryCacheProvider, TimeSpan.FromMinutes(5));

// For .NET Core DI examples see the CacheProviders linked to from https://github.com/App-vNext/Polly/wiki/Cache#working-with-cacheproviders :
// - https://github.com/App-vNext/Polly.Caching.MemoryCache
// - https://github.com/App-vNext/Polly.Caching.IDistributedCache 

// Define a cache policy with absolute expiration at midnight tonight.
var cachePolicy = Policy.Cache(memoryCacheProvider, new AbsoluteTtl(DateTimeOffset.Now.Date.AddDays(1));

// Define a cache policy with sliding expiration: items remain valid for another 5 minutes each time the cache item is used.
var cachePolicy = Policy.Cache(memoryCacheProvider, new SlidingTtl(TimeSpan.FromMinutes(5));

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

### PolicyWrap

```csharp
// Define a combined policy strategy, built of previously-defined policies.
var policyWrap = Policy
  .Wrap(fallback, cache, retry, breaker, timeout, bulkhead);
// (wraps the policies around any executed delegate: fallback outermost ... bulkhead innermost)
policyWrap.Execute(...)

// Define a standard resilience strategy ...
PolicyWrap commonResilience = Policy.Wrap(retry, breaker, timeout);

// ... then wrap in extra policies specific to a call site, at that call site:
Avatar avatar = Policy
   .Handle<Whatever>()
   .Fallback<Avatar>(Avatar.Blank)
   .Wrap(commonResilience)
   .Execute(() => { /* get avatar */ });

// Share commonResilience, but wrap different policies in at another call site:
Reputation reps = Policy
   .Handle<Whatever>()
   .Fallback<Reputation>(Reputation.NotAvailable)
   .Wrap(commonResilience)
   .Execute(() => { /* get reputation */ });  

```

For more detail see: [PolicyWrap documentation](https://github.com/App-vNext/Polly/wiki/PolicyWrap) on wiki.

### NoOp

```csharp
// Define a policy which will simply cause delegates passed for execution to be executed 'as is'.
// This is useful for stubbing-out Polly in unit tests, 
// or in application situations where your code architecture might expect a policy
// but you simply want to pass the execution through without policy intervention.
NoOpPolicy noOp = Policy.NoOp();
```
For more detail see: [NoOp documentation](https://github.com/App-vNext/Polly/wiki/NoOp) on wiki.

## Step 2 : Execute the policy

As for fault-handling policies [above](#step-3--execute-the-policy).

# Post-execution: capturing the result, or any final exception

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

# Handing return values, and Policy&lt;TResult&gt;

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

### Strongly-typed Policy&lt;TResult&gt;

Configuring a policy with `.HandleResult<TResult>(...)` or `.OrResult<TResult>(...)` generates a strongly-typed `Policy<TResult>` of the specific policy type, eg `Retry<TResult>`, `AdvancedCircuitBreaker<TResult>`.

These policies must be used to execute delegates returning `TResult`, ie:

* `Execute(Func<TResult>)` (and related overloads)
* `ExecuteAsync(Func<CancellationToken, Task<TResult>>)` (and related overloads)  

### ExecuteAndCapture&lt;TResult&gt;()

`.ExecuteAndCapture(...)` on non-generic policies returns a `PolicyResult` with properties:

```          
policyResult.Outcome - whether the call succeeded or failed         
policyResult.FinalException - the final exception captured; will be null if the call succeeded
policyResult.ExceptionType - was the final exception an exception the policy was defined to handle (like HttpRequestException above) or an unhandled one (say Exception)? Will be null if the call succeeded.
policyResult.Result - if executing a func, the result if the call succeeded; otherwise, the type's default value
```

`.ExecuteAndCapture<TResult>(Func<TResult>)` on strongly-typed policies adds two properties:

```
policyResult.FaultType - was the final fault handled an exception or a result handled by the policy? Will be null if the delegate execution succeeded. 
policyResult.FinalHandledResult - the final fault result handled; will be null or the type's default value, if the call succeeded
```

### State-change delegates on Policy&lt;TResult&gt; policies

In non-generic policies handling only exceptions, state-change delegates such as `onRetry` and `onBreak` take an `Exception` parameter.  

In generic-policies handling `TResult` return values, state-change delegates are identical except they take a `DelegateResult<TResult>` parameter in place of `Exception.` `DelegateResult<TResult>` has two properties:

* `Exception // The exception just thrown if policy is in process of handling an exception (otherwise null)`
* `Result // The TResult just raised, if policy is in process of handling a result (otherwise default(TResult))`
   

### BrokenCircuitException&lt;TResult&gt;

Non-generic CircuitBreaker policies throw a `BrokenCircuitException` when the circuit is broken.  This `BrokenCircuitException` contains the last exception (the one which caused the circuit to break) as the `InnerException`.

For `CircuitBreakerPolicy<TResult>` policies: 

* A circuit broken due to an exception throws a `BrokenCircuitException` with `InnerException` set to the exception which triggered the break (as previously).
* A circuit broken due to handling a result throws a `BrokenCircuitException<TResult>` with the `Result` property set to the result which caused the circuit to break.

# Policy Keys and Context data


```csharp
// Identify policies with a PolicyKey, using the WithPolicyKey() extension method
// (for example, for correlation in logs or metrics)

var policy = Policy
    .Handle<DataAccessException>()
    .Retry(3, onRetry: (exception, retryCount, context) =>
       {
           logger.Error($"Retry {retryCount} of {context.PolicyKey} at {context.ExecutionKey}, due to: {exception}.");
       })
    .WithPolicyKey("MyDataAccessPolicy");

// Identify call sites with an ExecutionKey, by passing in a Context
var customerDetails = policy.Execute(myDelegate, new Context("GetCustomerDetails"));

// "MyDataAccessPolicy" -> context.PolicyKey 
// "GetCustomerDetails  -> context.ExecutionKey


// Pass additional custom information from call site into execution context 
var policy = Policy
    .Handle<DataAccessException>()
    .Retry(3, onRetry: (exception, retryCount, context) =>
       {
           logger.Error($"Retry {retryCount} of {context.PolicyKey} at {context.ExecutionKey}, getting {context["Type"]} of id {context["Id"]}, due to: {exception}.");
       })
    .WithPolicyKey("MyDataAccessPolicy");

int id = ... // customer id from somewhere
var customerDetails = policy.Execute(context => GetCustomer(id), 
    new Context("GetCustomerDetails", new Dictionary<string, object>() {{"Type","Customer"},{"Id",id}}
    ));
```

For more detail see: [Keys and Context Data](https://github.com/App-vNext/Polly/wiki/Keys-And-Context-Data) on wiki.

# PolicyRegistry

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


# Asynchronous Support

Polly fully supports asynchronous executions, using the asynchronous methods:

* `RetryAsync`
* `WaitAndRetryAsync`
* `CircuitBreakerAsync`
* (etc)
* `ExecuteAsync`
* `ExecuteAndCaptureAsync`

In place of their synchronous counterparts:

* `Retry`
* `WaitAndRetry`
* `CircuitBreaker`
* (etc)
* `Execute`
* `ExecuteAndCapture`

Async overloads exist for all policy types and for all `Execute()` and `ExecuteAndCapture()` overloads.

Usage example:

```csharp
await Policy
  .Handle<SqlException>(ex => ex.Number == 1205)
  .Or<ArgumentException>(ex => ex.ParamName == "example")
  .RetryAsync()
  .ExecuteAsync(() => DoSomethingAsync());

```

### SynchronizationContext ###

Async continuations and retries by default do not run on a captured synchronization context. To change this, use `.ExecuteAsync(...)` overloads taking a boolean `continueOnCapturedContext` parameter.  

### Cancellation support ###

Async policy execution supports cancellation via `.ExecuteAsync(...)` overloads taking a `CancellationToken`.  

The token you pass as the `cancellationToken` parameter to the `ExecuteAsync(...)` call serves three purposes:

+ It cancels Policy actions such as further retries, waits between retries or waits for a bulkhead execution slot.
+ It is passed by the policy as the `CancellationToken` input parameter to any delegate executed through the policy, to support cancellation during delegate execution.  
+ In common with the Base Class Library implementation in `Task.Run(…)` and elsewhere, if the cancellation token is cancelled before execution begins, the user delegate is not executed at all.

```csharp
// Try several times to retrieve from a uri, but support cancellation at any time.
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

# Thread safety

All Polly policies are fully thread-safe.  You can safely re-use policies at multiple call sites, and execute through policies concurrently on different threads. 

While the internal operation of the policy is thread-safe, this does not magically make delegates you execute through the policy thread-safe: if delegates you execute through the policy are not thread-safe, they remain not thread-safe.


# Interfaces

Polly v5.2.0 adds interfaces intended to support [`PolicyRegistry`](https://github.com/App-vNext/Polly/wiki/PolicyRegistry) and to group Policy functionality by the [interface segregation principle](https://en.wikipedia.org/wiki/Interface_segregation_principle).  Polly's interfaces are not intended for coding your own policy implementations against.

## Execution interfaces: `ISyncPolicy` etc

Execution interfaces [`ISyncPolicy`](https://github.com/App-vNext/Polly/tree/master/src/Polly.Shared/ISyncPolicy.cs), [`IAsyncPolicy`](https://github.com/App-vNext/Polly/tree/master/src/Polly.Shared/IAsyncPolicy.cs), [`ISyncPolicy<TResult>`](https://github.com/App-vNext/Polly/tree/master/src/Polly.Shared/ISyncPolicyTResult.cs) and [`IAsyncPolicy<TResult>`](https://github.com/App-vNext/Polly/tree/master/src/Polly.Shared/IAsyncPolicyTResult.cs)  define the execution overloads available to policies targeting sync/async, and non-generic / generic calls respectively.

See blog posts for why Polly has [both non-generic and generic policies](http://www.thepollyproject.org/2017/06/07/why-does-polly-offer-both-non-generic-and-generic-policies/) and [separate sync and async policies](http://www.thepollyproject.org/2017/06/09/polly-and-synchronous-versus-asynchronous-policies/).

## Policy-kind interfaces: `ICircuitBreakerPolicy` etc

Orthogonal to the execution interfaces, interfaces specific to the kind of Policy define properties and methods common to that type of policy.  

For example, [`ICircuitBreakerPolicy`](https://github.com/App-vNext/Polly/tree/master/src/Polly.Shared/CircuitBreaker/ICircuitBreakerPolicy.cs) defines 


+ `CircuitState CircuitState`
+ `Exception LastException`
+ `void Isolate()`
+ `void Reset()`

with `ICircuitBreakerPolicy<TResult> : ICircuitBreakerPolicy` adding:

+ `TResult LastHandledResult`.

This allows collections of similar kinds of policy to be treated as one - for example, for monitoring all your circuit-breakers as [described here](https://github.com/App-vNext/Polly/pull/205). 

For more detail see: [Polly and interfaces](https://github.com/App-vNext/Polly/wiki/Polly-and-interfaces) on wiki.

# 3rd Party Libraries and Contributions

* [Fluent Assertions](https://github.com/fluentassertions/fluentassertions) - A set of .NET extension methods that allow you to more naturally specify the expected outcome of a TDD or BDD-style test | [Apache License 2.0 (Apache)](https://github.com/dennisdoomen/fluentassertions/blob/develop/LICENSE)
* [xUnit.net](https://github.com/xunit/xunit) - Free, open source, community-focused unit testing tool for the .NET Framework | [Apache License 2.0 (Apache)](https://github.com/xunit/xunit/blob/master/license.txt)
* [Ian Griffith's TimedLock](http://www.interact-sw.co.uk/iangblog/2004/04/26/yetmoretimedlocking)
* [Steven van Deursen's ReadOnlyDictionary](http://www.cuttingedge.it/blogs/steven/pivot/entry.php?id=29) (until Polly v5.0.6)
* [Stephen Cleary's AsyncEx library](https://github.com/StephenCleary/AsyncEx) for AsyncSemaphore (supports BulkheadAsync policy for .NET4.0 only) (until Polly v5.9.0) | [MIT license](https://github.com/StephenCleary/AsyncEx/blob/master/LICENSE)
* [@theraot](https://github.com/theraot)'s [ExceptionDispatchInfo implementation for .NET4.0](https://stackoverflow.com/a/31226509/) (supports Timeout policy for .NET4.0 only) (until Polly v5.9.0) including also a contribution by [@migueldeicaza](https://github.com/migueldeicaza) | Licensed under and distributed under [Creative Commons Attribution Share Alike license](https://creativecommons.org/licenses/by-sa/3.0/) per [StackExchange Terms of Service](https://stackexchange.com/legal)
* Build powered by [Cake](http://cakebuild.net/) and [GitVersionTask](https://github.com/GitTools/GitVersion).

# Acknowledgements

* [lokad-shared-libraries](https://github.com/Lokad/lokad-shared-libraries) - Helper assemblies originally for .NET 3.5 and Silverlight 2.0 which were developed as part of the Open Source effort by Lokad.com (discontinued) | [New BSD License](https://raw.github.com/Lokad/lokad-shared-libraries/master/Lokad.Shared.License.txt)
* [@michael-wolfenden](https://github.com/michael-wolfenden) - The creator and mastermind of Polly!
* [@ghuntley](https://github.com/ghuntley) - Portable Class Library implementation.
* [@mauricedb](https://github.com/mauricedb) - Initial async implementation.
* [@robgibbens](https://github.com/RobGibbens) - Added existing async files to PCL project
* [Hacko](https://github.com/hacko-bede) - Added extra `NotOnCapturedContext` call to prevent potential deadlocks when blocking on asynchronous calls
* [@ThomasMentzel](https://github.com/ThomasMentzel) - Added ability to capture the results of executing a policy via `ExecuteAndCapture`
* [@yevhen](https://github.com/yevhen) - Added full control of whether to continue on captured synchronization context or not
* [@reisenberger](https://github.com/reisenberger) - Added full async cancellation support
* [@reisenberger](https://github.com/reisenberger) - Added async support for ContextualPolicy
* [@reisenberger](https://github.com/reisenberger) - Added ContextualPolicy support for circuit-breaker
* [@reisenberger](https://github.com/reisenberger) - Extended circuit-breaker for public monitoring and control
* [@reisenberger](https://github.com/reisenberger) - Added ExecuteAndCapture support with arbitrary context data 
* [@kristianhald](https://github.com/kristianhald) and [@reisenberger](https://github.com/reisenberger) - Added AdvancedCircuitBreaker
* [@reisenberger](https://github.com/reisenberger) - Allowed async onRetry delegates to async retry policies
* [@Lumirris](https://github.com/Lumirris) - Add new Polly.Net40Async project/package supporting async for .NET40 via Microsoft.Bcl.Async
* [@SteveCote](https://github.com/SteveCote) - Added overloads to WaitAndRetry and WaitAndRetryAsync methods that accept an onRetry delegate which includes the attempt count.
* [@reisenberger](https://github.com/reisenberger) - Allowed policies to handle returned results; added strongly-typed policies Policy&lt;TResult&gt;;.
* [@christopherbahr](https://github.com/christopherbahr) - Added optimisation for circuit-breaker hot path.
* [@Finity](https://github.com/Finity) - Fixed circuit-breaker threshold bug.
* [@reisenberger](https://github.com/reisenberger) - Add some missing ExecuteAndCapture/Async overloads. 
* [@brunolauze](https://github.com/brunolauze) - Add CancellationToken support to synchronous executions (to support TimeoutPolicy).  
* [@reisenberger](https://github.com/reisenberger) - Add PolicyWrap.  
* [@reisenberger](https://github.com/reisenberger) - Add Fallback policy. 
* [@reisenberger](https://github.com/reisenberger) - Add PolicyKeys and context to all policy executions, as bedrock for policy events and metrics tracking executions. 
* [@reisenberger](https://github.com/reisenberger),  and contributions from [@brunolauze](https://github.com/brunolauze) -  Add Bulkhead Isolation policy.  
* [@reisenberger](https://github.com/reisenberger) - Add Timeout policy.
* [@reisenberger](https://github.com/reisenberger) - Fix .NETStandard 1.0 targeting.  Remove PCL259 target.  PCL259 support is provided via .NETStandard1.0 target, going forward.
* [@reisenberger](https://github.com/reisenberger) - Fix CircuitBreaker HalfOpen state and cases when breakDuration is shorter than typical call timeout.  Thanks to [@vgouw](https://github.com/vgouw) and [@kharos](https://github.com/kharos) for the reports and insightful thinking.
* [@lakario](https://github.com/lakario) - Tidy CircuitBreaker LastException property.
* [@lakario](https://github.com/lakario) - Add NoOpPolicy.
* [@Julien-Mialon](https://github.com/Julien-Mialon) - Fixes, support and examples for .NETStandard compatibility with Xamarin PCL projects
* [@reisenberger](https://github.com/reisenberger) - Add mutable Context and extra overloads taking Context.  Allows different parts of a policy execution to exchange data via the mutable Context travelling with each execution.
* [@ankitbko](https://github.com/ankitbko) - Add PolicyRegistry for storing and retrieving policies.
* [@reisenberger](https://github.com/reisenberger) - Add interfaces by policy type and execution type.
* [@seanfarrow](https://github.com/SeanFarrow) - Add IReadOnlyPolicyRegistry interface.
* [@kesmy](https://github.com/Kesmy) - Migrate solution to msbuild15, banish project.json and packages.config
* [@hambudi](https://github.com/hambudi) - Ensure sync TimeoutPolicy with TimeoutStrategy.Pessimistic rethrows delegate exceptions without additional AggregateException.
* [@jiimaho](https://github.com/jiimaho) and [@Extremo75](https://github.com/ExtRemo75) - Provide public factory methods for PolicyResult, to support testing.
* [@Extremo75](https://github.com/ExtRemo75) - Allow fallback delegates to take handled fault as input parameter.
* [@reisenberger](https://github.com/reisenberger) and [@seanfarrow](https://github.com/SeanFarrow) - Add CachePolicy, with interfaces for pluggable cache providers and serializers.
* Thanks to the awesome devs at [@tretton37](https://github.com/tretton37) who delivered the following as part of a one-day in-company hackathon led by [@reisenberger](https://github.com/reisenberger), sponsored by [@tretton37](https://github.com/tretton37) and convened by [@thecodejunkie](https://github.com/thecodejunkie) 
  * [@matst80](https://github.com/matst80) - Allow WaitAndRetry to take handled fault as an input to the sleepDurationProvider, allowing WaitAndRetry to take account of systems which specify a duration to wait as part of a fault response; eg Azure CosmosDB may specify this in `x-ms-retry-after-ms` headers or in a property to an exception thrown by the Azure CosmosDB SDK.
  * [@MartinSStewart](https://github.com/martinsstewart) - Add GetPolicies() extension methods to IPolicyWrap.
  * [@jbergens37](https://github.com/jbergens37) - Parallelize test running where possible, to improve overall build speed.
* [@reisenberger](https://github.com/reisenberger) - Add new .HandleInner<TException>(...) syntax for handling inner exceptions natively.
* [@rjongeneelen](https://github.com/rjongeneelen) and [@reisenberger](https://github.com/reisenberger) - Allow PolicyWrap configuration to configure policies via interfaces. 
* [@reisenberger](https://github.com/reisenberger) - Performance improvements.
* [@awarrenlove](https://github.com/awarrenlove) - Add ability to calculate cache Ttl based on item to cache.
* [@erickhouse](https://github.com/erickhouse) - Add a new onBreak overload that provides the prior state on a transition to an open state.
* [@benagain](https://github.com/benagain) - Bug fix: RelativeTtl in CachePolicy now always returns a ttl relative to time item is cached.
* [@urig](https://github.com/urig) - Allow TimeoutPolicy to be configured with Timeout.InfiniteTimeSpan.
* [@reisenberger](https://github.com/reisenberger) - Integration with [IHttpClientFactory](https://github.com/aspnet/HttpClientFactory/) for ASPNET Core 2.1.
* [@freakazoid182](https://github.com/Freakazoid182) - WaitAnd/RetryForever overloads where onRetry takes the retry number as a parameter.
* [@dustyhoppe](https://github.com/dustyhoppe) - Overloads where onTimeout takes thrown exception as a parameter.
* [@flin-zap](https://github.com/flin-zap) - Catch missing async continuation control.
* [@reisenberger](https://github.com/reisenberger) - Clarify separation of sync and async policies.
* [@reisenberger](https://github.com/reisenberger) - Enable extensibility by custom policies hosted external to Polly.
* [@seanfarrow](https://github.com/SeanFarrow) - Enable collection initialization syntax for PolicyRegistry.
* [@moerwald](https://github.com/moerwald) - Code clean-ups, usage of more concise C# members.
* [@cmeeren](https://github.com/cmeeren) - Enable cache policies to cache values of default(TResult).
* [@aprooks](https://github.com/aprooks) - Build script tweaks for Mac and mono.
* [@kesmy](https://github.com/Kesmy) - Add Soucelink support, clean up cake build.


# Sample Projects

* [Polly-Samples](https://github.com/App-vNext/Polly-Samples) contains practical examples for using various implementations of Polly. Please feel free to contribute to the Polly-Samples repository in order to assist others who are either learning Polly for the first time, or are seeking advanced examples and novel approaches provided by our generous community.

* Microsoft's [eShopOnContainers project](https://github.com/dotnet-architecture/eShopOnContainers) is a sample project demonstrating a .NET Microservices architecture and using Polly for resilience

# Instructions for Contributing

Please be sure to branch from the head of the latest vX.Y.Z dev branch (rather than master) when developing contributions.  

For github workflow, check out our [Wiki](https://github.com/App-vNext/Polly/wiki/Git-Workflow). We are following the excellent GitHub Flow process, and would like to make sure you have all of the information needed to be a world-class contributor!

Since Polly is part of the .NET Foundation, we ask our contributors to abide by their [Code of Conduct](https://www.dotnetfoundation.org/code-of-conduct).  To contribute (beyond trivial typo corrections), review and sign the [.Net Foundation Contributor License Agreement](https://cla.dotnetfoundation.org/). This ensures the community is free to use your contributions.  The registration process can be completed entirely online.

Also, we've stood up a [Slack](http://www.pollytalk.org) channel for easier real-time discussion of ideas and the general direction of Polly as a whole. Be sure to [join the conversation](http://www.pollytalk.org) today!

# License

Licensed under the terms of the [New BSD License](http://opensource.org/licenses/BSD-3-Clause)

# Simmy

[Simmy](https://github.com/Polly-Contrib/Simmy) is a major new companion project adding a chaos-engineering and fault-injection dimension to Polly, through the provision of policies to selectively inject faults or latency.  

Head over to the [Simmy](https://github.com/Polly-Contrib/Simmy) repo to find out more.

# Custom policies

From Polly v7.0 it is possible to [create your own custom policies](http://www.thepollyproject.org/2019/02/13/introducing-custom-polly-policies-and-polly-contrib-custom-policies-part-i/) outside Polly.  These custom policies can integrate in to all the existing goodness from Polly: the `Policy.Handle<>()` syntax; PolicyWrap; all the execution-dispatch overloads.

For more info see our blog series:

+ [Part I: Introducing custom Polly policies and the Polly.Contrib](http://www.thepollyproject.org/2019/02/13/introducing-custom-polly-policies-and-polly-contrib-custom-policies-part-i/)
+ [Part II: Authoring a non-reactive custom policy](http://www.thepollyproject.org/2019/02/13/authoring-a-proactive-polly-policy-custom-policies-part-ii/) (a policy which acts on all executions)
+ [Part III: Authoring a reactive custom policy](http://www.thepollyproject.org/2019/02/13/authoring-a-reactive-polly-policy-custom-policies-part-iii-2/) (a policy which react to faults).
+ [Part IV: Custom policies for all execution types](http://www.thepollyproject.org/2019/02/13/custom-policies-for-all-execution-types-custom-policies-part-iv/): sync and async, generic and non-generic.

We provide a [starter template for a custom policy](https://github.com/Polly-Contrib/Polly.Contrib.CustomPolicyTemplates) for developing your own custom policy. 

# Polly-Contrib

Polly now has a [Polly-Contrib](https://github.com/Polly-Contrib) to allow the community to contribute policies or other enhancements around Polly with a low burden of ceremony.

Have a contrib you'd like to publish under Polly-Contrib? Contact us with  an issue here or on [Polly slack](http://pollytalk.slack.com), and we can set up a Polly.Contrib repo to which you have full rights, to help you manage and deliver your awesomeness to the community!

We also provide:

+ a blank [starter template for a custom policy](https://github.com/Polly-Contrib/Polly.Contrib.CustomPolicyTemplates) (see above for more on custom policies)
+ a [template repo for any other contrib](https://github.com/Polly-Contrib/Polly.Contrib.BlankTemplate)

Both templates contain a full project structure referencing Polly, Polly's default build targets, and a build to build and test your contrib and make a nuget package.

## Available via Polly-Contrib

+ [Polly.Contrib.TimingPolicy](https://github.com/Polly-Contrib/Polly.Contrib.TimingPolicy): a starter policy to publish execution timings of any call executed through Policy.
+ [Polly.Contrib.LoggingPolicy](https://github.com/Polly-Contrib/Polly.Contrib.LoggingPolicy): a policy simply to log handled exceptions/faults, and rethrow or bubble the fault outwards.

# Blogs, podcasts, courses, ebooks, architecture samples and videos around Polly

When we discover an interesting write-up on Polly, we'll add it to this list. If you have a blog post you'd like to share, please submit a PR!

## Blog posts

* [Adding resilience and Transient Fault handling to your .NET Core HttpClient with Polly](https://www.hanselman.com/blog/AddingResilienceAndTransientFaultHandlingToYourNETCoreHttpClientWithPolly.aspx) - by [Scott Hanselman](https://www.hanselman.com/about/) 
* [Reliable Event Processing in Azure Functions](https://hackernoon.com/reliable-event-processing-in-azure-functions-37054dc2d0fc) - by [Jeff Hollan](https://hackernoon.com/@jeffhollan)
* [Optimally configuring ASPNET Core HttpClientFactory](https://rehansaeed.com/optimally-configuring-asp-net-core-httpclientfactory/) including with Polly policies - by [Muhammad Rehan Saeed](https://twitter.com/RehanSaeedUK/)
* [Integrating HttpClientFactory with Polly for transient fault handling](https://www.stevejgordon.co.uk/httpclientfactory-using-polly-for-transient-fault-handling) - by [Steve Gordon](https://www.stevejgordon.co.uk/)
* [Resilient network connectivity in Xamarin Forms](https://xamarinhelp.com/resilient-network-connectivity-xamarin-forms/) - by [Adam Pedley](http://xamarinhelp.com/contact/)
* [Policy recommendations for Azure Cognitive Services](http://www.thepollyproject.org/2018/03/06/policy-recommendations-for-azure-cognitive-services/) - by [Joel Hulen](http://www.thepollyproject.org/author/joel/)
* [Using Polly with F# async workflows](http://blog.ploeh.dk/2017/05/30/using-polly-with-f-async-workflows/) - by [Mark Seemann](http://blog.ploeh.dk/about/)
* [Building resilient applications with Polly](http://elvanydev.com/resilience-with-polly/) (with focus on Azure SQL transient errors) - by [Geovanny Alzate Sandoval](https://github.com/vany0114)
* [Azure SQL transient errors](https://hackernoon.com/azure-sql-transient-errors-7625ad6e0a06) - by [Mattias Karlsson](https://hackernoon.com/@devlead)
* [Polly series on No Dogma blog](http://nodogmablog.bryanhogan.net/tag/polly/) - by [Bryan Hogan](https://twitter.com/bryanjhogan)
* [Polly 5.0 - a wider resilience framework!](http://www.thepollyproject.org/2016/10/25/polly-5-0-a-wider-resilience-framework/) - by [Dylan Reisenberger](http://www.thepollyproject.org/author/dylan/)
* [Implementing the retry pattern in c sharp using Polly](https://alastaircrabtree.com/implementing-the-retry-pattern-using-polly/) - by [Alastair Crabtree](https://alastaircrabtree.com/about/)
* [NuGet Package of the Week: Polly wanna fluently express transient exception handling policies in .NET?](https://www.hanselman.com/blog/NuGetPackageOfTheWeekPollyWannaFluentlyExpressTransientExceptionHandlingPoliciesInNET.aspx) - by [Scott Hanselman](https://www.hanselman.com/about/)
* [Exception handling policies with Polly](http://putridparrot.com/blog/exception-handling-policies-with-polly/) - by [Mark Timmings](http://putridparrot.com/blog/about/)
* [When you use the Polly circuit-breaker, make sure you share your Policy instances!](https://andrewlock.net/when-you-use-the-polly-circuit-breaker-make-sure-you-share-your-policy-instances-2/) - by [Andrew Lock](https://andrewlock.net/about/)
* [Polly is Repetitive, and I love it!](http://www.appvnext.com/blog/2015/11/19/polly-is-repetitive-and-i-love-it) - by [Joel Hulen](http://www.thepollyproject.org/author/joel/)
* [Using the Context to Obtain the Retry Count for Diagnostics](https://www.stevejgordon.co.uk/polly-using-context-to-obtain-retry-count-diagnostics) - by [Steve Gordon](https://twitter.com/stevejgordon)
* [Passing an ILogger to Polly Policies](https://www.stevejgordon.co.uk/passing-an-ilogger-to-polly-policies) - by [Steve Gordon](https://twitter.com/stevejgordon)
* [Using Polly and Flurl to improve your website](https://jeremylindsayni.wordpress.com/2019/01/01/using-polly-and-flurl-to-improve-your-website/) - by Jeremy Lindsay.

## Podcasts

* June 2018: [DotNetRocks features Polly](https://www.dotnetrocks.com/?show=1556) as [Carl Franklin](https://twitter.com/carlfranklin) and [Richard Campbell](https://twitter.com/richcampbell) chat with [Dylan Reisenberger](https://twitter.com/softwarereisen) about policy patterns, and the new ASP NET Core 2.1 integration with IHttpClientFactory.  

* April 2017: [Dylan Reisenberger](https://twitter.com/softwarereisen) sits down virtually with [Bryan Hogan](https://twitter.com/bryanjhogan) of [NoDogmaBlog](http://nodogmablog.bryanhogan.net/) for an [Introduction to Polly podcast](http://nodogmapodcast.bryanhogan.net/71-dylan-reisenberger-the-polly-project/).  Why do I need Polly?  History of the Polly project.  What do we mean by resilience and transient faults?  How retry and circuit-breaker help.  Exponential backoff.  Stability patterns.  Bulkhead isolation.  Future directions (as at April 2017).

## PluralSight course

* [Bryan Hogan](https://twitter.com/bryanjhogan) of the [NoDogmaBlog](http://nodogmablog.bryanhogan.net/) has authored a [PluralSight course on Polly](https://www.pluralsight.com/courses/polly-fault-tolerant-web-service-requests).  The course takes you through all the major features of Polly, with an additional module added in the fall of 2018 on Http Client Factory.  The course examples are based around using Polly for fault tolerance when calling remote web services, but the principles and techniques are applicable to any context in which Polly may be used.

## Sample microservices architecture and ebook

### Sample microservices architecture

* [Cesar de la Torre](https://github.com/CESARDELATORRE) produced the Microsoft [eShopOnContainers project](https://github.com/dotnet-architecture/eShopOnContainers), a sample project demonstrating a .NET Microservices architecture. The project uses Polly retry and circuit-breaker policies for resilience in calls to microservices, and in establishing connections to transports such as RabbitMQ.  

### ebook

* Accompanying the project is a [.Net Microservices Architecture ebook](https://www.microsoft.com/net/download/thank-you/microservices-architecture-ebook) with an extensive section (section 8) on using Polly for resilience, to which [Dylan Reisenberger](https://twitter.com/softwarereisen) contributed.  The ebook and code is now (June 2018) updated for using the latest ASP NET Core 2.1 features, [Polly with IHttpClientFactory](https://github.com/App-vNext/Polly/wiki/Polly-and-HttpClientFactory).

## Twitter

* Follow [Dylan Reisenberger on twitter](https://twitter.com/softwarereisen) for notification of new Polly releases, advance notice of new proposals, tweets of interesting resilience articles (no junk).

## Videos

* [Robust Applications with Polly, the .NET Resilience Framework](https://www.infoq.com/presentations/polly), Bryan Hogan introduces Polly and explains how to use it to build a fault tolerant application.
* From MVP [Houssem Dellai](https://github.com/HoussemDellai), a [youtube video on How to use Polly with Xamarin Apps](https://www.youtube.com/watch?v=7vsN0RkFN_E), covering wait-and-retry and discussing circuit-breaker policy with a demonstration in Xamarin Forms.  Here is the [source code](https://github.com/HoussemDellai/ResilientHttpClient) of the application demonstrated in the video.  Draws on the [`ResilientHttpClient`](https://github.com/dotnet-architecture/eShopOnContainers/blob/dev/src/BuildingBlocks/Resilience/Resilience.Http/ResilientHttpClient.cs) from Microsoft's [eShopOnContainers project](https://github.com/dotnet-architecture/eShopOnContainers).
* In the video, [.NET Rocks Live with Jon Skeet and Bill Wagner](https://youtu.be/LCj7h7ZoHA8?t=1617), Bill Wagner discusses Polly.
* Scott Allen discusses Polly during his [Building for Resiliency and Scale in the Cloud](https://youtu.be/SFLu6jZWXGs?t=1440) presentation at NDC.
* [ASP.NET Community Standup April 24, 2018](https://youtu.be/k0Xy-5zE9to?t=12m22s): Damian Edwards, Jon Galloway and Scott Hanselman discuss Scott Hanselman's blog on [Polly with IHttpClientFactory](https://www.hanselman.com/blog/AddingResilienceAndTransientFaultHandlingToYourNETCoreHttpClientWithPolly.aspx) and the [Polly team documentation on IHttpClientFactory](https://github.com/App-vNext/Polly/wiki/Polly-and-HttpClientFactory). Interesting background discussion also on feature richness and the importance of good documentation. 
