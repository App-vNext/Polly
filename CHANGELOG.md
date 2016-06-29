## 4.3.0

- Added ability for policies to handle returned results.  Optimised circuit-breaker hot path.  Fixed circuit-breaker threshold bug.  Thanks to [@reisenberger](https://github.com/reisenberger), [@christopherbahr](https://github.com/christopherbahr) and [@Finity](https://github.com/Finity) respectively.

## 4.2.4

- Added overloads to WaitAndRetry and WaitAndRetryAsync methods that accept an onRetry delegate which includes the attempt count.  Thanks to [@SteveCote](https://github.com/steveCote) 
     
## 4.2.3

- Updated the Polly.Net40Async NuGet package to enable async via the SUPPORTSASYNC constant. Cleaned up the build scripts in order to ensure unnecessary DLL references are not included within each of the framework targets.  Thanks to [@reisenberger](https://github.com/reisenberger) and [@joelhulen](https://github.com/joelhulen)

## 4.2.2

- Add new Polly.Net40Async project/package supporting async for .NET40 via Microsoft.Bcl.Async.  Thanks to [@Lumirris](https://github.com/Lumirris)


## 4.2.1

- Allowed async onRetry delegates to async retry policies.  Thanks to [@reisenberger](https://github.com/reisenberger)


## 4.2.0

- Add AdvancedCircuitBreaker.  Thanks to [@reisenberger](https://github.com/reisenberger) and [@kristianhald](https://github.com/kristianhald)


## 4.1.2

- Fixed an issue with the onReset delegate of the CircuitBreaker.


## 4.1.1

- Add ExecuteAndCapture support with arbitrary context data - Thanks to [@reisenberger](https://github.com/reisenberger)


## 4.1.0

- Add Wait and retry forever policy - Thanks to [@nedstoyanov](https://github.com/nedstoyanov)
- Remove time-limit on CircuitBreaker state-change delegates - Thanks to [@reisenberger](https://github.com/reisenberger)


## 4.0.0

- Add async support and circuit-breaker support for ContextualPolicy
- Add manual control of circuit-breaker (reset and manual circuit isolation)
- Add public reporting of circuit-breaker state, for health/performance monitoring
- Add delegates on changes of circuit state
- Thanks to [@reisenberger](https://github.com/reisenberger)


## 3.0.0

- Add cancellation support for all async Policy execution - Thanks to [@reisenberger](https://github.com/reisenberger)


## 2.2.7

- Fixes an issue where continueOnCapturedContext needed to be specified in two places (on action execution and Policy configuration), when wanting to flow async action execution on the captured context - Thanks to [@reisenberger](https://github.com/reisenberger)
- Fixes excess line ending issues

## 2.2.6

- Async sleep fix, plus added continueOnCapturedContext parameter on async methods to control whether continuation and retry will run on captured synchronization context - Thanks to [@yevhen](https://github.com/yevhen)

## 2.2.5

- Policies with a retry count of zero are now allowed - Thanks to [@nelsonghezzi](https://github.com/nelsonghezzi)

## 2.2.4

- Add .NET Core support

## 2.2.3

- Fix PCL implementation of `SystemClock.Reset`
- Added ability to capture the results of executing a policy via `ExecuteAndCapture` - Thanks to [@ThomasMentzel](https://github.com/ThomasMentzel)

## 2.2.2

- Added extra `NotOnCapturedContext` call to prevent potential deadlocks when blocking on asynchronous calls - Thanks to [Hacko](https://github.com/hacko-bede)

## 2.2.1

- Replaced non-blocking sleep implementation with a blocking one for PCL
       
## 2.2.0

- Added Async Support (PCL)
- PCL Profile updated from Profile78 ->  Profile 259
- Added missing WaitAndRetryAsync overload

## 2.1.0

- Added Async Support (.NET Framework 4.5 Only) - Massive thanks to  [@mauricedb](https://github.com/mauricedb) for the implementation

## 2.0.0

- Added Portable Class Library ([Issue #4](https://github.com/michael-wolfenden/Polly/issues/4)) - Thanks to  [@ghuntley](https://github.com/ghuntley) for the implementation
- The `Polly` NuGet package is now no longer strongly named. The strongly named NuGet package is now `Polly-Signed` ([Issue #5](https://github.com/michael-wolfenden/Polly/issues/5)) 

## 1.1.0

- Added additional overloads to Retry
- Allow arbitrary data to be passed to policy execution ([Issue #1](https://github.com/michael-wolfenden/Polly/issues/1)) 
