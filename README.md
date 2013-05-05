Polly
=
Polly is a .NET 3.5 / 4.0 / 4.5 library that allows developers to express transient exception handling policies such as Retry, Retry Forever, Wait and Retry or Circuit Breaker in a fluent manner.

![](https://raw.github.com/michael-wolfenden/Polly/master/Polly.png)

Installing via NuGet
=

    Install-Package Polly

Usage
=
## Step 1 : Specify the type of exceptions you want the policy to handle ##

### Single exception type

```csharp
Policy
  .Handle<DivideByZeroException>()
```

### Single exception type with condition

```csharp
Policy
  .Handle<DivideByZeroException>()
  .Or<ArgumentException>()

```

### Multiple exception types

```csharp
Policy
  .Handle<DivideByZeroException>()
```

### Multiple exception types with condition

```csharp
Policy
  .Handle<SqlException>(ex => ex.Number == 1205)
  .Or<ArgumentException>(ex => ex => x.ParamName == "example")
```

## Step 2 : Specifiy how the policy should handle those exceptions

### Retry once ###

```csharp
Policy
  .Handle<DivideByZeroException>()
  .Retry()
```

### Retry multiple times ###

```csharp
Policy
  .Handle<DivideByZeroException>()
  .Retry(3)
```

### Retry multiple times calling an action on each retry with the current exception and retry count ###

```csharp
Policy
    .Handle<DivideByZeroException>()
    .Retry(3, (exception, retyCount) =>
    {
        // do something 
    });
```

### Retry forever ###

```csharp
Policy
  .Handle<DivideByZeroException>()
  .RetryForever()
```

### Retry forever calling an action on each retry with the current exception ###

```csharp
Policy
  .Handle<DivideByZeroException>()
  .RetryForever(exception =>
  {
        // do something       
  });
```

### Retry, waiting a specified time duration between each retry ###

```csharp
Policy
  .Handle<DivideByZeroException>()
  .WaitAndRetry(new[]
  {
    TimeSpan.FromSeconds(1),
    TimeSpan.FromSeconds(2),
    TimeSpan.FromSeconds(3)
  });
```

### Retry, waiting a specified time between each retry and calling an action on each retry with the current exception and timespan

```csharp
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
```

### Retry a specified number of times passing a function to calculate the interval to wait between retries based on the current retry attempt (allows for exponential backoff)
```csharp
// in this case will wait for
//  1 ^ 2 = 2 seconds then
//  2 ^ 2 = 4 seconds then
//  3 ^ 2 = 8 seconds then
//  4 ^ 2 = 16 seconds then
//  5 ^ 2 = 32 seconds
Policy
  .Handle<DivideByZeroException>()
  .WaitAndRetry(5, retryAttempt => 
	TimeSpan.FromSeconds(Math.Pow(2, retryAttempt) 
  );
```

### Retry a specified number of times passing a function to calculate the interval to wait between retries based on the current retry attempt and calling an action on each retry with the current exception and timespan
```csharp
Policy
  .Handle<DivideByZeroException>()
  .WaitAndRetry(
    5, 
    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), 
    (exception, timeSpan) => {
      // do something
    }
  );
```

### Circuit Breaker, break circuit after the specified number of exceptions and keep circuit broken for the specified duration
```csharp
Policy
  .Handle<DivideByZeroException>()
  .CircuitBreaker(2, TimeSpan.FromMinutes(1));
```

## Step 3 : Execute the policy

### Execute an action
```csharp
var policy = Policy
              .Handle<DivideByZeroException>()
              .Retry();

policy.Execute(() => DoSomething());
```

### Execute a function returning a result
```csharp
var policy = Policy
              .Handle<DivideByZeroException>()
              .Retry();

var result = policy.Execute(() => DoSomething());
```

### You can of course chain it all together
```csharp
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
Acknowledgements
=

* [lokad-shared-libraries](https://github.com/Lokad/lokad-shared-libraries) - Helper assemblies for .NET 3.5 and Silverlight 2.0 that are being developed as part of the Open Source effort by Lokad.com (discontinued) | [New BSD License](https://raw.github.com/Lokad/lokad-shared-libraries/master/Lokad.Shared.License.txt)

License
=
Licensed under the terms of the [New BSD License](http://opensource.org/licenses/BSD-3-Clause)