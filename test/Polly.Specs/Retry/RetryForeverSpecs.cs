namespace Polly.Specs.Retry;

public class RetryForeverSpecs
{
    [Fact]
    public void Should_throw_when_onretry_action_without_context_is_null()
    {
        Action<Exception> nullOnRetry = null!;

        Action policy = () => Policy
                                  .Handle<DivideByZeroException>()
                                  .RetryForever(nullOnRetry);

        policy.Should().Throw<ArgumentNullException>().And
              .ParamName.Should().Be("onRetry");
    }

    [Fact]
    public void Should_throw_when_onretry_action_with_context_is_null()
    {
        Action<Exception, Context> nullOnRetry = null!;

        Action policy = () => Policy
                                  .Handle<DivideByZeroException>()
                                  .RetryForever(nullOnRetry);

        policy.Should().Throw<ArgumentNullException>().And
              .ParamName.Should().Be("onRetry");
    }

    [Fact]
    public void Should_not_throw_regardless_of_how_many_times_the_specified_exception_is_raised()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .RetryForever();

        policy.Invoking(x => x.RaiseException<DivideByZeroException>(3))
              .Should().NotThrow();
    }

    [Fact]
    public void Should_not_throw_regardless_of_how_many_times_one_of_the_specified_exception_is_raised()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .Or<ArgumentException>()
            .RetryForever();

        policy.Invoking(x => x.RaiseException<ArgumentException>(3))
              .Should().NotThrow();
    }

    [Fact]
    public void Should_throw_when_exception_thrown_is_not_the_specified_exception_type()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .RetryForever();

        policy.Invoking(x => x.RaiseException<NullReferenceException>())
              .Should().Throw<NullReferenceException>();
    }

    [Fact]
    public async Task Should_throw_when_exception_thrown_is_not_the_specified_exception_type_async()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .RetryForeverAsync();

        await policy.Awaiting(x => x.RaiseExceptionAsync<NullReferenceException>())
              .Should().ThrowAsync<NullReferenceException>();
    }

    [Fact]
    public void Should_throw_when_exception_thrown_is_not_one_of_the_specified_exception_types()
    {
        var policy = Policy
            .Handle<DivideByZeroException>()
            .Or<ArgumentException>()
            .RetryForever();

        policy.Invoking(x => x.RaiseException<NullReferenceException>())
              .Should().Throw<NullReferenceException>();
    }

    [Fact]
    public void Should_throw_when_specified_exception_predicate_is_not_satisfied()
    {
        var policy = Policy
            .Handle<DivideByZeroException>(_ => false)
            .RetryForever();

        policy.Invoking(x => x.RaiseException<DivideByZeroException>())
              .Should().Throw<DivideByZeroException>();
    }

    [Fact]
    public void Should_throw_when_none_of_the_specified_exception_predicates_are_satisfied()
    {
        var policy = Policy
            .Handle<DivideByZeroException>(_ => false)
            .Or<ArgumentException>(_ => false)
            .RetryForever();

        policy.Invoking(x => x.RaiseException<ArgumentException>())
              .Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Should_not_throw_when_specified_exception_predicate_is_satisfied()
    {
        var policy = Policy
            .Handle<DivideByZeroException>(_ => true)
            .RetryForever();

        policy.Invoking(x => x.RaiseException<DivideByZeroException>())
              .Should().NotThrow();
    }

    [Fact]
    public async Task Should_not_throw_when_specified_exception_predicate_is_satisfied_async()
    {
        var policy = Policy
            .Handle<DivideByZeroException>(_ => true)
            .RetryForeverAsync();

        await policy.Awaiting(x => x.RaiseExceptionAsync<DivideByZeroException>())
              .Should().NotThrowAsync();
    }

    [Fact]
    public void Should_not_throw_when_one_of_the_specified_exception_predicates_are_satisfied()
    {
        var policy = Policy
            .Handle<DivideByZeroException>(_ => true)
            .Or<ArgumentException>(_ => true)
            .RetryForever();

        policy.Invoking(x => x.RaiseException<ArgumentException>())
              .Should().NotThrow();
    }

    [Fact]
    public async Task Should_not_throw_when_one_of_the_specified_exception_predicates_are_satisfied_async()
    {
        var policy = Policy
            .Handle<DivideByZeroException>(_ => true)
            .Or<ArgumentException>(_ => true)
            .RetryForeverAsync();

        await policy.Awaiting(x => x.RaiseExceptionAsync<ArgumentException>())
              .Should().NotThrowAsync();
    }

    [Fact]
    public void Should_call_onretry_on_each_retry_with_the_current_exception()
    {
        var expectedExceptions = new[] { "Exception #1", "Exception #2", "Exception #3" };
        var retryExceptions = new List<Exception>();

        var policy = Policy
            .Handle<DivideByZeroException>()
            .RetryForever(exception => retryExceptions.Add(exception));

        policy.RaiseException<DivideByZeroException>(3, (e, i) => e.HelpLink = "Exception #" + i);

        retryExceptions
            .Select(x => x.HelpLink)
            .Should()
            .ContainInOrder(expectedExceptions);
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

        contextData.Should()
                   .ContainKeys("key1", "key2").And
                   .ContainValues("value1", "value2");
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

        retryCounts.Should()
            .ContainInOrder(expectedRetryCounts);
    }

    [Fact]
    public void Should_not_call_onretry_when_no_retries_are_performed()
    {
        var retryExceptions = new List<Exception>();

        var policy = Policy
            .Handle<DivideByZeroException>()
            .RetryForever(exception => retryExceptions.Add(exception));

        policy.Invoking(x => x.RaiseException<ArgumentException>())
              .Should().Throw<ArgumentException>();

        retryExceptions.Should()
                       .BeEmpty();
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

        contextValue.Should().Be("original_value");

        policy.RaiseException<DivideByZeroException>(
            CreateDictionary("key", "new_value"));

        contextValue.Should().Be("new_value");
    }
}
