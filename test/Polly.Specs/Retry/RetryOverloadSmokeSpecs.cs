namespace Polly.Specs.Retry;

public class RetryOverloadSmokeSpecs
{
    [Fact]
    public void Result_retry_overloads_should_retry_successfully()
    {
        var executions = new Action[]
        {
            () => ExecuteSequence(ResultBuilder().Retry(), ResultPrimitive.Fault, ResultPrimitive.Good).ShouldBe(ResultPrimitive.Good),
            () => ExecuteSequence(ResultBuilder().Retry(1), ResultPrimitive.Fault, ResultPrimitive.Good).ShouldBe(ResultPrimitive.Good),
            () =>
            {
                int seenRetryCount = 0;
                ExecuteSequence(ResultBuilder().Retry((_, retryCount) => seenRetryCount = retryCount), ResultPrimitive.Fault, ResultPrimitive.Good)
                    .ShouldBe(ResultPrimitive.Good);
                seenRetryCount.ShouldBe(1);
            },
            () =>
            {
                int seenRetryCount = 0;
                ExecuteSequence(ResultBuilder().Retry(1, (_, retryCount) => seenRetryCount = retryCount), ResultPrimitive.Fault, ResultPrimitive.Good)
                    .ShouldBe(ResultPrimitive.Good);
                seenRetryCount.ShouldBe(1);
            },
            () =>
            {
                Context? seenContext = null;
                ExecuteSequence(ResultBuilder().Retry((_, retryCount, context) =>
                {
                    retryCount.ShouldBe(1);
                    seenContext = context;
                }), ResultPrimitive.Fault, ResultPrimitive.Good).ShouldBe(ResultPrimitive.Good);
                seenContext.ShouldNotBeNull();
            },
            () =>
            {
                Context? seenContext = null;
                ExecuteSequence(ResultBuilder().Retry(1, (_, retryCount, context) =>
                {
                    retryCount.ShouldBe(1);
                    seenContext = context;
                }), ResultPrimitive.Fault, ResultPrimitive.Good).ShouldBe(ResultPrimitive.Good);
                seenContext.ShouldNotBeNull();
            },
            () => ExecuteSequence(ResultBuilder().RetryForever(), ResultPrimitive.Fault, ResultPrimitive.Good).ShouldBe(ResultPrimitive.Good),
            () =>
            {
                DelegateResult<ResultPrimitive>? outcome = null;
                ExecuteSequence(ResultBuilder().RetryForever(result => outcome = result), ResultPrimitive.Fault, ResultPrimitive.Good)
                    .ShouldBe(ResultPrimitive.Good);
                outcome.ShouldNotBeNull();
                outcome!.Result.ShouldBe(ResultPrimitive.Fault);
            },
            () =>
            {
                int seenRetryCount = 0;
                ExecuteSequence(ResultBuilder().RetryForever((_, retryCount) => seenRetryCount = retryCount), ResultPrimitive.Fault, ResultPrimitive.Good)
                    .ShouldBe(ResultPrimitive.Good);
                seenRetryCount.ShouldBe(1);
            },
            () =>
            {
                Context? seenContext = null;
                ExecuteSequence(ResultBuilder().RetryForever((_, context) => seenContext = context), ResultPrimitive.Fault, ResultPrimitive.Good)
                    .ShouldBe(ResultPrimitive.Good);
                seenContext.ShouldNotBeNull();
            },
            () =>
            {
                Context? seenContext = null;
                ExecuteSequence(ResultBuilder().RetryForever((_, retryCount, context) =>
                {
                    retryCount.ShouldBe(1);
                    seenContext = context;
                }), ResultPrimitive.Fault, ResultPrimitive.Good).ShouldBe(ResultPrimitive.Good);
                seenContext.ShouldNotBeNull();
            }
        };

        foreach (Action execution in executions)
        {
            execution();
        }
    }

    [Fact]
    public void Result_wait_and_retry_overloads_should_retry_successfully()
    {
        TimeSpan[] sleepDurations = [TimeSpan.Zero];

        var executions = new Action[]
        {
            () => ExecuteSequence(ResultBuilder().WaitAndRetry(1, _ => TimeSpan.Zero), ResultPrimitive.Fault, ResultPrimitive.Good).ShouldBe(ResultPrimitive.Good),
            () => ExecuteSequence(ResultBuilder().WaitAndRetry(1, _ => TimeSpan.Zero, (_, delay) => delay.ShouldBe(TimeSpan.Zero)), ResultPrimitive.Fault, ResultPrimitive.Good).ShouldBe(ResultPrimitive.Good),
            () => ExecuteSequence(ResultBuilder().WaitAndRetry(1, _ => TimeSpan.Zero, (_, delay, context) =>
            {
                delay.ShouldBe(TimeSpan.Zero);
                context.OperationKey.ShouldBe("operation-key");
            }), ResultPrimitive.Fault, ResultPrimitive.Good).ShouldBe(ResultPrimitive.Good),
            () => ExecuteSequence(ResultBuilder().WaitAndRetry(1, _ => TimeSpan.Zero, (_, delay, retryCount, context) =>
            {
                delay.ShouldBe(TimeSpan.Zero);
                retryCount.ShouldBe(1);
                context.OperationKey.ShouldBe("operation-key");
            }), ResultPrimitive.Fault, ResultPrimitive.Good).ShouldBe(ResultPrimitive.Good),
            () => ExecuteSequence(ResultBuilder().WaitAndRetry(1, (_, context) =>
            {
                context.OperationKey.ShouldBe("operation-key");
                return TimeSpan.Zero;
            }), ResultPrimitive.Fault, ResultPrimitive.Good).ShouldBe(ResultPrimitive.Good),
            () => ExecuteSequence(ResultBuilder().WaitAndRetry(1, (_, _) => TimeSpan.Zero, (_, delay, context) =>
            {
                delay.ShouldBe(TimeSpan.Zero);
                context.OperationKey.ShouldBe("operation-key");
            }), ResultPrimitive.Fault, ResultPrimitive.Good).ShouldBe(ResultPrimitive.Good),
            () => ExecuteSequence(ResultBuilder().WaitAndRetry(1, (_, _) => TimeSpan.Zero, (_, delay, retryCount, context) =>
            {
                delay.ShouldBe(TimeSpan.Zero);
                retryCount.ShouldBe(1);
                context.OperationKey.ShouldBe("operation-key");
            }), ResultPrimitive.Fault, ResultPrimitive.Good).ShouldBe(ResultPrimitive.Good),
            () => ExecuteSequence(ResultBuilder().WaitAndRetry(1, (_, outcome, context) =>
            {
                outcome.Result.ShouldBe(ResultPrimitive.Fault);
                context.OperationKey.ShouldBe("operation-key");
                return TimeSpan.Zero;
            }), ResultPrimitive.Fault, ResultPrimitive.Good).ShouldBe(ResultPrimitive.Good),
            () => ExecuteSequence(ResultBuilder().WaitAndRetry(1, (_, _, _) => TimeSpan.Zero, (outcome, delay, context) =>
            {
                outcome.Result.ShouldBe(ResultPrimitive.Fault);
                delay.ShouldBe(TimeSpan.Zero);
                context.OperationKey.ShouldBe("operation-key");
            }), ResultPrimitive.Fault, ResultPrimitive.Good).ShouldBe(ResultPrimitive.Good),
            () => ExecuteSequence(ResultBuilder().WaitAndRetry(1, (_, _, _) => TimeSpan.Zero, (outcome, delay, retryCount, context) =>
            {
                outcome.Result.ShouldBe(ResultPrimitive.Fault);
                delay.ShouldBe(TimeSpan.Zero);
                retryCount.ShouldBe(1);
                context.OperationKey.ShouldBe("operation-key");
            }), ResultPrimitive.Fault, ResultPrimitive.Good).ShouldBe(ResultPrimitive.Good),
            () => ExecuteSequence(ResultBuilder().WaitAndRetry(sleepDurations), ResultPrimitive.Fault, ResultPrimitive.Good).ShouldBe(ResultPrimitive.Good),
            () => ExecuteSequence(ResultBuilder().WaitAndRetry(sleepDurations, (_, delay) => delay.ShouldBe(TimeSpan.Zero)), ResultPrimitive.Fault, ResultPrimitive.Good).ShouldBe(ResultPrimitive.Good),
            () => ExecuteSequence(ResultBuilder().WaitAndRetry(sleepDurations, (_, delay, context) =>
            {
                delay.ShouldBe(TimeSpan.Zero);
                context.OperationKey.ShouldBe("operation-key");
            }), ResultPrimitive.Fault, ResultPrimitive.Good).ShouldBe(ResultPrimitive.Good),
            () => ExecuteSequence(ResultBuilder().WaitAndRetry(sleepDurations, (_, delay, retryCount, context) =>
            {
                delay.ShouldBe(TimeSpan.Zero);
                retryCount.ShouldBe(1);
                context.OperationKey.ShouldBe("operation-key");
            }), ResultPrimitive.Fault, ResultPrimitive.Good).ShouldBe(ResultPrimitive.Good),
            () => ExecuteSequence(ResultBuilder().WaitAndRetryForever(_ => TimeSpan.Zero), ResultPrimitive.Fault, ResultPrimitive.Good).ShouldBe(ResultPrimitive.Good),
            () => ExecuteSequence(ResultBuilder().WaitAndRetryForever((_, context) =>
            {
                context.OperationKey.ShouldBe("operation-key");
                return TimeSpan.Zero;
            }), ResultPrimitive.Fault, ResultPrimitive.Good).ShouldBe(ResultPrimitive.Good),
            () => ExecuteSequence(ResultBuilder().WaitAndRetryForever(_ => TimeSpan.Zero, (_, delay) => delay.ShouldBe(TimeSpan.Zero)), ResultPrimitive.Fault, ResultPrimitive.Good).ShouldBe(ResultPrimitive.Good),
            () => ExecuteSequence(ResultBuilder().WaitAndRetryForever(_ => TimeSpan.Zero, (_, retryCount, delay) =>
            {
                retryCount.ShouldBe(1);
                delay.ShouldBe(TimeSpan.Zero);
            }), ResultPrimitive.Fault, ResultPrimitive.Good).ShouldBe(ResultPrimitive.Good),
            () => ExecuteSequence(ResultBuilder().WaitAndRetryForever((_, _) => TimeSpan.Zero, (_, delay, context) =>
            {
                delay.ShouldBe(TimeSpan.Zero);
                context.OperationKey.ShouldBe("operation-key");
            }), ResultPrimitive.Fault, ResultPrimitive.Good).ShouldBe(ResultPrimitive.Good),
            () => ExecuteSequence(ResultBuilder().WaitAndRetryForever((_, _) => TimeSpan.Zero, (_, retryCount, delay, context) =>
            {
                retryCount.ShouldBe(1);
                delay.ShouldBe(TimeSpan.Zero);
                context.OperationKey.ShouldBe("operation-key");
            }), ResultPrimitive.Fault, ResultPrimitive.Good).ShouldBe(ResultPrimitive.Good),
            () => ExecuteSequence(ResultBuilder().WaitAndRetryForever((_, outcome, context) =>
            {
                outcome.Result.ShouldBe(ResultPrimitive.Fault);
                context.OperationKey.ShouldBe("operation-key");
                return TimeSpan.Zero;
            }, (_, delay, context) =>
            {
                delay.ShouldBe(TimeSpan.Zero);
                context.OperationKey.ShouldBe("operation-key");
            }), ResultPrimitive.Fault, ResultPrimitive.Good).ShouldBe(ResultPrimitive.Good),
            () => ExecuteSequence(ResultBuilder().WaitAndRetryForever((_, outcome, context) =>
            {
                outcome.Result.ShouldBe(ResultPrimitive.Fault);
                context.OperationKey.ShouldBe("operation-key");
                return TimeSpan.Zero;
            }, (_, retryCount, delay, context) =>
            {
                retryCount.ShouldBe(1);
                delay.ShouldBe(TimeSpan.Zero);
                context.OperationKey.ShouldBe("operation-key");
            }), ResultPrimitive.Fault, ResultPrimitive.Good).ShouldBe(ResultPrimitive.Good)
        };

        foreach (Action execution in executions)
        {
            execution();
        }
    }

    [Fact]
    public async Task Async_result_retry_overloads_should_retry_successfully()
    {
        var executions = new Func<Task>[]
        {
            () => ExecuteSequenceAsync(ResultBuilder().RetryAsync(), ResultPrimitive.Fault, ResultPrimitive.Good).ContinueWith(task => task.Result.ShouldBe(ResultPrimitive.Good)),
            () => ExecuteSequenceAsync(ResultBuilder().RetryAsync(1), ResultPrimitive.Fault, ResultPrimitive.Good).ContinueWith(task => task.Result.ShouldBe(ResultPrimitive.Good)),
            async () =>
            {
                int seenRetryCount = 0;
                (await ExecuteSequenceAsync(ResultBuilder().RetryAsync((_, retryCount) => seenRetryCount = retryCount), ResultPrimitive.Fault, ResultPrimitive.Good))
                    .ShouldBe(ResultPrimitive.Good);
                seenRetryCount.ShouldBe(1);
            },
            async () =>
            {
                int seenRetryCount = 0;
                (await ExecuteSequenceAsync(ResultBuilder().RetryAsync((_, retryCount) =>
                {
                    seenRetryCount = retryCount;
                    return TaskHelper.EmptyTask;
                }), ResultPrimitive.Fault, ResultPrimitive.Good)).ShouldBe(ResultPrimitive.Good);
                seenRetryCount.ShouldBe(1);
            },
            async () =>
            {
                int seenRetryCount = 0;
                (await ExecuteSequenceAsync(ResultBuilder().RetryAsync(1, (_, retryCount) => seenRetryCount = retryCount), ResultPrimitive.Fault, ResultPrimitive.Good))
                    .ShouldBe(ResultPrimitive.Good);
                seenRetryCount.ShouldBe(1);
            },
            async () =>
            {
                int seenRetryCount = 0;
                (await ExecuteSequenceAsync(ResultBuilder().RetryAsync(1, (_, retryCount) =>
                {
                    seenRetryCount = retryCount;
                    return TaskHelper.EmptyTask;
                }), ResultPrimitive.Fault, ResultPrimitive.Good)).ShouldBe(ResultPrimitive.Good);
                seenRetryCount.ShouldBe(1);
            },
            async () =>
            {
                Context? seenContext = null;
                (await ExecuteSequenceAsync(ResultBuilder().RetryAsync((_, retryCount, context) =>
                {
                    retryCount.ShouldBe(1);
                    seenContext = context;
                }), ResultPrimitive.Fault, ResultPrimitive.Good)).ShouldBe(ResultPrimitive.Good);
                seenContext.ShouldNotBeNull();
            },
            async () =>
            {
                Context? seenContext = null;
                (await ExecuteSequenceAsync(ResultBuilder().RetryAsync((_, retryCount, context) =>
                {
                    retryCount.ShouldBe(1);
                    seenContext = context;
                    return TaskHelper.EmptyTask;
                }), ResultPrimitive.Fault, ResultPrimitive.Good)).ShouldBe(ResultPrimitive.Good);
                seenContext.ShouldNotBeNull();
            },
            async () =>
            {
                Context? seenContext = null;
                (await ExecuteSequenceAsync(ResultBuilder().RetryAsync(1, (_, retryCount, context) =>
                {
                    retryCount.ShouldBe(1);
                    seenContext = context;
                }), ResultPrimitive.Fault, ResultPrimitive.Good)).ShouldBe(ResultPrimitive.Good);
                seenContext.ShouldNotBeNull();
            },
            async () =>
            {
                Context? seenContext = null;
                (await ExecuteSequenceAsync(ResultBuilder().RetryAsync(1, (_, retryCount, context) =>
                {
                    retryCount.ShouldBe(1);
                    seenContext = context;
                    return TaskHelper.EmptyTask;
                }), ResultPrimitive.Fault, ResultPrimitive.Good)).ShouldBe(ResultPrimitive.Good);
                seenContext.ShouldNotBeNull();
            },
            async () => (await ExecuteSequenceAsync(ResultBuilder().RetryForeverAsync(), ResultPrimitive.Fault, ResultPrimitive.Good)).ShouldBe(ResultPrimitive.Good),
            async () =>
            {
                DelegateResult<ResultPrimitive>? outcome = null;
                (await ExecuteSequenceAsync(ResultBuilder().RetryForeverAsync(result => outcome = result), ResultPrimitive.Fault, ResultPrimitive.Good))
                    .ShouldBe(ResultPrimitive.Good);
                outcome.ShouldNotBeNull();
            },
            async () =>
            {
                int seenRetryCount = 0;
                (await ExecuteSequenceAsync(ResultBuilder().RetryForeverAsync((_, retryCount) => seenRetryCount = retryCount), ResultPrimitive.Fault, ResultPrimitive.Good))
                    .ShouldBe(ResultPrimitive.Good);
                seenRetryCount.ShouldBe(1);
            },
            async () =>
            {
                Context? seenContext = null;
                (await ExecuteSequenceAsync(ResultBuilder().RetryForeverAsync((_, context) => seenContext = context), ResultPrimitive.Fault, ResultPrimitive.Good))
                    .ShouldBe(ResultPrimitive.Good);
                seenContext.ShouldNotBeNull();
            },
            async () =>
            {
                Context? seenContext = null;
                (await ExecuteSequenceAsync(ResultBuilder().RetryForeverAsync((_, retryCount, context) =>
                {
                    retryCount.ShouldBe(1);
                    seenContext = context;
                }), ResultPrimitive.Fault, ResultPrimitive.Good)).ShouldBe(ResultPrimitive.Good);
                seenContext.ShouldNotBeNull();
            },
            async () =>
            {
                Context? seenContext = null;
                (await ExecuteSequenceAsync(ResultBuilder().RetryForeverAsync((_, context) =>
                {
                    seenContext = context;
                    return TaskHelper.EmptyTask;
                }), ResultPrimitive.Fault, ResultPrimitive.Good)).ShouldBe(ResultPrimitive.Good);
                seenContext.ShouldNotBeNull();
            },
            async () =>
            {
                Context? seenContext = null;
                (await ExecuteSequenceAsync(ResultBuilder().RetryForeverAsync((_, retryCount, context) =>
                {
                    retryCount.ShouldBe(1);
                    seenContext = context;
                    return TaskHelper.EmptyTask;
                }), ResultPrimitive.Fault, ResultPrimitive.Good)).ShouldBe(ResultPrimitive.Good);
                seenContext.ShouldNotBeNull();
            }
        };

        foreach (Func<Task> execution in executions)
        {
            await execution();
        }
    }

    [Fact]
    public async Task Async_result_wait_and_retry_overloads_should_retry_successfully()
    {
        TimeSpan[] sleepDurations = [TimeSpan.Zero];

        var executions = new Func<Task>[]
        {
            async () => (await ExecuteSequenceAsync(ResultBuilder().WaitAndRetryAsync(1, _ => TimeSpan.Zero), ResultPrimitive.Fault, ResultPrimitive.Good)).ShouldBe(ResultPrimitive.Good),
            async () => (await ExecuteSequenceAsync(ResultBuilder().WaitAndRetryAsync(1, _ => TimeSpan.Zero, (_, delay) => delay.ShouldBe(TimeSpan.Zero)), ResultPrimitive.Fault, ResultPrimitive.Good)).ShouldBe(ResultPrimitive.Good),
            async () => (await ExecuteSequenceAsync(ResultBuilder().WaitAndRetryAsync(1, _ => TimeSpan.Zero, (_, delay) => { delay.ShouldBe(TimeSpan.Zero); return TaskHelper.EmptyTask; }), ResultPrimitive.Fault, ResultPrimitive.Good)).ShouldBe(ResultPrimitive.Good),
            async () => (await ExecuteSequenceAsync(ResultBuilder().WaitAndRetryAsync(1, _ => TimeSpan.Zero, (_, delay, context) =>
            {
                delay.ShouldBe(TimeSpan.Zero);
                context.OperationKey.ShouldBe("operation-key");
            }), ResultPrimitive.Fault, ResultPrimitive.Good)).ShouldBe(ResultPrimitive.Good),
            async () => (await ExecuteSequenceAsync(ResultBuilder().WaitAndRetryAsync(1, _ => TimeSpan.Zero, (_, delay, context) =>
            {
                delay.ShouldBe(TimeSpan.Zero);
                context.OperationKey.ShouldBe("operation-key");
                return TaskHelper.EmptyTask;
            }), ResultPrimitive.Fault, ResultPrimitive.Good)).ShouldBe(ResultPrimitive.Good),
            async () => (await ExecuteSequenceAsync(ResultBuilder().WaitAndRetryAsync(1, _ => TimeSpan.Zero, (_, delay, retryCount, context) =>
            {
                delay.ShouldBe(TimeSpan.Zero);
                retryCount.ShouldBe(1);
                context.OperationKey.ShouldBe("operation-key");
            }), ResultPrimitive.Fault, ResultPrimitive.Good)).ShouldBe(ResultPrimitive.Good),
            async () => (await ExecuteSequenceAsync(ResultBuilder().WaitAndRetryAsync(1, (_, _) => TimeSpan.Zero, (_, delay, context) =>
            {
                delay.ShouldBe(TimeSpan.Zero);
                context.OperationKey.ShouldBe("operation-key");
            }), ResultPrimitive.Fault, ResultPrimitive.Good)).ShouldBe(ResultPrimitive.Good),
            async () => (await ExecuteSequenceAsync(ResultBuilder().WaitAndRetryAsync(1, (_, _) => TimeSpan.Zero, (_, delay, retryCount, context) =>
            {
                delay.ShouldBe(TimeSpan.Zero);
                retryCount.ShouldBe(1);
                context.OperationKey.ShouldBe("operation-key");
            }), ResultPrimitive.Fault, ResultPrimitive.Good)).ShouldBe(ResultPrimitive.Good),
            async () => (await ExecuteSequenceAsync(ResultBuilder().WaitAndRetryAsync(1, (_, _) => TimeSpan.Zero, (_, delay, context) =>
            {
                delay.ShouldBe(TimeSpan.Zero);
                context.OperationKey.ShouldBe("operation-key");
                return TaskHelper.EmptyTask;
            }), ResultPrimitive.Fault, ResultPrimitive.Good)).ShouldBe(ResultPrimitive.Good),
            async () => (await ExecuteSequenceAsync(
                ResultBuilder().WaitAndRetryAsync(
                    1,
                    (_, _, _) => TimeSpan.Zero,
                    (outcome, delay, retryCount, context) =>
                    {
                        outcome.Result.ShouldBe(ResultPrimitive.Fault);
                        delay.ShouldBe(TimeSpan.Zero);
                        retryCount.ShouldBe(1);
                        context.OperationKey.ShouldBe("operation-key");
                        return TaskHelper.EmptyTask;
                    }),
                ResultPrimitive.Fault,
                ResultPrimitive.Good)).ShouldBe(ResultPrimitive.Good),
            async () => (await ExecuteSequenceAsync(ResultBuilder().WaitAndRetryAsync(sleepDurations), ResultPrimitive.Fault, ResultPrimitive.Good)).ShouldBe(ResultPrimitive.Good),
            async () => (await ExecuteSequenceAsync(ResultBuilder().WaitAndRetryAsync(sleepDurations, (_, delay) => delay.ShouldBe(TimeSpan.Zero)), ResultPrimitive.Fault, ResultPrimitive.Good)).ShouldBe(ResultPrimitive.Good),
            async () => (await ExecuteSequenceAsync(ResultBuilder().WaitAndRetryAsync(sleepDurations, (_, delay) => { delay.ShouldBe(TimeSpan.Zero); return TaskHelper.EmptyTask; }), ResultPrimitive.Fault, ResultPrimitive.Good)).ShouldBe(ResultPrimitive.Good),
            async () => (await ExecuteSequenceAsync(ResultBuilder().WaitAndRetryAsync(sleepDurations, (_, delay, context) =>
            {
                delay.ShouldBe(TimeSpan.Zero);
                context.OperationKey.ShouldBe("operation-key");
            }), ResultPrimitive.Fault, ResultPrimitive.Good)).ShouldBe(ResultPrimitive.Good),
            async () => (await ExecuteSequenceAsync(ResultBuilder().WaitAndRetryAsync(sleepDurations, (_, delay, context) =>
            {
                delay.ShouldBe(TimeSpan.Zero);
                context.OperationKey.ShouldBe("operation-key");
                return TaskHelper.EmptyTask;
            }), ResultPrimitive.Fault, ResultPrimitive.Good)).ShouldBe(ResultPrimitive.Good),
            async () => (await ExecuteSequenceAsync(ResultBuilder().WaitAndRetryAsync(sleepDurations, (_, delay, retryCount, context) =>
            {
                delay.ShouldBe(TimeSpan.Zero);
                retryCount.ShouldBe(1);
                context.OperationKey.ShouldBe("operation-key");
            }), ResultPrimitive.Fault, ResultPrimitive.Good)).ShouldBe(ResultPrimitive.Good),
            async () => (await ExecuteSequenceAsync(ResultBuilder().WaitAndRetryAsync(sleepDurations, (_, delay, retryCount, context) =>
            {
                delay.ShouldBe(TimeSpan.Zero);
                retryCount.ShouldBe(1);
                context.OperationKey.ShouldBe("operation-key");
                return TaskHelper.EmptyTask;
            }), ResultPrimitive.Fault, ResultPrimitive.Good)).ShouldBe(ResultPrimitive.Good),
            async () => (await ExecuteSequenceAsync(ResultBuilder().WaitAndRetryForeverAsync(_ => TimeSpan.Zero), ResultPrimitive.Fault, ResultPrimitive.Good)).ShouldBe(ResultPrimitive.Good),
            async () => (await ExecuteSequenceAsync(ResultBuilder().WaitAndRetryForeverAsync((_, _) => TimeSpan.Zero), ResultPrimitive.Fault, ResultPrimitive.Good)).ShouldBe(ResultPrimitive.Good),
            async () => (await ExecuteSequenceAsync(ResultBuilder().WaitAndRetryForeverAsync(_ => TimeSpan.Zero, (_, delay) => delay.ShouldBe(TimeSpan.Zero)), ResultPrimitive.Fault, ResultPrimitive.Good)).ShouldBe(ResultPrimitive.Good),
            async () => (await ExecuteSequenceAsync(ResultBuilder().WaitAndRetryForeverAsync(_ => TimeSpan.Zero, (_, retryCount, delay) =>
            {
                retryCount.ShouldBe(1);
                delay.ShouldBe(TimeSpan.Zero);
            }), ResultPrimitive.Fault, ResultPrimitive.Good)).ShouldBe(ResultPrimitive.Good),
            async () => (await ExecuteSequenceAsync(ResultBuilder().WaitAndRetryForeverAsync(_ => TimeSpan.Zero, (_, delay) => { delay.ShouldBe(TimeSpan.Zero); return TaskHelper.EmptyTask; }), ResultPrimitive.Fault, ResultPrimitive.Good)).ShouldBe(ResultPrimitive.Good),
            async () => (await ExecuteSequenceAsync(ResultBuilder().WaitAndRetryForeverAsync(_ => TimeSpan.Zero, (_, retryCount, delay) =>
            {
                retryCount.ShouldBe(1);
                delay.ShouldBe(TimeSpan.Zero);
                return TaskHelper.EmptyTask;
            }), ResultPrimitive.Fault, ResultPrimitive.Good)).ShouldBe(ResultPrimitive.Good),
            async () => (await ExecuteSequenceAsync(ResultBuilder().WaitAndRetryForeverAsync(
                (_, _) => TimeSpan.Zero,
                (_, delay, context) =>
            {
                delay.ShouldBe(TimeSpan.Zero);
                context.OperationKey.ShouldBe("operation-key");
            }), ResultPrimitive.Fault, ResultPrimitive.Good)).ShouldBe(ResultPrimitive.Good),
            async () => (await ExecuteSequenceAsync(ResultBuilder().WaitAndRetryForeverAsync(
                (_, _) => TimeSpan.Zero,
                (_, retryCount, delay, context) =>
            {
                retryCount.ShouldBe(1);
                delay.ShouldBe(TimeSpan.Zero);
                context.OperationKey.ShouldBe("operation-key");
            }), ResultPrimitive.Fault, ResultPrimitive.Good)).ShouldBe(ResultPrimitive.Good),
            async () => (await ExecuteSequenceAsync(ResultBuilder().WaitAndRetryForeverAsync(
                (_, _) => TimeSpan.Zero,
                (_, delay, context) =>
            {
                delay.ShouldBe(TimeSpan.Zero);
                context.OperationKey.ShouldBe("operation-key");
                return TaskHelper.EmptyTask;
            }), ResultPrimitive.Fault, ResultPrimitive.Good)).ShouldBe(ResultPrimitive.Good),
            async () => (await ExecuteSequenceAsync(ResultBuilder().WaitAndRetryForeverAsync(
                (_, _) => TimeSpan.Zero,
                (_, retryCount, delay, context) =>
            {
                retryCount.ShouldBe(1);
                delay.ShouldBe(TimeSpan.Zero);
                context.OperationKey.ShouldBe("operation-key");
                return TaskHelper.EmptyTask;
            }), ResultPrimitive.Fault, ResultPrimitive.Good)).ShouldBe(ResultPrimitive.Good),
            async () => (await ExecuteSequenceAsync(ResultBuilder().WaitAndRetryForeverAsync((_, outcome, context) =>
            {
                outcome.Result.ShouldBe(ResultPrimitive.Fault);
                context.OperationKey.ShouldBe("operation-key");
                return TimeSpan.Zero;
            }, (outcome, delay, context) =>
            {
                outcome.Result.ShouldBe(ResultPrimitive.Fault);
                delay.ShouldBe(TimeSpan.Zero);
                context.OperationKey.ShouldBe("operation-key");
                return TaskHelper.EmptyTask;
            }), ResultPrimitive.Fault, ResultPrimitive.Good)).ShouldBe(ResultPrimitive.Good),
            async () => (await ExecuteSequenceAsync(ResultBuilder().WaitAndRetryForeverAsync((_, outcome, context) =>
            {
                outcome.Result.ShouldBe(ResultPrimitive.Fault);
                context.OperationKey.ShouldBe("operation-key");
                return TimeSpan.Zero;
            }, (outcome, retryCount, delay, context) =>
            {
                outcome.Result.ShouldBe(ResultPrimitive.Fault);
                retryCount.ShouldBe(1);
                delay.ShouldBe(TimeSpan.Zero);
                context.OperationKey.ShouldBe("operation-key");
                return TaskHelper.EmptyTask;
            }), ResultPrimitive.Fault, ResultPrimitive.Good)).ShouldBe(ResultPrimitive.Good)
        };

        foreach (Func<Task> execution in executions)
        {
            await execution();
        }
    }

    [Fact]
    public async Task Async_exception_retry_overloads_should_retry_successfully()
    {
        var executions = new Func<Task>[]
        {
            () => ExecuteExceptionSequenceAsync(ExceptionBuilder().RetryAsync(1, (_, retryCount) =>
            {
                retryCount.ShouldBe(1);
                return TaskHelper.EmptyTask;
            }), new InvalidOperationException()),
            () => ExecuteExceptionSequenceAsync(ExceptionBuilder().RetryAsync((_, retryCount, context) =>
            {
                retryCount.ShouldBe(1);
                context.OperationKey.ShouldBe("operation-key");
                return TaskHelper.EmptyTask;
            }), new InvalidOperationException()),
            () => ExecuteExceptionSequenceAsync(ExceptionBuilder().RetryForeverAsync(_ => TaskHelper.EmptyTask), new InvalidOperationException()),
            () => ExecuteExceptionSequenceAsync(ExceptionBuilder().RetryForeverAsync((_, retryCount) =>
            {
                retryCount.ShouldBe(1);
                return TaskHelper.EmptyTask;
            }), new InvalidOperationException()),
            () => ExecuteExceptionSequenceAsync(ExceptionBuilder().RetryForeverAsync((_, retryCount, context) =>
            {
                retryCount.ShouldBe(1);
                context.OperationKey.ShouldBe("operation-key");
            }), new InvalidOperationException())
        };

        foreach (Func<Task> execution in executions)
        {
            await execution();
        }
    }

    [Fact]
    public async Task Async_exception_wait_and_retry_overloads_should_retry_successfully()
    {
        var executions = new Func<Task>[]
        {
            () => ExecuteExceptionSequenceAsync(ExceptionBuilder().WaitAndRetryAsync(1, _ => TimeSpan.Zero), new InvalidOperationException()),
            () => ExecuteExceptionSequenceAsync(ExceptionBuilder().WaitAndRetryAsync(1, _ => TimeSpan.Zero, (_, delay, context) =>
            {
                delay.ShouldBe(TimeSpan.Zero);
                context.OperationKey.ShouldBe("operation-key");
            }), new InvalidOperationException()),
            () => ExecuteExceptionSequenceAsync(ExceptionBuilder().WaitAndRetryAsync(1, _ => TimeSpan.Zero, (_, delay, context) =>
            {
                delay.ShouldBe(TimeSpan.Zero);
                context.OperationKey.ShouldBe("operation-key");
                return TaskHelper.EmptyTask;
            }), new InvalidOperationException()),
            () => ExecuteExceptionSequenceAsync(ExceptionBuilder().WaitAndRetryAsync(1, _ => TimeSpan.Zero, (_, delay, retryCount, context) =>
            {
                delay.ShouldBe(TimeSpan.Zero);
                retryCount.ShouldBe(1);
                context.OperationKey.ShouldBe("operation-key");
            }), new InvalidOperationException()),
            () => ExecuteExceptionSequenceAsync(ExceptionBuilder().WaitAndRetryAsync(1, (_, _) => TimeSpan.Zero, (_, delay, context) =>
            {
                delay.ShouldBe(TimeSpan.Zero);
                context.OperationKey.ShouldBe("operation-key");
                return TaskHelper.EmptyTask;
            }), new InvalidOperationException()),
            () => ExecuteExceptionSequenceAsync(ExceptionBuilder().WaitAndRetryAsync(1, (_, _) => TimeSpan.Zero, (_, delay, retryCount, context) =>
            {
                delay.ShouldBe(TimeSpan.Zero);
                retryCount.ShouldBe(1);
                context.OperationKey.ShouldBe("operation-key");
            }), new InvalidOperationException()),
            () => ExecuteExceptionSequenceAsync(ExceptionBuilder().WaitAndRetryForeverAsync(_ => TimeSpan.Zero, (_, delay) =>
            {
                delay.ShouldBe(TimeSpan.Zero);
                return TaskHelper.EmptyTask;
            }), new InvalidOperationException()),
            () => ExecuteExceptionSequenceAsync(ExceptionBuilder().WaitAndRetryForeverAsync(_ => TimeSpan.Zero, (_, retryCount, delay) =>
            {
                retryCount.ShouldBe(1);
                delay.ShouldBe(TimeSpan.Zero);
                return TaskHelper.EmptyTask;
            }), new InvalidOperationException()),
            () => ExecuteExceptionSequenceAsync(ExceptionBuilder().WaitAndRetryForeverAsync((_, _, _) => TimeSpan.Zero, (_, delay, context) =>
            {
                delay.ShouldBe(TimeSpan.Zero);
                context.OperationKey.ShouldBe("operation-key");
                return TaskHelper.EmptyTask;
            }), new InvalidOperationException())
        };

        foreach (Func<Task> execution in executions)
        {
            await execution();
        }
    }

    private static PolicyBuilder<ResultPrimitive> ResultBuilder() =>
        Policy.HandleResult(ResultPrimitive.Fault);

    private static PolicyBuilder ExceptionBuilder() =>
        Policy.Handle<InvalidOperationException>();

    private static ResultPrimitive ExecuteSequence(Policy<ResultPrimitive> policy, params ResultPrimitive[] results)
    {
        var context = new Context("operation-key", CreateDictionary("key", "value"));
        int index = 0;

        return policy.Execute(_ => results[index++], context);
    }

    private static Task<ResultPrimitive> ExecuteSequenceAsync(AsyncPolicy<ResultPrimitive> policy, params ResultPrimitive[] results)
    {
        var context = new Context("operation-key", CreateDictionary("key", "value"));
        int index = 0;

        return policy.ExecuteAsync(_ => Task.FromResult(results[index++]), context);
    }

    private static Task ExecuteExceptionSequenceAsync(AsyncPolicy policy, params Exception[] exceptions)
    {
        var context = new Context("operation-key", CreateDictionary("key", "value"));
        int index = 0;

        return policy.ExecuteAsync(_ =>
        {
            if (index < exceptions.Length)
            {
                throw exceptions[index++];
            }

            return TaskHelper.EmptyTask;
        }, context);
    }
}
