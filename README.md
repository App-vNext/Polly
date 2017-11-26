# Polly

Polly is a .NET resilience and transient-fault-handling library that allows developers to express policies such as Retry, Circuit Breaker, Timeout, Bulkhead Isolation, and Fallback in a fluent and thread-safe manner.  

Polly targets .NET 4.0, .NET 4.5 and .NET Standard 1.1 ([coverage](https://github.com/dotnet/standard/blob/master/docs/versions.md): .NET Core, Mono, Xamarin.iOS, Xamarin.Android, UWP, WP8.1+).

[<img align="right" src="https://dotnetfoundation.org/Themes/DotNetFoundation.Theme/Images/logo-small.png" width="100" />](https://www.dotnetfoundation.org/)
We are now a member of the [.NET Foundation](https://www.dotnetfoundation.org/about)!

**Keep up to date with new feature announcements, tips & tricks, and other news through [www.thepollyproject.org](http://www.thepollyproject.org)**

[![NuGet version](https://badge.fury.io/nu/polly.svg)](https://badge.fury.io/nu/polly) [![Build status](https://ci.appveyor.com/api/projects/status/imt7dymt50346k5u?svg=true)](https://ci.appveyor.com/project/joelhulen/polly) [![Slack Status](http://www.pollytalk.org/badge.svg)](http://www.pollytalk.org)

![](https://raw.github.com/App-vNext/Polly/master/Polly-Logo.png)

# Installing via NuGet

    Install-Package Polly

You can install the Strongly Named version via: 

    Install-Package Polly-Signed

.NET4.0 support is provided via the packages:

    Install-Package Polly.Net40Async
    Install-Package Polly.Net40Async-Signed


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

# Usage &ndash; fault-handling policies

Fault-handling policies handle specific exceptions thrown by, or results returned by, the delegates you execute through the policy.

## Step 1 : Specify the  exceptions/faults you want the policy to handle 

### (for fault-handling policies:  Retry family, CircuitBreaker family and Fallback)

```csharp
// Single exception type
Policy
  .Handle<DivideByZeroException>()

// Single exception type with condition
Policy
  .Handle<SqlException>(ex => ex.Number == 1205)

// Multiple exception types
Policy
  .Handle<DivideByZeroException>()
  .Or<ArgumentException>()

// Multiple exception types with condition
Policy
  .Handle<SqlException>(ex => ex.Number == 1205)
  .Or<ArgumentException>(ex => ex.ParamName == "example")

// Inner exceptions of ordinary exceptions or AggregateException, with or without conditions
Policy
  .HandleInner<HttpResponseException>()
  .OrInner<OperationCanceledException>(ex => ex.CancellationToken == myToken)
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
  .OrResult<HttpResponseMessage>(r => r.StatusCode == HttpStatusCode.BadGateway)

// Handle primitive return values (implied use of .Equals())
Policy
  .HandleResult<HttpStatusCode>(HttpStatusCode.InternalServerError)
  .OrResult<HttpStatusCode>(HttpStatusCode.BadGateway)
 
// Handle both exceptions and return values in one policy
HttpStatusCode[] httpStatusCodesWorthRetrying = {
   HttpStatusCode.RequestTimeout, // 408
   HttpStatusCode.InternalServerError, // 500
   HttpStatusCode.BadGateway, // 502
   HttpStatusCode.ServiceUnavailable, // 503
   HttpStatusCode.GatewayTimeout // 504
}; 
HttpResponseMessage result = Policy
  .Handle<HttpResponseException>()
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
  .Handle<DivideByZeroException>()
  .Retry()

// Retry multiple times
Policy
  .Handle<DivideByZeroException>()
  .Retry(3)

// Retry multiple times, calling an action on each retry 
// with the current exception and retry count
Policy
    .Handle<DivideByZeroException>()
    .Retry(3, (exception, retryCount) =>
    {
        // do something 
    });

// Retry multiple times, calling an action on each retry 
// with the current exception, retry count and context 
// provided to Execute()
Policy
    .Handle<DivideByZeroException>()
    .Retry(3, (exception, retryCount, context) =>
    {
        // do something 
    });
```

### Retry forever (until succeeds)

```csharp

// Retry forever
Policy
  .Handle<DivideByZeroException>()
  .RetryForever()

// Retry forever, calling an action on each retry with the 
// current exception
Policy
  .Handle<DivideByZeroException>()
  .RetryForever(exception =>
  {
        // do something       
  });

// Retry forever, calling an action on each retry with the
// current exception and context provided to Execute()
Policy
  .Handle<DivideByZeroException>()
  .RetryForever((exception, context) =>
  {
        // do something       
  });
```

### Wait and retry 

```csharp
// Retry, waiting a specified duration between each retry
Policy
  .Handle<DivideByZeroException>()
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
  .Handle<DivideByZeroException>()
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
  .Handle<DivideByZeroException>()
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
  .Handle<DivideByZeroException>()
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
  .Handle<DivideByZeroException>()
  .WaitAndRetry(5, retryAttempt => 
	TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) 
  );

// Retry a specified number of times, using a function to 
// calculate the duration to wait between retries based on 
// the current retry attempt, calling an action on each retry 
// with the current exception, duration and context provided 
// to Execute()
Policy
  .Handle<DivideByZeroException>()
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
  .Handle<DivideByZeroException>()
  .WaitAndRetry(
    5, 
    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), 
    (exception, timeSpan, retryCount, context) => {
      // do something
    }
  );
```

### Wait and retry forever (until succeeds) 

```csharp

// Wait and retry forever
Policy
  .Handle<DivideByZeroException>()
  .WaitAndRetryForever(retryAttempt => 
	TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
    );

// Wait and retry forever, calling an action on each retry with the 
// current exception and the time to wait
Policy
  .Handle<DivideByZeroException>()
  .WaitAndRetryForever(
    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),    
    (exception, timespan) =>
    {
        // do something       
    });

// Wait and retry forever, calling an action on each retry with the
// current exception, time to wait, and context provided to Execute()
Policy
  .Handle<DivideByZeroException>()
  .WaitAndRetryForever(
    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),    
    (exception, timespan, context) =>
    {
        // do something       
    });
```

For more detail see: [Retry policy documentation](https://github.com/App-vNext/Polly/wiki/Retry) on wiki.

### Circuit Breaker
 
```csharp
// Break the circuit after the specified number of consecutive exceptions
// and keep circuit broken for the specified duration.
Policy
    .Handle<DivideByZeroException>()
    .CircuitBreaker(2, TimeSpan.FromMinutes(1));

// Break the circuit after the specified number of consecutive exceptions
// and keep circuit broken for the specified duration,
// calling an action on change of circuit state.
Action<Exception, TimeSpan> onBreak = (exception, timespan) => { ... };
Action onReset = () => { ... };
CircuitBreakerPolicy breaker = Policy
    .Handle<DivideByZeroException>()
    .CircuitBreaker(2, TimeSpan.FromMinutes(1), onBreak, onReset);

// Break the circuit after the specified number of consecutive exceptions
// and keep circuit broken for the specified duration,
// calling an action on change of circuit state,
// passing a context provided to Execute().
Action<Exception, TimeSpan, Context> onBreak = (exception, timespan, context) => { ... };
Action<Context> onReset = context => { ... };
CircuitBreakerPolicy breaker = Policy
    .Handle<DivideByZeroException>()
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
For more detail see: [Circuit-Breaker documentation](https://github.com/App-vNext/Polly/wiki/Circuit-Breaker) on wiki.

### Advanced Circuit Breaker 
```csharp
// Break the circuit if, within any period of duration samplingDuration, 
// the proportion of actions resulting in a handled exception exceeds failureThreshold, 
// provided also that the number of actions through the circuit in the period
// is at least minimumThroughput.

Policy
    .Handle<DivideByZeroException>()
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
* [Circuit breaking with Polly](http://blog.jaywayco.co.uk/circuit-breaking-with-polly/)
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
              .Handle<DivideByZeroException>()
              .Retry();

policy.Execute(() => DoSomething());

// Execute an action passing arbitrary context data
var policy = Policy
    .Handle<DivideByZeroException>()
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
              .Handle<DivideByZeroException>()
              .Retry();

var result = policy.Execute(() => DoSomething());

// Execute a function returning a result passing arbitrary context data
var policy = Policy
    .Handle<DivideByZeroException>()
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

The above examples show policy definition immediately followed by policy execution, for simplicity.  Policy definition and execution may equally be separated - for example, define policies on start-up, then provide them to point-of-use by dependency injection (perhaps using [`PolicyRegistry`](#policyregistry)).

# Usage &ndash; general resilience policies

The general resilience policies add resilience strategies that are not explicitly centred around handling faults which delegates may throw or return.

## Step 1 : Configure

### Timeout

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

// Timeout after 30 seconds, if the executed delegate has not completed.  Enforces a timeout on delegates which have no in-built timeout and do not honour CancellationToken, at the expense (in synchronous executions) of an extra thread.
// (for more detail, see deep documentation)
Policy
  .Timeout(30, TimeoutStrategy.Pessimistic)

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


For more detail see: [Bulkhead policy documentation](https://github.com/App-vNext/Polly/wiki/Bulkhead) on wiki.

### Cache

```csharp
// Define a cache Policy in the .NET Framework, using the Polly.Caching.MemoryCache nuget package.
var memoryCacheProvider = new Polly.Caching.MemoryCache.MemoryCacheProvider(MemoryCache.Default);
var cachePolicy = Policy.Cache(memoryCacheProvider, TimeSpan.FromMinutes(5));

// Define a cache policy with absolute expiration at midnight tonight.
var cachePolicy = Policy.Cache(memoryCacheProvider, new AbsoluteTtl(DateTimeOffset.Now.Date.AddDays(1));

// Define a cache policy with sliding expiration: items remain valid for another 5 minutes each time the cache item is used.
var cachePolicy = Policy.Cache(memoryCacheProvider, new SlidingTtl(TimeSpan.FromMinutes(5));

// Execute through the cache as a read-through cache: check the cache first; if not found, execute underlying delegate and store the result in the cache. 
TResult result = cachePolicy.Execute(() => getFoo(), new Context("FooKey")); // "FooKey" is the cache key used in this execution.

// Define a cache Policy, and catch any cache provider errors for logging. 
var cachePolicy = Policy.Cache(myCacheProvider, TimeSpan.FromMinutes(5), 
   (context, key, ex) => { 
       logger.Error($"Cache provider, for key {key}, threw exception: {ex}."); // (for example) 
   }
);


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

## Step 2 : Execute the policy.

As for fault-handling policies [above](#step-3--execute-the-policy).

# Post-execution: capturing the result, or any final exception

Using the `ExecuteAndCapture(...)` methods you can capture the result of executing a policy.

```csharp
var policyResult = Policy
              .Handle<DivideByZeroException>()
              .Retry()
              .ExecuteAndCapture(() => DoSomething());
/*              
policyResult.Outcome - whether the call succeeded or failed         
policyResult.FinalException - the final exception captured, will be null if the call succeeded
policyResult.ExceptionType - was the final exception the policy was defined to handle (like DivideByZeroException above) or an unhandled one (say Exception). Will be null if the call succeeded.
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
HttpResponseMessage result = Policy
  .Handle<HttpResponseException>()
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
policyResult.ExceptionType - was the final exception an exception the policy was defined to handle (like DivideByZeroException above) or an unhandled one (say Exception)? Will be null if the call succeeded.
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
var customerDetails = policy.Execute(() => GetCustomer(id), 
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


# Thread safety

All Polly policies are fully thread-safe.  You can safely re-use policies at multiple call sites, and execute through policies concurrently on different threads. 

While the internal operation of the policy is thread-safe, this does not magically make delegates you execute through the policy thread-safe: if delegates you execute through the policy are not thread-safe, they remain not thread-safe.


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
    .Handle<WebException>()
    .Or<HttpRequestException>()
    .WaitAndRetryAsync(new[] { 
        TimeSpan.FromSeconds(1), 
        TimeSpan.FromSeconds(2), 
        TimeSpan.FromSeconds(4) 
    });
var response = await policy.ExecuteAsync(ct => httpClient.GetAsync(uri, ct), cancellationToken);
```

From Polly v5.0, synchronous executions also support cancellation via `CancellationToken`.

# .NET4.0 support 

The .NET4.0 package uses `Microsoft.Bcl.Async` to add async support.  To minimise  dependencies on the main Polly nuget package, the .NET4.0 version is available as separate Nuget packages `Polly.Net40Async` and `Polly.Net40Async-signed`.

# Release notes

For details of changes by release see the [change log](https://github.com/App-vNext/Polly/blob/master/CHANGELOG.md).  We also tag relevant Pull Requests and Issues with [milestones](https://github.com/App-vNext/Polly/milestones), which match to nuget package release numbers.

# 3rd Party Libraries and Contributions

* [Fluent Assertions](https://github.com/fluentassertions/fluentassertions) - A set of .NET extension methods that allow you to more naturally specify the expected outcome of a TDD or BDD-style test | [Apache License 2.0 (Apache)](https://github.com/dennisdoomen/fluentassertions/blob/develop/LICENSE)
* [xUnit.net](https://github.com/xunit/xunit) - Free, open source, community-focused unit testing tool for the .NET Framework | [Apache License 2.0 (Apache)](https://github.com/xunit/xunit/blob/master/license.txt)
* [Ian Griffith's TimedLock](http://www.interact-sw.co.uk/iangblog/2004/04/26/yetmoretimedlocking)
* [Steven van Deursen's ReadOnlyDictionary](http://www.cuttingedge.it/blogs/steven/pivot/entry.php?id=29) (until v5.0.6)
* [Stephen Cleary's AsyncEx library](https://github.com/StephenCleary/AsyncEx) for AsyncSemaphore (supports BulkheadAsync policy for .NET4.0 only) | [MIT license](https://github.com/StephenCleary/AsyncEx/blob/master/LICENSE)
* [@theraot](https://github.com/theraot)'s [ExceptionDispatchInfo implementation for .NET4.0](https://stackoverflow.com/a/31226509/) (supports Timeout policy for .NET4.0 only) including also a contribution by [@migueldeicaza](https://github.com/migueldeicaza) | Licensed under and distributed under [Creative Commons Attribution Share Alike license](https://creativecommons.org/licenses/by-sa/3.0/) per [StackExchange Terms of Service](https://stackexchange.com/legal)
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

# Sample Projects

[Polly-Samples](https://github.com/App-vNext/Polly-Samples) contains practical examples for using various implementations of Polly. Please feel free to contribute to the Polly-Samples repository in order to assist others who are either learning Polly for the first time, or are seeking advanced examples and novel approaches provided by our generous community.

# Instructions for Contributing

Please check out our [Wiki](https://github.com/App-vNext/Polly/wiki/Git-Workflow) for contributing guidelines. We are following the excellent GitHub Flow process, and would like to make sure you have all of the information needed to be a world-class contributor!

Since Polly is part of the .NET Foundation, we ask our contributors to abide by their [Code of Conduct](https://www.dotnetfoundation.org/code-of-conduct).  To contribute (beyond trivial typo corrections), review and sign the .Net Foundation Contributor License Agreement ([info](https://cla.dotnetfoundation.org/); [sign](https://cla2.dotnetfoundation.org/)). This ensures the community is free to use your contributions.  The registration process can be completed entirely online.

Also, we've stood up a [Slack](http://www.pollytalk.org) channel for easier real-time discussion of ideas and the general direction of Polly as a whole. Be sure to [join the conversation](http://www.pollytalk.org) today!

# License

Licensed under the terms of the [New BSD License](http://opensource.org/licenses/BSD-3-Clause)

# Blog posts, podcasts and videos about Polly

When we discover an interesting write-up on Polly, we'll add it to this list. If you have a blog post you'd like to share, please submit a PR!

## Blog posts

* [Using Polly with F# async workflows](http://blog.ploeh.dk/2017/05/30/using-polly-with-f-async-workflows/) - by [Mark Seemann](http://blog.ploeh.dk/about/)
* [Azure SQL transient errors](https://hackernoon.com/azure-sql-transient-errors-7625ad6e0a06) - by [Mattias Karlsson](https://hackernoon.com/@devlead)
* [Polly series on No Dogma blog](http://nodogmablog.bryanhogan.net/tag/polly/) - by [Bryan Hogan](https://twitter.com/bryanjhogan)
* [Polly 5.0 - a wider resilience framework!](http://www.thepollyproject.org/2016/10/25/polly-5-0-a-wider-resilience-framework/) - by [Dylan Reisenberger](http://www.thepollyproject.org/author/dylan/)
* [Implementing the retry pattern in c sharp using Polly](https://alastaircrabtree.com/implementing-the-retry-pattern-using-polly/) - by [Alastair Crabtree](https://alastaircrabtree.com/about/)
* [NuGet Package of the Week: Polly wanna fluently express transient exception handling policies in .NET?](https://www.hanselman.com/blog/NuGetPackageOfTheWeekPollyWannaFluentlyExpressTransientExceptionHandlingPoliciesInNET.aspx) - by [Scott Hanselman](https://www.hanselman.com/about/)
* [Circuit Breaking with Polly](http://blog.jaywayco.co.uk/circuit-breaking-with-polly/) - by [J. Conway](http://blog.jaywayco.co.uk/sample-page/)
* [Exception handling policies with Polly](http://putridparrot.com/blog/exception-handling-policies-with-polly/) - by [Mark Timmings](http://putridparrot.com/blog/about/)
* [Polly is Repetitive, and I love it!](http://www.appvnext.com/blog/2015/11/19/polly-is-repetitive-and-i-love-it) - by [Joel Hulen](http://www.thepollyproject.org/author/joel/)

## Podcasts

* [Dylan Reisenberger](https://twitter.com/softwarereisen) sits down virtually with [Bryan Hogan](https://twitter.com/bryanjhogan) of [NoDogmaBlog](http://nodogmablog.bryanhogan.net/) for an [Introduction to Polly podcast](http://nodogmapodcast.bryanhogan.net/71-dylan-reisenberger-the-polly-project/).  Why do I need Polly?  History of the Polly project.  What do we mean by resilience and transient faults?  How retry and circuit-breaker help.  Exponential backoff.  Stability patterns.  Bulkhead isolation.  Future directions (as at April 2017).

## Twitter

* Follow [Dylan Reisenberger on twitter](https://twitter.com/softwarereisen) for notification of new Polly releases, advance notice of new proposals, tweets of interesting resilience articles (no junk).

## Videos

* From MVP [Houssem Dellai](https://github.com/HoussemDellai), a [youtube video on How to use Polly with Xamarin Apps](https://www.youtube.com/watch?v=7vsN0RkFN_E), covering wait-and-retry and discussing circuit-breaker policy with a demonstration in Xamarin Forms.  Here is the [source code](https://github.com/HoussemDellai/ResilientHttpClient) of the application demonstrated in the video.  Draws on the [`ResilientHttpClient`](https://github.com/dotnet-architecture/eShopOnContainers/blob/dev/src/BuildingBlocks/Resilience/Resilience.Http/ResilientHttpClient.cs) from Microsoft's [eShopOnContainers project](https://github.com/dotnet-architecture/eShopOnContainers).     

# Laptop Stickers!

Show your support for Polly by getting your laptop sticker today, available from [Sticker Mule](https://www.stickermule.com/en/marketplace/15794-polly-dot-net)!

[![Polly Sticker](https://www.stickermule.com/en/marketplace/embed_img/15794)](https://www.stickermule.com/en/marketplace/15794-polly-dot-net)
