# Polly

Polly is a .NET 3.5 / 4.0 / 4.5 / PCL (Profile 259) library that allows developers to express transient exception- and fault-handling policies such as Retry, Retry Forever, Wait and Retry or Circuit Breaker in a fluent manner.

[![NuGet version](https://badge.fury.io/nu/polly.svg)](https://badge.fury.io/nu/polly) [![Build status](https://ci.appveyor.com/api/projects/status/imt7dymt50346k5u?svg=true)](https://ci.appveyor.com/project/joelhulen/polly)

![](https://raw.github.com/App-vNext/Polly/master/Polly.png)

# Installing via NuGet


    Install-Package Polly

You can install the Strongly Named version via: 

    Install-Package Polly-Signed

There are now .NET 4.0 Async versions (via Microsoft.Bcl.Async) of the signed and unsigned NuGet packages, which can be installed via:

    Install-Package Polly.Net40Async
    Install-Package Polly.Net40Async-Signed

**Please note:** The Polly.Net40Async package is only needed if you are targeting .NET 4.0 and need async capabilities. If you are targeting .NET 4.5 or greater, please use the standard Polly package.

# Usage

## Step 1 : Specify the type of exceptions you want the policy to handle ##

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
```

### Step 1b: (optionally) Specify return results you want to handle

From Polly v4.3.0 onwards, policies wrapping calls returning a `TResult` can also handle `TResult` return values:

```csharp
// Handle return value with condition 
Policy
  .HandleResult<HttpResponse>(r => r.StatusCode == 404)

// Handle multiple return values 
Policy
  .HandleResult<HttpResponse>(r => r.StatusCode == 500)
  .OrResult<HttpResponse>(r => r.StatusCode == 502)

// Handle primitive return values (implied use of .Equals())
Policy
  .HandleResult<HttpStatusCode>(HttpStatusCode.InternalServerError)
  .OrResult<HttpStatusCode>(HttpStatusCode.BadGateway)
 
// Handle both exceptions and return values in one policy
int[] httpStatusCodesWorthRetrying = {408, 500, 502, 503, 504}; 
HttpResponse result = Policy
  .Handle<HttpException>()
  .OrResult<HttpResponse>(r => httpStatusCodesWorthRetrying.Contains(r.StatusCode))
```

For more information, see Handling Return Values at foot of this readme. 

## Step 2 : Specify how the policy should handle those faults

### Retry ###

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

### Retry forever ###

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

### Retry and Wait ###

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

### Wait and retry forever ###

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

For further information on the operation of retry policies, see also the [wiki](https://github.com/App-vNext/Polly/wiki/Retry).

### Circuit Breaker ###
```csharp
// Break the circuit after the specified number of exceptions
// and keep circuit broken for the specified duration.
Policy
    .Handle<DivideByZeroException>()
    .CircuitBreaker(2, TimeSpan.FromMinutes(1));

// Break the circuit after the specified number of exceptions
// and keep circuit broken for the specified duration,
// calling an action on change of circuit state.
Action<Exception, TimeSpan> onBreak = (exception, timespan) => { ... };
Action onReset = () => { ... };
CircuitBreakerPolicy breaker = Policy
    .Handle<DivideByZeroException>()
    .CircuitBreaker(2, TimeSpan.FromMinutes(1), onBreak, onReset);

// Break the circuit after the specified number of exceptions
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

For further information on the operation of circuit breaker, see also the [wiki](https://github.com/App-vNext/Polly/wiki/Circuit-Breaker).

### Advanced Circuit Breaker ###
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

For further information on the operation of Advanced Circuit Breaker, see the [Wiki](https://github.com/App-vNext/Polly/wiki/Advanced-Circuit-Breaker)

For more information on the Circuit Breaker pattern in general see:
* [Making the Netflix API More Resilient](http://techblog.netflix.com/2011/12/making-netflix-api-more-resilient.html)
* [Circuit Breaker (Martin Fowler)](http://martinfowler.com/bliki/CircuitBreaker.html)
* [Circuit Breaker Pattern (Microsoft)](https://msdn.microsoft.com/en-us/library/dn589784.aspx)
* [Circuit breaking with Polly](http://blog.jaywayco.co.uk/circuit-breaking-with-polly/)
* [Original Circuit Breaking Link](https://web.archive.org/web/20160106203951/http://thatextramile.be/blog/2008/05/the-circuit-breaker)
 

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

# Post Execution Steps

Using the `ExecuteAndCapture` method you can capture the result of executing a policy.

```csharp
var policyResult = Policy
              .Handle<DivideByZeroException>()
              .Retry()
              .ExecuteAndCapture(() => DoSomething());
/*              
policyResult.Outcome - whether the call succeeded or failed         
policyResult.FinalException - the final exception captured, will be null if the call succeeded
policyResult.ExceptionType - was the final exception an exception the policy was defined to handle (like DivideByZeroException above) or an unhandled one (say Exception). Will be null if the call succeeded.
policyResult.Result - if executing a func, the result if the call succeeded or the type's default value
*/
```

# Asynchronous Support (.NET 4.5, PCL and .NET4.0)

You can use Polly with asynchronous functions by using the asynchronous methods

* `RetryAsync`
* `RetryForeverAsync`
* `WaitAndRetryAsync`
* `WaitAndRetryForeverAsync`
* `CircuitBreakerAsync`
* `AdvancedCircuitBreakerAsync`
* `ExecuteAsync`
* `ExecuteAndCaptureAsync`

In place of their synchronous counterparts

* `Retry`
* `RetryForever`
* `WaitAndRetry`
* `WaitAndRetryForever`
* `CircuitBreaker`
* `AdvancedCircuitBreaker`
* `Execute`
* `ExecuteAndCapture`

For example

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

Cancellation cancels Policy actions such as further retries and waits between retries.  The delegate taken by the relevant `.ExecuteAsync(...)` overloads also takes a cancellation token input parameter, to support cancellation during delegate execution.  

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
### .NET4.0 Async support ###

The .NET4.0 Async support uses `Microsoft.Bcl.Async` to add async support to a .NET4.0 package.  To minimise extra dependencies on the main Polly nuget package, the .NET4.0 async version is available as separate Nuget packages `Polly.Net40Async` and `Polly.Net40Async-signed`.

# Handing return values, and Policy&lt;TResult&gt;

As described at step 1b, from Polly v4.3.0 onwards, policies can handle return values and exceptions in combination: 

```csharp
// Handle both exceptions and return values in one policy
int[] httpStatusCodesWorthRetrying = {408, 500, 502, 503, 504}; 
HttpResponse result = Policy
  .Handle<HttpException>()
  .OrResult<HttpResponse>(r => httpStatusCodesWorthRetrying.Contains(r.StatusCode))
  .Retry(...)
  .Execute( /* some Func<HttpResponse> */ )
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
policyResult.Result - if executing a func, the result if the call succeeded or the type's default value
```

`.ExecuteAndCapture<TResult>(Func<TResult>)` on strongly-typed policies adds two properties:

```
policyResult.FaultType - was the final fault handled an exception or a result handled by the policy? Will be null if the delegate execution succeeded. 
policyResult.FinalHandledResult - the final result handled; will be null if the call succeeded or the type's default value
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

# 3rd Party Libraries

* [Fluent Assertions](https://github.com/dennisdoomen/fluentassertions) - A set of .NET extension methods that allow you to more naturally specify the expected outcome of a TDD or BDD-style test | [Apache License 2.0 (Apache)](https://github.com/dennisdoomen/fluentassertions/blob/develop/LICENSE)
* [xUnit.net](https://github.com/xunit/xunit) - Free, open source, community-focused unit testing tool for the .NET Framework | [Apache License 2.0 (Apache)](https://github.com/xunit/xunit/blob/master/license.txt)
* [Ian Griffith's TimedLock] (http://www.interact-sw.co.uk/iangblog/2004/04/26/yetmoretimedlocking)
* [Steven van Deursen's ReadOnlyDictionary] (http://www.cuttingedge.it/blogs/steven/pivot/entry.php?id=29)

# Acknowledgements

* [lokad-shared-libraries](https://github.com/Lokad/lokad-shared-libraries) - Helper assemblies for .NET 3.5 and Silverlight 2.0 that are being developed as part of the Open Source effort by Lokad.com (discontinued) | [New BSD License](https://raw.github.com/Lokad/lokad-shared-libraries/master/Lokad.Shared.License.txt)
* [@michael-wolfenden](https://github.com/michael-wolfenden) - The creator and mastermind of Polly!
* [@ghuntley](https://github.com/ghuntley) - Portable Class Library implementation.
* [@mauricedb](https://github.com/mauricedb) - Async implementation.
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

# Sample Projects

[Polly-Samples](https://github.com/App-vNext/Polly-Samples) contains practical examples for using various implementations of Polly. Please feel free to contribute to the Polly-Samples repository in order to assist others who are either learning Polly for the first time, or are seeking advanced examples and novel approaches provided by our generous community.

# Instructions for Contributing

Please check out our [Wiki](https://github.com/App-vNext/Polly/wiki/Git-Workflow) for contributing guidelines. We are following the excellent GitHub Flow process, and would like to make sure you have all of the information needed to be a world-class contributor!

# License

Licensed under the terms of the [New BSD License](http://opensource.org/licenses/BSD-3-Clause)
