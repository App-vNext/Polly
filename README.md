Polly
=
Polly is a .NET 3.5 / 4.0 / 4.5 / PCL library that allows developers to express transient exception handling policies such as Retry, Retry Forever, Wait and Retry or Circuit Breaker in a fluent manner.

![](https://raw.github.com/michael-wolfenden/Polly/master/Polly.png)

Installing via NuGet
=

    Install-Package Polly

You can install the Strongly Named version via: 

    Install-Package Polly-Signed

Usage
=
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
  .Or<ArgumentException>(ex => x.ParamName == "example")
```

## Step 2 : Specify how the policy should handle those exceptions

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
    1.Seconds(),
    2.Seconds(),
    3.Seconds()
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
    1.Seconds(),
    2.Seconds(),
    3.Seconds()
  }, (exception, timeSpan, context) => {
    // do something    
  });

// Retry a specified number of times, using a function to 
// calculate the duration to wait between retries based on 
// the current retry attempt (allows for exponential backoff)
// In this case will wait for
//  1 ^ 2 = 2 seconds then
//  2 ^ 2 = 4 seconds then
//  3 ^ 2 = 8 seconds then
//  4 ^ 2 = 16 seconds then
//  5 ^ 2 = 32 seconds
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
```

### Circuit Breaker ###
```csharp
// Break the circuit after the specified number of exceptions
// and keep circuit broken for the specified duration
Policy
  .Handle<DivideByZeroException>()
  .CircuitBreaker(2, TimeSpan.FromMinutes(1));
```

For more information on the Circuit Breaker pattern see:
* [Making the Netflix API More Resilient](http://techblog.netflix.com/2011/12/making-netflix-api-more-resilient.html)
* [The Circuit Breaker](http://thatextramile.be/blog/2008/05/the-circuit-breaker)
 
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

3rd Party Libraries
=

* [Fluent Assertions](http://fluentassertions.codeplex.com/) - A set of .NET extension methods that allow you to more naturally specify the expected outcome of a TDD or BDD-style test | [Microsoft Public License (Ms-PL)](http://fluentassertions.codeplex.com/license)

* [xUnit.net](http://xunit.codeplex.com/) - Free, open source, community-focused unit testing tool for the .NET Framework | [Apache License 2.0 (Apache)](http://xunit.codeplex.com/license)

* [Ian Griffith's TimedLock] (http://www.interact-sw.co.uk/iangblog/2004/04/26/yetmoretimedlocking)

* [Steven van Deursen's ReadOnlyDictionary] (http://www.cuttingedge.it/blogs/steven/pivot/entry.php?id=29)

Acknowledgements
=

* [lokad-shared-libraries](https://github.com/Lokad/lokad-shared-libraries) - Helper assemblies for .NET 3.5 and Silverlight 2.0 that are being developed as part of the Open Source effort by Lokad.com (discontinued) | [New BSD License](https://raw.github.com/Lokad/lokad-shared-libraries/master/Lokad.Shared.License.txt)
* [@ghuntley](https://github.com/ghuntley) - Contributed Portable Class Library implementation.

License
=
Licensed under the terms of the [New BSD License](http://opensource.org/licenses/BSD-3-Clause)
