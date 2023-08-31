## 8.0.0-alpha.9

* Updates for alpha.8 by [@martincostello](https://github.com/martincostello) in https://github.com/App-vNext/Polly/pull/1465
* Fix unstable build by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1466
* Improve samples by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1467
* Specify DebuggerDisplay for ReactiveResilienceStrategyBridge by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1468
* Drop the `Extensions` from `Polly.Extensions` namespace by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1469
* Remove Moq by [@martincostello](https://github.com/martincostello) in https://github.com/App-vNext/Polly/pull/1472
* Add new metering tests to cover uncovered lines by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1474
* Default names for individual resilience strategies by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1475
* Introduce `NonReactiveResilienceStrategy` by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1476
* Drop `TelemetryResilienceStrategy` by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1482
* API Review feedback (1) by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1484
* Rename ResilienceStrategy to ResiliencePipeline by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1483
* API Review Feedback (2) by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1485
* Introduce TelemetryListener by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1486
* Improve documentation by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1487
* Fix metering tests by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1488
* Hide validation APIs by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1490
* Logging improvements by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1489
* Hide/drop some unused APIs by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1491
* Cleanup internals by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1492
* ResilienceContextPool improvements by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1493
* Hide IsSynchronous property by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1494
* Drop unused ResiliencePipelineRegistry APIs by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1495
* `ResiliencePipelineRegistry` is now disposable by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1496
* Move pipeline-related internals into `Pipeline` folder by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1497
* Update benchmarks by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1503
* Minor ResiliencePipelineRegistry cleanup of internals by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1505
* API Review Feedback by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1506
* Minor API cleanup by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1508
* Clenaup rate limiter API by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1509
* Cleanup ResiliencePipelineRegistry internals by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1510
* Allow to dispose linked resources on pipeline disposal by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1511
* Simplify and enhance the pipeline reloads by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1512
* Drop `OutcomeArguments` struct by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1513
* API Review Feedback by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1520
* Got rid of some warnings in the Polly project by [@IgorIgorevich94](https://github.com/IgorIgorevich94) in https://github.com/App-vNext/Polly/pull/1514
* API Review Feedback by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1521
* Cleanup Outcome internals and drop unused hedging and fallbacks APIs by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1523
* Improve debugging experience for `ResilienceProperties` by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1524
* Protect against retry delay overflows by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1522
* Fix DelayAsync extension by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1525

## 8.0.0-alpha.8

* Updates for 8.0.0-alpha.7 by [@martincostello](https://github.com/martincostello) in https://github.com/App-vNext/Polly/pull/1433
* Improve logging messages by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1436
* Rename `BuilderName` to `Name` by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1437
* Do not record null tags to meter by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1438
* Fix telemetry test failures by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1439
* Simplify OutcomeResilienceStrategy by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1440
* Drop simple circuit breaker by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1444
* Allow jitter for all backoff types by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1445
* Rename `Attempt` to `AttemptNumber` by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1447
* Hide `CircuitBreakerStateProvider.LastHandledOutcome` by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1446
* Rename `ResilienceStrategyBuilder` to `CompositeStrategyBuilder` by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1448
* Added `readonly` modifier to private fields which are never changed by [@Lehonti](https://github.com/Lehonti) in https://github.com/App-vNext/Polly/pull/1451
* Reduced nesting levels through block-scoped `using`s and the inversion of an `if` block. by [@Lehonti](https://github.com/Lehonti) in https://github.com/App-vNext/Polly/pull/1453
* Simplify file names by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1455
* Improve `MultipleStrategiesBenchmark` by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1457
* Elaborate about synchronous vs asynchronous executions by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1456
* Fix some typos in XML docs by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1458
* Make `ReactiveResilienceStrategy` public by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1460
* Make the `ReactiveResilienceStrategy` type-safe by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1462
* Use `StrategyHelper` for safe executions and drop redundant methods by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1463
* Drop unnecessary allocation in circuit breaker by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1464

## 8.0.0-alpha.7

* Introduce ResilienceStrategyBuilder.Validator by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1412
* Update docs by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1413
* Annotate the library with trimming attributes by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1414
* Update trimming justifications by [@martincostello](https://github.com/martincostello) in https://github.com/App-vNext/Polly/pull/1415
* Fix condition by [@martincostello](https://github.com/martincostello) in https://github.com/App-vNext/Polly/pull/1416
* API Review Feedback (1) by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1420
* Introduce ResilienceContextPool (ApiReview) by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1421
* Convert records to classes by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1422
* Rename ExecuteCoreAsync to ExecuteCore by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1424
* Benchmark for strategy creation by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1426
* Fix README.md example syntax errors by [@Sensational-Code](https://github.com/Sensational-Code) in https://github.com/App-vNext/Polly/pull/1427
* Improve usability of ResilienceStrategy&lt;T&gt; by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1428
* Cleanup OutcomeResilienceStrategy by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1430
* Drop the ResilienceStrategyBuilder.IsGenericBuilder property by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1431
* Allow isolate CircuitBreakerManualControl using constructor by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1432

## 8.0.0-alpha.6

* Update docs by [@martincostello](https://github.com/martincostello) in https://github.com/App-vNext/Polly/pull/1382
* Add support for `PartitionedRateLimiter` by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1383
* Fix debugger proxies by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1384
* Allow adding generic strategies to generic builder by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1386
* Add new issue that demonstrates how to use PartitionedRateLimiter by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1385
* Introduce `TelemetryOptions.OnTelemetryEvent` by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1387
* `ResilienceStrategyRegistry` API improvements by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1388
* Simplify condition by [@martincostello](https://github.com/martincostello) in https://github.com/App-vNext/Polly/pull/1391
* Introduce `ResilienceStrategyBuilder.InstanceName` and use it in telemetry by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1392
* Introduce `Polly.Testing` package by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1394
* Kill mutant by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1395
* Fix unstable test by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1396
* Rename  AddResilienceStrategy to AddResilienceStrategyRegistry by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1397
* Update README.md for Polly.Extensions with telemetry info by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1401
* Kill mutant by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1407
* Assertion failed when running tests in Visual Studio by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1408
* Include PublicApiAnalyzers by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1400
* Kill mutant by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1409
* Demonstrate how to create dynamic strategies with complex keys by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1366

## 8.0.0-alpha.5

* Skip mutation tests for tagged builds by [@martincostello](https://github.com/martincostello) in https://github.com/App-vNext/Polly/pull/1354
* Update CHANGELOG by [@martincostello](https://github.com/martincostello) in https://github.com/App-vNext/Polly/pull/1353
* Drop TimeProvider.Delay by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1355
* Fix race conditions in tests by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1358
* Simplify the logging by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1359
* Prepare for .NET 8 by [@martincostello](https://github.com/martincostello) in https://github.com/App-vNext/Polly/pull/1360
* Introduce ResilienceEventSeverity by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1361
* Upload coverage reports by [@martincostello](https://github.com/martincostello) in https://github.com/App-vNext/Polly/pull/1363
* Kill mutant by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1368
* Accelerate build in VS by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1369
* Simplify handling of reloads by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1374
* Allow reusing CircuitBreakerManualControl across multiple CBs by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1375
* PR and issue automation by [@martincostello](https://github.com/martincostello) in https://github.com/App-vNext/Polly/pull/1370
* Exclude some labels from stale by [@martincostello](https://github.com/martincostello) in https://github.com/App-vNext/Polly/pull/1378
* Debugger proxies for resilience strategies by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1379
* Introduce `ResilienceContext.OperationKey` by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1380

## 8.0.0-alpha.4

* Rename FakeTimeProvider by [@martincostello](https://github.com/martincostello) in https://github.com/App-vNext/Polly/pull/1349
* Adopt FakeTimeProvider by [@martincostello](https://github.com/martincostello) in https://github.com/App-vNext/Polly/pull/1350
* Drop custom validation attributes by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1351
* Drop TimeProvider.CancelAfter by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1352

## 8.0.0-alpha.3

* Update README.md with v8 Alpha v2 release announcement by [@joelhulen](https://github.com/joelhulen) in https://github.com/App-vNext/Polly/pull/1337
* Adopt Alpha 2 in samples by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1338
* Add net6.0 and net7.0 to Polly by [@martincostello](https://github.com/martincostello) in https://github.com/App-vNext/Polly/pull/1336
* Sync TimeProvider and cleanup by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1339
* Fix hedging being cancelled too early by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1340
* Fix logging of execution attempt by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1341
* Bump github/codeql-action from 2.20.0 to 2.20.1 by [@dependabot](https://github.com/dependabot) in https://github.com/App-vNext/Polly/pull/1344
* Bump martincostello/update-dotnet-sdk from 2.2.2 to 2.2.3 by [@dependabot](https://github.com/dependabot) in https://github.com/App-vNext/Polly/pull/1345
* Bump StyleCop.Analyzers from 1.2.0-beta.435 to 1.2.0-beta.507 by [@dependabot](https://github.com/dependabot) in https://github.com/App-vNext/Polly/pull/1342
* Bump SonarAnalyzer.CSharp from 9.3.0.71466 to 9.4.0.72892 by [@dependabot](https://github.com/dependabot) in https://github.com/App-vNext/Polly/pull/1343
* Expose Randomizer property and use it in retry strategy by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1346

## 8.0.0-alpha.2

- Drop redundant validation of resilience strategy options by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1299
- Add NuGet configuration file by [@martincostello](https://github.com/martincostello) in https://github.com/App-vNext/Polly/pull/1305
- Log unhealthy executions with warning level by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1306
- Add hedging to package tags by [@martincostello](https://github.com/martincostello) in https://github.com/App-vNext/Polly/pull/1307
- Bump github/codeql-action from 2.3.6 to 2.20.0 by [@dependabot](https://github.com/dependabot) in https://github.com/App-vNext/Polly/pull/1310
- Bump actions/checkout from 3.5.2 to 3.5.3 by [@dependabot](https://github.com/dependabot) in https://github.com/App-vNext/Polly/pull/1309
- Introduce `samples` folder by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1295
- Update telemetry benchmark by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1311
- Enhance `OnHedgingArguments` by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1314
- The options that handle outcomes now have sensible defaults by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1316
- Update README.md with v8 alpha announcement by [@joelhulen](https://github.com/joelhulen) in https://github.com/App-vNext/Polly/pull/1317
- The deafult RateLimiterStrategyOptions instance is now valid by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1315
- Rename `TelemetryResilienceStrategyOptions` to `TelemetryOptions` by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1318
- Alpha fixes and improvements by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1319
- Fix OnHedging not being called by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1320
- Reduce allocations in telemetry by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1321
- Add new issue test that demonstrates library scenario by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1322
- Fix relative links by [@martincostello](https://github.com/martincostello) in https://github.com/App-vNext/Polly/pull/1325
- API feedback by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1327
- Introduce ExecutionAttemptArguments by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1326
- Introduce OutcomeResilienceStrategy and drop some internals by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1330
- Introduce Outcome by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1331
- Validate NuGet package signatures by [@martincostello](https://github.com/martincostello) in https://github.com/App-vNext/Polly/pull/1335
- Allow implicit conversion of `PredicateBuilder` to delegates by [@martintmk](https://github.com/martintmk) in https://github.com/App-vNext/Polly/pull/1332

## 8.0.0-alpha.1

- The first public preview of [Polly v8](https://github.com/App-vNext/Polly/issues/1048) with our [new high-performance core API](https://github.com/App-vNext/Polly/blob/main/src/Polly.Core/README.md) and extensions. Feel free to check out the [samples](samples/) to see the new and improved Polly V8 in action.
- The first release of the new NuGet packages:
  - [`Polly.Core`](https://nuget.org/packages/Polly.Core) - This package contains the new Polly V8 API.
  - [`Polly.Extensions`](https://nuget.org/packages/Polly.Extensions) - This package is designed to integrate Polly with dependency injection and enable telemetry.
  - [`Polly.RateLimiting`](https://nuget.org/packages/Polly.RateLimiting) - This package provides an integration between Polly and [`System.Threading.RateLimiting`](https://www.nuget.org/packages/System.Threading.RateLimiting/).

Thanks to:
  - [@adamnova](https://github.com/adamnova)
  - [@andrey-noskov](https://github.com/andrey-noskov)
  - [@joelhulen](https://github.com/joelhulen)
  - [@juraj-blazek](https://github.com/juraj-blazek)
  - [@geeknoid](https://github.com/geeknoid)
  - [@laura-mi](https://github.com/laura-mi)
  - [@martincostello](https://github.com/martincostello)
  - [@martintmk](https://github.com/martintmk)
  - [@SimonCropp](https://github.com/SimonCropp)
  - [@tekian](https://github.com/tekian)
  - [@terrajobst](https://github.com/terrajobst)

## 7.2.4

- Fixed an incorrect exception argument - Thanks to [@FoxTes](https://github.com/FoxTes)
- Upgrade FluentAssertions - Thanks to [@dotnetspark](https://github.com/dotnetspark)
- Upgrade Cake - Thanks to [@eugeneogongo](https://github.com/eugeneogongo)
- Fixed possible NullReferenceException - Thanks to [@FoxTes](https://github.com/FoxTes)
- Migrate build to GitHub Actions - Thanks to [@martincostello](https://github.com/martincostello)
- Authenticode sign the assembly and NuGet package - Thanks to [@martincostello](https://github.com/martincostello) and the .NET Foundation

## 7.2.3

- Add RateLimit policy - Thanks to [@reisenberger](https://github.com/reisenberger)
- Various codebase health updates - Thanks to [@SimonCropp](https://github.com/SimonCropp)
- Add benchmarks project - Thanks to [@martincostello](https://github.com/martincostello)
- Fix broken README image - Thanks to [@GitHubPang](https://github.com/GitHubPang)

## 7.2.2

- Recursively search all `AggregateException` inner exceptions for predicate matches when using `HandleInner()` ([#818](https://github.com/App-vNext/Polly/issues/818)) - Thanks to [@sideproject](https://github.com/sideproject)
- Polly now builds deterministically - Thanks to [@304NotModified](https://github.com/304NotModified)
- Bug fix: the `timeoutStrategy` parameter was not being used by the `TimeoutAsync(Func<TimeSpan>, TimeoutStrategy, Func<Context, TimeSpan, Task, Task>)` method - Thanks to [@martincostello](https://github.com/martincostello)
- Bug fix: the solution can now be built with the .NET 5.0 SDK - Thanks to [@martincostello](https://github.com/martincostello)

## 7.2.1
- Upgrade SourceLink to RTM v1 (fixes building from source for latest .NET Core 3.1.x)
- Bug fix: rare circuit-breaker race condition causing NullReferenceException when circuit throws BrokenCircuitException.

## 7.2.0
- Add test target for netcoreapp3.0.
- Extend PolicyRegistry with concurrent method support, TryAdd, TryRemove, TryUpdate, GetOrAdd, AddOrUpdate; new interface IConcurrentPolicyRegistry
- Improve .NET Framework support: Add explicit targets for .NET Framework 4.6.1 and 4.7.2.
- TimeoutPolicy: if timeout occurs while a user exception is being marshalled (edge case race condition), do not mask user exception (fix issue 620)
- Enhance debugging/stacktrace experience for some contexts: Include pdb symbols in package again.

## 7.1.1
- Bug fix: ensure async retry policies honor continueOnCapturedContext setting (affected v7.1.0 only).
- Remove deprecated cake add-in from build

## 7.1.0
- Add SourceLink debugger support.
- Bug fix: PolicyRegistry with .NET Core services.AddPolicyRegistry() overload (affected Polly v7.0.1-3 only)
- Rationalise solution layout
- Add explicit .NET framework 4.6.2 and 4.7.2 test runs

## 7.0.3
- Bug fix for AdvancedCircuitBreakerAsync<TResult> (issue affecting v7.0.1-2 only)

## 7.0.2
- Bug fix for PolicyRegistry (issue affecting v7.0.1 only)

## 7.0.1
- Clarify separation of sync and async policies (breaking change)
- Enable extensibility by custom policies hosted external to Polly
- Enable collection initialization syntax for PolicyRegistry
- Enable cache policies to cache default(TResult) (breaking change)
- Restore Exception binary serialization for .Net Standard 2.0

## 6.1.2
- Bug Fix: Async continuation control for async executions (issue 540, affected only v6.1.1)

## 6.1.1
- Bug Fix: Context.PolicyKey behaviour in PolicyWrap (issue 510)

## 6.1.0
- Bug Fix: Context.PolicyKey behaviour in PolicyWrap (issue 463)
- Bug Fix: CachePolicy behaviour with non-nullable types (issues 472, 475)
- Enhancement: WaitAnd/RetryForever overloads where onRetry takes the retry number as a parameter (issue 451)
- Enhancement: Overloads where onTimeout takes thrown exception as a parameter (issue 338)
- Enhancement: Improved cache error message (issue 455)

## 6.0.1
- Version 6 RTM, for integration to ASPNET Core 2.1 IHttpClientFactory

## 6.0.0-v6alpha
- Publish as strong-named package only (discontinue non-strong-named versions)
- Add .NetStandard 2.0 tfm
- Provide .NET4.5 support via .NetStandard 1.1 tfm
- Discontinue .NET4.0 support
- Remove methods marked as deprecated in v5.9.0

## 5.9.0
- Allow Timeout.InfiniteTimeSpan (no timeout) for TimeoutPolicy.
- Add .AsPolicy&lt;TResult&gt; and .AsAsyncPolicy&lt;TResult&gt; methods for converting non-generic policies to generic policies.
- Per Semver, indicates deprecation of overloads and properties intended to be removed or renamed in Polly v6.

## 5.8.0
- Add a new onBreak overload that provides the prior state on a transition to an open state
- Bug fix: RelativeTtl in CachePolicy now always returns a ttl relative to time item is cached

## 5.7.0
- Minor cache fixes
- Add ability to calculate cache Ttl based on item to cache
- Allow user-created custom policies

## 5.6.1
- Extend PolicyWrap syntax with interfaces

## 5.6.0
- Add ability to handle inner exceptions natively: .HandleInner&lt;TEx&gt;()
- Allow WaitAndRetry policies to calculate wait based on the handled fault
- Add the ability to access the policies within an IPolicyWrap
- Allow PolicyWrap to configure policies expressed as interfaces
- Bug fix: set context keys for generic execute methods with PolicyWrap
- Bug fix: generic TResult method with non-generic fallback policy
- Performance improvements
- Multiple build speed improvements

## 5.5.0
- Bug fix: non-generic CachePolicy with PolicyWrap
- Add Cache interfaces

## 5.4.0
- Add CachePolicy: cache-aside pattern, with interfaces for pluggable cache providers and serializers.
- Bug fix: Sync TimeoutPolicy in pessimistic mode no longer interposes AggregateException.
- Provide public factory methods for PolicyResult, to support testing.
- Fallback delegates can now take handled fault as input parameter.

## 5.3.1
- Make ISyncPolicy<TResult> public
- (Upgrade solution to msbuild15)

## 5.3.0
- Fix ExecuteAndCapture() usage with PolicyWrap
- Allow Fallback delegates to take execution Context
- Provide IReadOnlyPolicyRegistry interface

## 5.2.0
- Add PolicyRegistry for storing and retrieving policies.
- Add interfaces by policy type and execution type.
- Change .NetStandard minimum support to NetStandard1.1.

## 5.1.0
- Allow different parts of a policy execution to exchange data via a mutable Context travelling with each execution.

## 5.0.6
- Update NETStandard.Library dependency to latest 1.6.1 for .NetStandard1.0 target.  Resolves compatibility for some Xamarin targets.

## 5.0.5
- Bug fix: Prevent request stampede during half-open state of CircuitBreaker and AdvancedCircuitBreaker.  Enforce only one new trial call per break duration, during half-open state.
- Bug fix: Prevent duplicate raising of the onBreak delegate, if executions started when a circuit was closed, return faults when a circuit has already opened.
- Optimisation: Optimise hotpaths for Circuit-Breaker, Retry and Fallback policies.
- Minor behavioural change: For a circuit which has not handled any faults since initialisation or last reset, make `LastException` property return null rather than a fake exception.
- Add NoOpPolicy: NoOpPolicy executes delegates without intervention; for eg stubbing out Polly in unit testing.

## 5.0.4 pre
- Fix Microsoft.Bcl and Nito.AsyncEx dependencies for Polly.Net40Async.

## 5.0.3 RTM
- Refine implementation of cancellable synchronous WaitAndRetry
- Minor breaking change: Where a user delegate does not observe cancellation, Polly will now honour the delegate's outcome rather than throw for the unobserved cancellation (issue 188).

## 5.0.2 alpha

- .NETStandard1.0 target: Correctly state dependencies.
- .NETStandard1.0 target: Fix SemVer stamping of Polly.dll.
- PCL259 project and target: Remove, in favour of .NETStandard1.0 target.  PCL259 is supported via .NETStandard1.0 target, going forward.
- Mark Polly.dll as CLSCompliant.
- Tidy build around GitVersionTask and ReferenceGenerator.
- Update FluentAssertions dependency.
- Added Polly.Net40Async specs project.
- Fix issue 179: Make Net4.0 async implementation for Bulkhead truly async.

## 5.0.1 alpha

- Add .NET Standard 1.0 project and target.

## 5.0.0 alpha

A major release, adding significant new resilience policies:

- Timeout policy: allows timing out any execution. Thanks to [@reisenberger](https://github.com/reisenberger).
- Bulkhead isolation policy: limits the resources consumable by governed actions, such that a faulting channel cannot cause cascading failures. Thanks to [@reisenberger](https://github.com/reisenberger) and contributions from [@brunolauze](https://github.com/brunolauze).
- Fallback policy: provides for a fallback execution or value, in case of overall failure. Thanks to [@reisenberger](https://github.com/reisenberger)
- PolicyWrap: allows flexibly combining Policy instances of any type, to form an overall resilience strategy. Thanks to [@reisenberger](https://github.com/reisenberger)

Other changes include:

- Add PolicyKeys and context to all policy executions, for logging and to support later introduction of policy events and metrics. Thanks to [@reisenberger](https://github.com/reisenberger)
- Add CancellationToken support to synchronous executions.  Thanks to [@brunolauze](https://github.com/brunolauze) and [@reisenberger](https://github.com/reisenberger)
- Add some missing ExecuteAndCapture/Async overloads. Thanks to [@reisenberger](https://github.com/reisenberger)
- Remove invalid ExecuteAsync overloads taking (but not making use of) a CancellationToken
- Provide .NET4.0 support uniquely through Polly.NET40Async package
- Retire ContextualPolicy (not part of documented API; support now in Policy base class)
- Discontinue .NET3.5 support

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
