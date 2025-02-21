namespace Polly.Specs.Retry;

public class RetryForeverSpecs
{
    [Fact]
    public void Should_throw_when_onretry_action_is_null()
    {
        Action<Exception> onRetry = null!;

        Action policy = () => Policy
            .Handle<DivideByZeroException>()
            .RetryForever(onRetry);

        Should.Throw<ArgumentNullException>(policy)
            .ParamName.ShouldBe("onRetry");

        Action<Exception, Context> onRetryContext = null!;

        policy = () => Policy
            .Handle<DivideByZeroException>()
            .RetryForever(onRetryContext);

        Should.Throw<ArgumentNullException>(policy)
              .ParamName.ShouldBe("onRetry");

        Action<DelegateResult<ResultPrimitive>> onRetryResult = null!;

        policy = () => Policy
            .HandleResult(ResultPrimitive.Fault)
            .RetryForever(onRetryResult);

        Should.Throw<ArgumentNullException>(policy)
            .ParamName.ShouldBe("onRetry");

        Action<DelegateResult<ResultPrimitive>, int> onRetryResultAttempts = null!;

        policy = () => Policy
            .HandleResult(ResultPrimitive.Fault)
            .RetryForever(onRetryResultAttempts);

        Should.Throw<ArgumentNullException>(policy)
            .ParamName.ShouldBe("onRetry");

        Action<DelegateResult<ResultPrimitive>, Context> onRetryResultContext = null!;

        policy = () => Policy
            .HandleResult(ResultPrimitive.Fault)
            .RetryForever(onRetryResultContext);

        Should.Throw<ArgumentNullException>(policy)
            .ParamName.ShouldBe("onRetry");

        policy = () => Policy
            .HandleResult(ResultPrimitive.Fault)
            .RetryForever(onRetryResultContext);

        Should.Throw<ArgumentNullException>(policy)
            .ParamName.ShouldBe("onRetry");

        Action<Exception, int> onRetryAttempts = null!;

        policy = () => Policy
            .Handle<DivideByZeroException>()
            .RetryForever(onRetryAttempts);

        Should.Throw<ArgumentNullException>(policy)
            .ParamName.ShouldBe("onRetry");

        Action<Exception, int, Context> onRetryAttemptsContext = null!;

        policy = () => Policy
            .Handle<DivideByZeroException>()
            .RetryForever(onRetryAttemptsContext);

        Should.Throw<ArgumentNullException>(policy)
            .ParamName.ShouldBe("onRetry");

        Action<DelegateResult<ResultPrimitive>, int, Context> onRetryAttemptsResult = null!;

        policy = () => Policy
            .HandleResult(ResultPrimitive.Fault)
            .RetryForever(onRetryAttemptsResult);

        Should.Throw<ArgumentNullException>(policy)
            .ParamName.ShouldBe("onRetry");
    }

    [Fact]
    public void Should_not_throw_when_onretry_action_with_context_is_valid()
    {
        Action<DelegateResult<ResultPrimitive>, Context> onRetry = (_, _) => { };

        Action policy = () => Policy
                                  .HandleResult(ResultPrimitive.Fault)
                                  .RetryForever(onRetry);

        Should.NotThrow(policy);
    }

    [Fact]
    public void Should_not_throw_when_onretry_action_with_int_is_valid()
    {
        Action<DelegateResult<ResultPrimitive>, int> onRetry = (_, _) => { };

        Action policy = () => Policy
                                  .HandleResult(ResultPrimitive.Fault)
                                  .RetryForever(onRetry);

        Should.NotThrow(policy);
    }

    [Fact]
    public void Should_not_throw_regardless_of_how_many_times_the_specified_exception_is_raised()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .RetryForever();

        Should.NotThrow(() => policy.RaiseException<DivideByZeroException>(3));
    }

    [Fact]
    public void Should_not_throw_regardless_of_how_many_times_one_of_the_specified_exception_is_raised()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .Or<ArgumentException>()
            .RetryForever();

        Should.NotThrow(() => policy.RaiseException<ArgumentException>(3));
    }

    [Fact]
    public void Should_throw_when_exception_thrown_is_not_the_specified_exception_type()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .RetryForever();

        Should.Throw<NullReferenceException>(() => policy.RaiseException<NullReferenceException>());
    }

    [Fact]
    public async Task Should_throw_when_exception_thrown_is_not_the_specified_exception_type_async()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .RetryForeverAsync();

        await Should.ThrowAsync<NullReferenceException>(() => policy.RaiseExceptionAsync<NullReferenceException>());
    }

    [Fact]
    public void Should_throw_when_exception_thrown_is_not_one_of_the_specified_exception_types()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .Or<ArgumentException>()
            .RetryForever();

        Should.Throw<NullReferenceException>(() => policy.RaiseException<NullReferenceException>());
    }

    [Fact]
    public void Should_throw_when_specified_exception_predicate_is_not_satisfied()
    {
        var policy = Policy
            .Handle<DivideByZeroException>(_ => false)
            .RetryForever();

        Should.Throw<DivideByZeroException>(() => policy.RaiseException<DivideByZeroException>());
    }

    [Fact]
    public void Should_throw_when_none_of_the_specified_exception_predicates_are_satisfied()
    {
        var policy = Policy
            .Handle<DivideByZeroException>(_ => false)
            .Or<ArgumentException>(_ => false)
            .RetryForever();

        Should.Throw<ArgumentException>(() => policy.RaiseException<ArgumentException>());
    }

    [Fact]
    public void Should_not_throw_when_specified_exception_predicate_is_satisfied()
    {
        var policy = Policy
            .Handle<DivideByZeroException>(_ => true)
            .RetryForever();

        Should.NotThrow(() => policy.RaiseException<DivideByZeroException>());
    }

    [Fact]
    public async Task Should_not_throw_when_specified_exception_predicate_is_satisfied_async()
    {
        var policy = Policy
            .Handle<DivideByZeroException>(_ => true)
            .RetryForeverAsync();

        await Should.NotThrowAsync(() => policy.RaiseExceptionAsync<DivideByZeroException>());
    }

    [Fact]
    public void Should_not_throw_when_one_of_the_specified_exception_predicates_are_satisfied()
    {
        var policy = Policy
            .Handle<DivideByZeroException>(_ => true)
            .Or<ArgumentException>(_ => true)
            .RetryForever();

        Should.NotThrow(() => policy.RaiseException<ArgumentException>());
    }

    [Fact]
    public async Task Should_not_throw_when_one_of_the_specified_exception_predicates_are_satisfied_async()
    {
        var policy = Policy
            .Handle<DivideByZeroException>(_ => true)
            .Or<ArgumentException>(_ => true)
            .RetryForeverAsync();

        await Should.NotThrowAsync(() => policy.RaiseExceptionAsync<ArgumentException>());
    }

    [Fact]
    public void Should_call_onretry_on_each_retry_with_the_current_exception()
    {
        var expectedExceptions = new[] { "Exception #1", "Exception #2", "Exception #3" };
        var retryExceptions = new List<Exception>();

        var policy = Policy
            .Handle<DivideByZeroException>()
            .RetryForever(retryExceptions.Add);

        policy.RaiseException<DivideByZeroException>(3, (e, i) => e.HelpLink = "Exception #" + i);

        retryExceptions
            .Select(x => x.HelpLink)
            .ShouldBe(expectedExceptions);
    }

    [Fact]
    public void Should_call_onretry_on_each_retry_with_the_passed_context()
    {
        IDictionary<string, object>? contextData = null;

        var policy = Policy
            .Handle<DivideByZeroException>()
            .RetryForever((_, context) => contextData = context);

        policy.RaiseException<DivideByZeroException>(
            CreateDictionary("key1", "value1", "key2", "value2"));

        contextData.ShouldNotBeNull();
        contextData.ShouldContainKeyAndValue("key1", "value1");
        contextData.ShouldContainKeyAndValue("key2", "value2");
    }

    [Fact]
    public void Should_call_onretry_on_each_retry_with_the_current_retry_count()
    {
        var expectedRetryCounts = new[] { 1, 2, 3 };
        var retryCounts = new List<int>();

        var policy = Policy
            .Handle<DivideByZeroException>()
            .RetryForever((_, retryCount) => retryCounts.Add(retryCount));

        policy.RaiseException<DivideByZeroException>(3);

        retryCounts.ShouldBe(expectedRetryCounts);
    }

    [Fact]
    public void Should_not_call_onretry_when_no_retries_are_performed()
    {
        var retryExceptions = new List<Exception>();

        var policy = Policy
            .Handle<DivideByZeroException>()
            .RetryForever(retryExceptions.Add);

        Should.Throw<ArgumentException>(() => policy.RaiseException<ArgumentException>());

        retryExceptions.ShouldBeEmpty();
    }

    [Fact]
    public void Should_create_new_context_for_each_call_to_execute()
    {
        string? contextValue = null;

        var policy = Policy
            .Handle<DivideByZeroException>()
            .RetryForever((_, context) => contextValue = context["key"].ToString());

        policy.RaiseException<DivideByZeroException>(
            CreateDictionary("key", "original_value"));

        contextValue.ShouldBe("original_value");

        policy.RaiseException<DivideByZeroException>(
            CreateDictionary("key", "new_value"));

        contextValue.ShouldBe("new_value");
    }
}
