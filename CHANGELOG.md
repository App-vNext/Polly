2.2.6
=
----------
- Async sleep fix, plus added continueOnCapturedContext parameter on async methods to control whether continuation and retry will run on captured synchronization context - Thanks to [@yevhen](https://github.com/yevhen)

2.2.5
=
----------
- Policies with a retry count of zero are now allowed - Thanks to [@nelsonghezzi](https://github.com/nelsonghezzi)

2.2.4
=
----------
- Add .NET Core support

2.2.3
=
----------
- Fix PCL implementation of `SystemClock.Reset`
- Added ability to capture the results of executing a policy via `ExecuteAndCapture` - Thanks to [@ThomasMentzel](https://github.com/ThomasMentzel)

2.2.2
=
----------
- Added extra `NotOnCapturedContext` call to prevent potential deadlocks when blocking on asynchronous calls - Thanks to [Hacko](https://github.com/hacko-bede)

2.2.1
=
----------
- Replaced non-blocking sleep implementation with a blocking one for PCL
       
2.2.0
=
----------
- Added Async Support (PCL)
- PCL Profile updated from Profile78 ->  Profile 259
- Added missing WaitAndRetryAsync overload

2.1.0
=
----------
- Added Async Support (.NET Framework 4.5 Only) - Massive thanks to  [@mauricedb](https://github.com/mauricedb) for the implementation

2.0.0
=
----------
- Added Portable Class Library ([Issue #4](https://github.com/michael-wolfenden/Polly/issues/4)) - Thanks to  [@ghuntley](https://github.com/ghuntley) for the implementation
- The `Polly` NuGet package is now no longer strongly named. The strongly named NuGet package is now `Polly-Signed` ([Issue #5](https://github.com/michael-wolfenden/Polly/issues/5)) 

1.1.0
=
----------
- Added additional overloads to Retry
- Allow arbitrary data to be passed to policy execution ([Issue #1](https://github.com/michael-wolfenden/Polly/issues/1)) 
