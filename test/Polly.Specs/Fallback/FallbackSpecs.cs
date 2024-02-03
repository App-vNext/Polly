using Scenario = Polly.Specs.Helpers.PolicyExtensions.ExceptionAndOrCancellationScenario;

namespace Polly.Specs.Fallback;

public class FallbackSpecs
{
    #region Configuration guard condition tests

    [Fact]
    public void Should_throw_when_fallback_action_is_null()
    {
        Action fallbackAction = null!;

        Action policy = () => Policy
                                .Handle<DivideByZeroException>()
                                .Fallback(fallbackAction);

        policy.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("fallbackAction");
    }

    [Fact]
    public void Should_throw_when_fallback_action_with_cancellation_is_null()
    {
        Action<CancellationToken> fallbackAction = null!;

        Action policy = () => Policy
            .Handle<DivideByZeroException>()
            .Fallback(fallbackAction);

        policy.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("fallbackAction");
    }

    [Fact]
    public void Should_throw_when_fallback_action_is_null_with_onFallback()
    {
        Action fallbackAction = null!;
        Action<Exception> onFallback = _ => { };

        Action policy = () => Policy
                                .Handle<DivideByZeroException>()
                                .Fallback(fallbackAction, onFallback);

        policy.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("fallbackAction");
    }

    [Fact]
    public void Should_throw_when_fallback_action_with_cancellation_is_null_with_onFallback()
    {
        Action<CancellationToken> fallbackAction = null!;
        Action<Exception> onFallback = _ => { };

        Action policy = () => Policy
            .Handle<DivideByZeroException>()
            .Fallback(fallbackAction, onFallback);

        policy.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("fallbackAction");
    }

    [Fact]
    public void Should_throw_when_fallback_action_is_null_with_onFallback_with_context()
    {
        Action<Context> fallbackAction = null!;
        Action<Exception, Context> onFallback = (_, _) => { };

        Action policy = () => Policy
                                .Handle<DivideByZeroException>()
                                .Fallback(fallbackAction, onFallback);

        policy.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("fallbackAction");
    }

    [Fact]
    public void Should_throw_when_fallback_action_with_cancellation_is_null_with_onFallback_with_context()
    {
        Action<Context, CancellationToken> fallbackAction = null!;
        Action<Exception, Context> onFallback = (_, _) => { };

        Action policy = () => Policy
                                .Handle<DivideByZeroException>()
                                .Fallback(fallbackAction, onFallback);

        policy.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("fallbackAction");
    }

    [Fact]
    public void Should_throw_when_onFallback_delegate_is_null()
    {
        Action fallbackAction = () => { };
        Action<Exception> onFallback = null!;

        Action policy = () => Policy
                                .Handle<DivideByZeroException>()
                                .Fallback(fallbackAction, onFallback);

        policy.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("onFallback");
    }

    [Fact]
    public void Should_throw_when_onFallback_delegate_is_null_with_action_with_cancellation()
    {
        Action<CancellationToken> fallbackAction = _ => { };
        Action<Exception> onFallback = null!;

        Action policy = () => Policy
            .Handle<DivideByZeroException>()
            .Fallback(fallbackAction, onFallback);

        policy.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("onFallback");
    }

    [Fact]
    public void Should_throw_when_onFallback_delegate_is_null_with_context()
    {
        Action<Context> fallbackAction = _ => { };
        Action<Exception, Context> onFallback = null!;

        Action policy = () => Policy
                                .Handle<DivideByZeroException>()
                                .Fallback(fallbackAction, onFallback);

        policy.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("onFallback");
    }

    [Fact]
    public void Should_throw_when_onFallback_delegate_is_null_with_context_with_action_with_cancellation()
    {
        Action<Context, CancellationToken> fallbackAction = (_, _) => { };
        Action<Exception, Context> onFallback = null!;

        Action policy = () => Policy
                                .Handle<DivideByZeroException>()
                                .Fallback(fallbackAction, onFallback);

        policy.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("onFallback");
    }

    #endregion

    #region Policy operation tests

    [Fact]
    public void Should_not_execute_fallback_when_executed_delegate_does_not_throw()
    {
        bool fallbackActionExecuted = false;
        Action fallbackAction = () => { fallbackActionExecuted = true; };

        FallbackPolicy fallbackPolicy = Policy
                                .Handle<DivideByZeroException>()
                                .Fallback(fallbackAction);

        fallbackPolicy.Execute(() => { });

        fallbackActionExecuted.Should().BeFalse();
    }

    [Fact]
    public void Should_not_execute_fallback_when_executed_delegate_throws_exception_not_handled_by_policy()
    {
        bool fallbackActionExecuted = false;
        Action fallbackAction = () => { fallbackActionExecuted = true; };

        FallbackPolicy fallbackPolicy = Policy
                                .Handle<DivideByZeroException>()
                                .Fallback(fallbackAction);

        fallbackPolicy.Invoking(x => x.RaiseException<ArgumentNullException>()).Should().Throw<ArgumentNullException>();

        fallbackActionExecuted.Should().BeFalse();
    }

    [Fact]
    public void Should_execute_fallback_when_executed_delegate_throws_exception_handled_by_policy()
    {
        bool fallbackActionExecuted = false;
        Action fallbackAction = () => { fallbackActionExecuted = true; };

        FallbackPolicy fallbackPolicy = Policy
            .Handle<DivideByZeroException>()
            .Fallback(fallbackAction);

        fallbackPolicy.Invoking(x => x.RaiseException<DivideByZeroException>()).Should().NotThrow();

        fallbackActionExecuted.Should().BeTrue();
    }

    [Fact]
    public void Should_execute_fallback_when_executed_delegate_throws_one_of_exceptions_handled_by_policy()
    {
        bool fallbackActionExecuted = false;
        Action fallbackAction = () => { fallbackActionExecuted = true; };

        FallbackPolicy fallbackPolicy = Policy
            .Handle<DivideByZeroException>()
            .Or<ArgumentException>()
            .Fallback(fallbackAction);

        fallbackPolicy.Invoking(x => x.RaiseException<ArgumentException>()).Should().NotThrow();

        fallbackActionExecuted.Should().BeTrue();
    }

    [Fact]
    public void Should_not_execute_fallback_when_executed_delegate_throws_exception_not_one_of_exceptions_handled_by_policy()
    {
        bool fallbackActionExecuted = false;
        Action fallbackAction = () => { fallbackActionExecuted = true; };

        FallbackPolicy fallbackPolicy = Policy
                                .Handle<DivideByZeroException>()
                                .Or<NullReferenceException>()
                                .Fallback(fallbackAction);

        fallbackPolicy.Invoking(x => x.RaiseException<ArgumentNullException>()).Should().Throw<ArgumentNullException>();

        fallbackActionExecuted.Should().BeFalse();
    }

    [Fact]
    public void Should_not_execute_fallback_when_exception_thrown_does_not_match_handling_predicates()
    {
        bool fallbackActionExecuted = false;
        Action fallbackAction = () => { fallbackActionExecuted = true; };

        FallbackPolicy fallbackPolicy = Policy
                                .Handle<DivideByZeroException>(_ => false)
                                .Fallback(fallbackAction);

        fallbackPolicy.Invoking(x => x.RaiseException<DivideByZeroException>()).Should().Throw<DivideByZeroException>();

        fallbackActionExecuted.Should().BeFalse();
    }

    [Fact]
    public void Should_not_execute_fallback_when_exception_thrown_does_not_match_any_of_handling_predicates()
    {
        bool fallbackActionExecuted = false;
        Action fallbackAction = () => { fallbackActionExecuted = true; };

        FallbackPolicy fallbackPolicy = Policy
                                .Handle<DivideByZeroException>(_ => false)
                                .Or<ArgumentNullException>(_ => false)
                                .Fallback(fallbackAction);

        fallbackPolicy.Invoking(x => x.RaiseException<DivideByZeroException>()).Should().Throw<DivideByZeroException>();

        fallbackActionExecuted.Should().BeFalse();
    }

    [Fact]
    public void Should_execute_fallback_when_exception_thrown_matches_handling_predicates()
    {
        bool fallbackActionExecuted = false;
        Action fallbackAction = () => { fallbackActionExecuted = true; };

        FallbackPolicy fallbackPolicy = Policy
            .Handle<DivideByZeroException>(_ => true)
            .Fallback(fallbackAction);

        fallbackPolicy.Invoking(x => x.RaiseException<DivideByZeroException>()).Should().NotThrow();

        fallbackActionExecuted.Should().BeTrue();
    }

    [Fact]
    public void Should_execute_fallback_when_exception_thrown_matches_one_of_handling_predicates()
    {
        bool fallbackActionExecuted = false;
        Action fallbackAction = () => { fallbackActionExecuted = true; };

        FallbackPolicy fallbackPolicy = Policy
            .Handle<DivideByZeroException>(_ => true)
            .Or<ArgumentNullException>()
            .Fallback(fallbackAction);

        fallbackPolicy.Invoking(x => x.RaiseException<DivideByZeroException>()).Should().NotThrow();

        fallbackActionExecuted.Should().BeTrue();
    }

    [Fact]
    public void Should_not_handle_exception_thrown_by_fallback_delegate_even_if_is_exception_handled_by_policy()
    {
        bool fallbackActionExecuted = false;
        Action fallbackAction = () =>
        {
            fallbackActionExecuted = true;
            throw new DivideByZeroException { HelpLink = "FromFallbackAction" };
        };

        FallbackPolicy fallbackPolicy = Policy
            .Handle<DivideByZeroException>()
            .Fallback(fallbackAction);

        fallbackPolicy.Invoking(x => x.RaiseException<DivideByZeroException>((e, _) => e.HelpLink = "FromExecuteDelegate"))
            .Should().Throw<DivideByZeroException>().And.HelpLink.Should().Be("FromFallbackAction");

        fallbackActionExecuted.Should().BeTrue();
    }

    [Fact]
    public void Should_throw_for_generic_method_execution_on_non_generic_policy()
    {
        FallbackPolicy fallbackPolicy = Policy
            .Handle<DivideByZeroException>()
            .Fallback(() => { });

        fallbackPolicy.Invoking(p => p.Execute<int>(() => 0)).Should().Throw<InvalidOperationException>();
    }

    #endregion

    #region HandleInner tests, inner of normal exceptions

    [Fact]
    public void Should_not_execute_fallback_when_executed_delegate_throws_inner_exception_where_policy_doesnt_handle_inner()
    {
        bool fallbackActionExecuted = false;
        Action fallbackAction = () => { fallbackActionExecuted = true; };

        FallbackPolicy fallbackPolicy = Policy
            .Handle<DivideByZeroException>()
            .Fallback(fallbackAction);

        Exception withInner = new Exception(string.Empty, new DivideByZeroException());

        fallbackPolicy.Invoking(x => x.RaiseException(withInner)).Should().Throw<Exception>().And.InnerException.Should().BeOfType<DivideByZeroException>();

        fallbackActionExecuted.Should().BeFalse();
    }

    [Fact]
    public void Should_execute_fallback_when_policy_handles_inner_and_executed_delegate_throws_as_non_inner()
    {
        bool fallbackActionExecuted = false;
        Action fallbackAction = () => { fallbackActionExecuted = true; };

        FallbackPolicy fallbackPolicy = Policy
            .HandleInner<DivideByZeroException>()
            .Fallback(fallbackAction);

        Exception nonInner = new DivideByZeroException();

        fallbackPolicy.Invoking(x => x.RaiseException(nonInner)).Should().NotThrow();

        fallbackActionExecuted.Should().BeTrue();
    }

    [Fact]
    public void Should_execute_fallback_when_executed_delegate_throws_inner_exception_handled_by_policy()
    {
        bool fallbackActionExecuted = false;
        Action fallbackAction = () => { fallbackActionExecuted = true; };

        FallbackPolicy fallbackPolicy = Policy
            .HandleInner<DivideByZeroException>()
            .Fallback(fallbackAction);

        Exception withInner = new Exception(string.Empty, new DivideByZeroException());

        fallbackPolicy.Invoking(x => x.RaiseException(withInner)).Should().NotThrow();

        fallbackActionExecuted.Should().BeTrue();
    }

    [Fact]
    public void Should_execute_fallback_when_executed_delegate_throws_one_of_inner_exceptions_handled_by_policy()
    {
        bool fallbackActionExecuted = false;
        Action fallbackAction = () => { fallbackActionExecuted = true; };

        FallbackPolicy fallbackPolicy = Policy
            .Handle<DivideByZeroException>()
            .OrInner<ArgumentException>()
            .Fallback(fallbackAction);

        Exception withInner = new Exception(string.Empty, new ArgumentException());

        fallbackPolicy.Invoking(x => x.RaiseException(withInner)).Should().NotThrow();

        fallbackActionExecuted.Should().BeTrue();
    }

    [Fact]
    public void Should_execute_fallback_when_executed_delegate_throws_nested_inner_exception_handled_by_policy()
    {
        bool fallbackActionExecuted = false;
        Action fallbackAction = () => { fallbackActionExecuted = true; };

        FallbackPolicy fallbackPolicy = Policy
            .HandleInner<DivideByZeroException>()
            .Fallback(fallbackAction);

        Exception withInner = new Exception(string.Empty, new Exception(string.Empty, new Exception(string.Empty, new DivideByZeroException())));

        fallbackPolicy.Invoking(x => x.RaiseException(withInner)).Should().NotThrow();

        fallbackActionExecuted.Should().BeTrue();
    }

    [Fact]
    public void Should_not_execute_fallback_when_executed_delegate_throws_inner_exception_not_handled_by_policy()
    {
        bool fallbackActionExecuted = false;
        Action fallbackAction = () => { fallbackActionExecuted = true; };

        FallbackPolicy fallbackPolicy = Policy
                                .HandleInner<DivideByZeroException>()
                                .Fallback(fallbackAction);

        Exception withInner = new Exception(string.Empty, new ArgumentException());

        fallbackPolicy.Invoking(x => x.RaiseException(withInner)).Should().Throw<Exception>().And.InnerException.Should().BeOfType<ArgumentException>();

        fallbackActionExecuted.Should().BeFalse();
    }

    [Fact]
    public void Should_execute_fallback_when_inner_exception_thrown_matches_handling_predicates()
    {
        bool fallbackActionExecuted = false;
        Action fallbackAction = () => { fallbackActionExecuted = true; };

        FallbackPolicy fallbackPolicy = Policy
            .HandleInner<DivideByZeroException>(_ => true)
            .Fallback(fallbackAction);

        Exception withInner = new Exception(string.Empty, new DivideByZeroException());

        fallbackPolicy.Invoking(x => x.RaiseException(withInner)).Should().NotThrow();

        fallbackActionExecuted.Should().BeTrue();
    }

    [Fact]
    public void Should_execute_fallback_when_inner_nested_exception_thrown_matches_handling_predicates()
    {
        bool fallbackActionExecuted = false;
        Action fallbackAction = () => { fallbackActionExecuted = true; };

        FallbackPolicy fallbackPolicy = Policy
            .HandleInner<DivideByZeroException>(_ => true)
            .Fallback(fallbackAction);

        Exception withInner = new Exception(string.Empty, new Exception(string.Empty, new Exception(string.Empty, new DivideByZeroException())));

        fallbackPolicy.Invoking(x => x.RaiseException(withInner)).Should().NotThrow();

        fallbackActionExecuted.Should().BeTrue();
    }

    [Fact]
    public void Should_execute_fallback_when_inner_exception_thrown_matches_one_of_handling_predicates()
    {
        bool fallbackActionExecuted = false;
        Action fallbackAction = () => { fallbackActionExecuted = true; };

        FallbackPolicy fallbackPolicy = Policy
            .HandleInner<DivideByZeroException>(_ => true)
            .OrInner<ArgumentNullException>(_ => true)
            .Fallback(fallbackAction);

        Exception withInner = new Exception(string.Empty, new ArgumentNullException());

        fallbackPolicy.Invoking(x => x.RaiseException(withInner)).Should().NotThrow();

        fallbackActionExecuted.Should().BeTrue();
    }

    [Fact]
    public void Should_not_execute_fallback_when_inner_exception_thrown_does_not_match_handling_predicates()
    {
        bool fallbackActionExecuted = false;
        Action fallbackAction = () => { fallbackActionExecuted = true; };

        FallbackPolicy fallbackPolicy = Policy
                                .HandleInner<DivideByZeroException>(_ => false)
                                .Fallback(fallbackAction);

        Exception withInner = new Exception(string.Empty, new DivideByZeroException());

        fallbackPolicy.Invoking(x => x.RaiseException(withInner)).Should().Throw<Exception>().And.InnerException.Should().BeOfType<DivideByZeroException>();

        fallbackActionExecuted.Should().BeFalse();
    }

    [Fact]
    public void Should_not_execute_fallback_when_inner_exception_thrown_does_not_match_any_of_handling_predicates()
    {
        bool fallbackActionExecuted = false;
        Action fallbackAction = () => { fallbackActionExecuted = true; };

        FallbackPolicy fallbackPolicy = Policy
                                .HandleInner<DivideByZeroException>(_ => false)
                                .OrInner<ArgumentNullException>(_ => false)
                                .Fallback(fallbackAction);

        Exception withInner = new Exception(string.Empty, new ArgumentNullException());

        fallbackPolicy.Invoking(x => x.RaiseException(withInner)).Should().Throw<Exception>().And.InnerException.Should().BeOfType<ArgumentNullException>();

        fallbackActionExecuted.Should().BeFalse();
    }

    #endregion

    #region HandleInner tests, inner of aggregate exceptions

    [Fact]
    public void Should_not_execute_fallback_when_executed_delegate_throws_inner_of_aggregate_exception_where_policy_doesnt_handle_inner()
    {
        bool fallbackActionExecuted = false;
        Action fallbackAction = () => { fallbackActionExecuted = true; };

        FallbackPolicy fallbackPolicy = Policy
            .Handle<DivideByZeroException>()
            .Fallback(fallbackAction);

        Exception withInner = new AggregateException(new DivideByZeroException());

        fallbackPolicy.Invoking(x => x.RaiseException(withInner)).Should().Throw<AggregateException>().And.InnerExceptions.Should().ContainSingle(e => e is DivideByZeroException);

        fallbackActionExecuted.Should().BeFalse();
    }

    [Fact]
    public void Should_execute_fallback_when_executed_delegate_throws_inner_of_aggregate_exception_handled_by_policy()
    {
        bool fallbackActionExecuted = false;
        Action fallbackAction = () => { fallbackActionExecuted = true; };

        FallbackPolicy fallbackPolicy = Policy
            .HandleInner<DivideByZeroException>()
            .Fallback(fallbackAction);

        Exception withInner = new AggregateException(new DivideByZeroException());

        fallbackPolicy.Invoking(x => x.RaiseException(withInner)).Should().NotThrow();

        fallbackActionExecuted.Should().BeTrue();
    }

    [Fact]
    public void Should_execute_fallback_when_executed_delegate_throws_one_of_inner_of_aggregate_exceptions_handled_by_policy()
    {
        bool fallbackActionExecuted = false;
        Action fallbackAction = () => { fallbackActionExecuted = true; };

        FallbackPolicy fallbackPolicy = Policy
            .Handle<DivideByZeroException>()
            .OrInner<ArgumentException>()
            .Fallback(fallbackAction);

        Exception withInner = new AggregateException(new ArgumentException());

        fallbackPolicy.Invoking(x => x.RaiseException(withInner)).Should().NotThrow();

        fallbackActionExecuted.Should().BeTrue();
    }

    [Fact]
    public void Should_execute_fallback_when_executed_delegate_throws_aggregate_exception_with_inner_handled_by_policy_amongst_other_inners()
    {
        bool fallbackActionExecuted = false;
        Action fallbackAction = () => { fallbackActionExecuted = true; };

        FallbackPolicy fallbackPolicy = Policy
            .HandleInner<DivideByZeroException>()
            .Fallback(fallbackAction);

        Exception withInner = new AggregateException(new ArgumentException(), new DivideByZeroException(), new ArgumentNullException());

        fallbackPolicy.Invoking(x => x.RaiseException(withInner)).Should().NotThrow();

        fallbackActionExecuted.Should().BeTrue();
    }

    [Fact]
    public void Should_execute_fallback_when_executed_delegate_throws_nested_inner_of_aggregate_exception_handled_by_policy()
    {
        bool fallbackActionExecuted = false;
        Action fallbackAction = () => { fallbackActionExecuted = true; };

        FallbackPolicy fallbackPolicy = Policy
            .HandleInner<DivideByZeroException>()
            .Fallback(fallbackAction);

        Exception withInner = new AggregateException(new AggregateException(new DivideByZeroException()));

        fallbackPolicy.Invoking(x => x.RaiseException(withInner)).Should().NotThrow();

        fallbackActionExecuted.Should().BeTrue();
    }

    [Fact]
    public void Should_not_execute_fallback_when_executed_delegate_throws_inner_of_aggregate_exception_not_handled_by_policy()
    {
        bool fallbackActionExecuted = false;
        Action fallbackAction = () => { fallbackActionExecuted = true; };

        FallbackPolicy fallbackPolicy = Policy
                                .HandleInner<DivideByZeroException>()
                                .Fallback(fallbackAction);

        Exception withInner = new AggregateException(new ArgumentException());

        fallbackPolicy.Invoking(x => x.RaiseException(withInner)).Should().Throw<AggregateException>().And.InnerExceptions.Should().ContainSingle(e => e is ArgumentException);

        fallbackActionExecuted.Should().BeFalse();
    }

    [Fact]
    public void Should_execute_fallback_when_inner_of_aggregate_exception_thrown_matches_handling_predicates()
    {
        bool fallbackActionExecuted = false;
        Action fallbackAction = () => { fallbackActionExecuted = true; };

        FallbackPolicy fallbackPolicy = Policy
            .HandleInner<DivideByZeroException>(_ => true)
            .Fallback(fallbackAction);

        Exception withInner = new AggregateException(new DivideByZeroException());

        fallbackPolicy.Invoking(x => x.RaiseException(withInner)).Should().NotThrow();

        fallbackActionExecuted.Should().BeTrue();
    }

    [Fact]
    public void Should_execute_fallback_when_inner_of_aggregate_nested_exception_thrown_matches_handling_predicates()
    {
        bool fallbackActionExecuted = false;
        Action fallbackAction = () => { fallbackActionExecuted = true; };

        FallbackPolicy fallbackPolicy = Policy
            .HandleInner<DivideByZeroException>(_ => true)
            .Fallback(fallbackAction);

        Exception withInner = new AggregateException(new AggregateException(new DivideByZeroException()));

        fallbackPolicy.Invoking(x => x.RaiseException(withInner)).Should().NotThrow();

        fallbackActionExecuted.Should().BeTrue();
    }

    [Fact]
    public void Should_execute_fallback_when_inner_of_aggregate_exception_thrown_matches_one_of_handling_predicates()
    {
        bool fallbackActionExecuted = false;
        Action fallbackAction = () => { fallbackActionExecuted = true; };

        FallbackPolicy fallbackPolicy = Policy
            .HandleInner<DivideByZeroException>(_ => true)
            .OrInner<ArgumentNullException>(_ => true)
            .Fallback(fallbackAction);

        Exception withInner = new AggregateException(new ArgumentNullException());

        fallbackPolicy.Invoking(x => x.RaiseException(withInner)).Should().NotThrow();

        fallbackActionExecuted.Should().BeTrue();
    }

    [Fact]
    public void Should_not_execute_fallback_when_inner_of_aggregate_exception_thrown_does_not_match_handling_predicates()
    {
        bool fallbackActionExecuted = false;
        Action fallbackAction = () => { fallbackActionExecuted = true; };

        FallbackPolicy fallbackPolicy = Policy
                                .HandleInner<DivideByZeroException>(_ => false)
                                .Fallback(fallbackAction);

        Exception withInner = new AggregateException(new DivideByZeroException());

        fallbackPolicy.Invoking(x => x.RaiseException(withInner)).Should().Throw<AggregateException>().And.InnerExceptions.Should().ContainSingle(e => e is DivideByZeroException);

        fallbackActionExecuted.Should().BeFalse();
    }

    [Fact]
    public void Should_not_execute_fallback_when_inner_of_aggregate_exception_thrown_does_not_match_any_of_handling_predicates()
    {
        bool fallbackActionExecuted = false;
        Action fallbackAction = () => { fallbackActionExecuted = true; };

        FallbackPolicy fallbackPolicy = Policy
                                .HandleInner<DivideByZeroException>(_ => false)
                                .OrInner<ArgumentNullException>(_ => false)
                                .Fallback(fallbackAction);

        Exception withInner = new AggregateException(new ArgumentNullException());

        fallbackPolicy.Invoking(x => x.RaiseException(withInner)).Should().Throw<AggregateException>().And.InnerExceptions.Should().ContainSingle(e => e is ArgumentNullException);

        fallbackActionExecuted.Should().BeFalse();
    }

    #endregion

    #region onPolicyEvent delegate tests

    [Fact]
    public void Should_call_onFallback_passing_exception_triggering_fallback()
    {
        bool fallbackActionExecuted = false;
        Action fallbackAction = () => { fallbackActionExecuted = true; };

        Exception? exceptionPassedToOnFallback = null;
        Action<Exception> onFallback = ex => { exceptionPassedToOnFallback = ex; };

        FallbackPolicy fallbackPolicy = Policy
            .Handle<ArgumentNullException>()
            .Fallback(fallbackAction, onFallback);

        Exception instanceToThrow = new ArgumentNullException("myParam");
        fallbackPolicy.RaiseException(instanceToThrow);

        fallbackActionExecuted.Should().BeTrue();
        exceptionPassedToOnFallback.Should().BeOfType<ArgumentNullException>();
        exceptionPassedToOnFallback.Should().Be(instanceToThrow);
    }

    [Fact]
    public void Should_not_call_onFallback_when_executed_delegate_does_not_throw()
    {
        Action fallbackAction = () => { };

        bool onFallbackExecuted = false;
        Action<Exception> onFallback = _ => { onFallbackExecuted = true; };

        FallbackPolicy fallbackPolicy = Policy
            .Handle<DivideByZeroException>()
            .Fallback(fallbackAction, onFallback);

        fallbackPolicy.Execute(() => { });

        onFallbackExecuted.Should().BeFalse();
    }

    #endregion

    #region Context passing tests

    [Fact]
    public void Should_call_onFallback_with_the_passed_context()
    {
        Action<Context> fallbackAction = _ => { };

        IDictionary<string, object>? contextData = null;

        Action<Exception, Context> onFallback = (_, ctx) => { contextData = ctx; };

        FallbackPolicy fallbackPolicy = Policy
            .Handle<ArgumentNullException>()
            .Fallback(fallbackAction, onFallback);

        fallbackPolicy.Invoking(p => p.Execute(_ => throw new ArgumentNullException(),
            new { key1 = "value1", key2 = "value2" }.AsDictionary()))
            .Should().NotThrow();

        contextData.Should()
            .ContainKeys("key1", "key2").And
            .ContainValues("value1", "value2");
    }

    [Fact]
    public void Should_call_onFallback_with_the_passed_context_when_execute_and_capture()
    {
        Action<Context> fallbackAction = _ => { };

        IDictionary<string, object>? contextData = null;

        Action<Exception, Context> onFallback = (_, ctx) => { contextData = ctx; };

        FallbackPolicy fallbackPolicy = Policy
            .Handle<ArgumentNullException>()
            .Fallback(fallbackAction, onFallback);

        fallbackPolicy.Invoking(p => p.ExecuteAndCapture(_ => throw new ArgumentNullException(),
            new { key1 = "value1", key2 = "value2" }.AsDictionary()))
            .Should().NotThrow();

        contextData.Should()
            .ContainKeys("key1", "key2").And
            .ContainValues("value1", "value2");
    }

    [Fact]
    public void Should_call_onFallback_with_independent_context_for_independent_calls()
    {
        Action<Context> fallbackAction = _ => { };

        IDictionary<Type, object> contextData = new Dictionary<Type, object>();

        Action<Exception, Context> onFallback = (ex, ctx) => { contextData[ex.GetType()] = ctx["key"]; };

        FallbackPolicy fallbackPolicy = Policy
            .Handle<ArgumentNullException>()
            .Or<DivideByZeroException>()
            .Fallback(fallbackAction, onFallback);

        fallbackPolicy.Invoking(
            p => p.Execute(_ => throw new ArgumentNullException(), new { key = "value1" }.AsDictionary()))
            .Should().NotThrow();

        fallbackPolicy.Invoking(
            p => p.Execute(_ => throw new DivideByZeroException(), new { key = "value2" }.AsDictionary()))
            .Should().NotThrow();

        contextData.Count.Should().Be(2);
        contextData.Keys.Should().Contain(typeof(ArgumentNullException));
        contextData.Keys.Should().Contain(typeof(DivideByZeroException));
        contextData[typeof(ArgumentNullException)].Should().Be("value1");
        contextData[typeof(DivideByZeroException)].Should().Be("value2");

    }

    [Fact]
    public void Context_should_be_empty_if_execute_not_called_with_any_context_data()
    {
        Context? capturedContext = null;
        bool onFallbackExecuted = false;

        Action<Context> fallbackAction = _ => { };
        Action<Exception, Context> onFallback = (_, ctx) => { onFallbackExecuted = true; capturedContext = ctx; };

        FallbackPolicy fallbackPolicy = Policy
            .Handle<ArgumentNullException>()
            .Or<DivideByZeroException>()
            .Fallback(fallbackAction, onFallback);

        fallbackPolicy.RaiseException<DivideByZeroException>();

        onFallbackExecuted.Should().BeTrue();
        capturedContext.Should().BeEmpty();
    }

    [Fact]
    public void Should_call_fallbackAction_with_the_passed_context()
    {
        IDictionary<string, object>? contextData = null;

        Action<Context, CancellationToken> fallbackAction = (ctx, _) => { contextData = ctx; };

        Action<Exception, Context> onFallback = (_, _) => { };

        FallbackPolicy fallbackPolicy = Policy
            .Handle<ArgumentNullException>()
            .Fallback(fallbackAction, onFallback);

        fallbackPolicy.Invoking(p => p.Execute(_ => throw new ArgumentNullException(),
                new { key1 = "value1", key2 = "value2" }.AsDictionary()))
            .Should().NotThrow();

        contextData.Should()
            .ContainKeys("key1", "key2").And
            .ContainValues("value1", "value2");
    }

    [Fact]
    public void Should_call_fallbackAction_with_the_passed_context_when_execute_and_capture()
    {
        IDictionary<string, object>? contextData = null;

        Action<Context, CancellationToken> fallbackAction = (ctx, _) => { contextData = ctx; };

        Action<Exception, Context> onFallback = (_, _) => { };

        FallbackPolicy fallbackPolicy = Policy
            .Handle<ArgumentNullException>()
            .Fallback(fallbackAction, onFallback);

        fallbackPolicy.Invoking(p => p.ExecuteAndCapture(_ => throw new ArgumentNullException(),
                new { key1 = "value1", key2 = "value2" }.AsDictionary()))
            .Should().NotThrow();

        contextData.Should()
            .ContainKeys("key1", "key2").And
            .ContainValues("value1", "value2");
    }

    [Fact]
    public void Context_should_be_empty_at_fallbackAction_if_execute_not_called_with_any_context_data()
    {
        Context? capturedContext = null;
        bool fallbackExecuted = false;

        Action<Context, CancellationToken> fallbackAction = (ctx, _) => { fallbackExecuted = true; capturedContext = ctx; };
        Action<Exception, Context> onFallback = (_, _) => { };

        FallbackPolicy fallbackPolicy = Policy
            .Handle<ArgumentNullException>()
            .Or<DivideByZeroException>()
            .Fallback(fallbackAction, onFallback);

        fallbackPolicy.RaiseException<DivideByZeroException>();

        fallbackExecuted.Should().BeTrue();
        capturedContext.Should().BeEmpty();
    }

    #endregion

    #region Exception passing tests

    [Fact]
    public void Should_call_fallbackAction_with_the_exception()
    {
        Exception? fallbackException = null;

        Action<Exception, Context, CancellationToken> fallbackAction = (ex, _, _) => { fallbackException = ex; };

        Action<Exception, Context> onFallback = (_, _) => { };

        FallbackPolicy fallbackPolicy = Policy
            .Handle<ArgumentNullException>()
            .Fallback(fallbackAction, onFallback);

        Exception instanceToThrow = new ArgumentNullException("myParam");
        fallbackPolicy.Invoking(p => p.RaiseException(instanceToThrow))
            .Should().NotThrow();

        fallbackException.Should().Be(instanceToThrow);
    }

    [Fact]
    public void Should_call_fallbackAction_with_the_exception_when_execute_and_capture()
    {
        Exception? fallbackException = null;

        Action<Exception, Context, CancellationToken> fallbackAction = (ex, _, _) => { fallbackException = ex; };

        Action<Exception, Context> onFallback = (_, _) => { };

        FallbackPolicy fallbackPolicy = Policy
            .Handle<ArgumentNullException>()
            .Fallback(fallbackAction, onFallback);
        fallbackPolicy.Invoking(p => p.ExecuteAndCapture(() => throw new ArgumentNullException()))
            .Should().NotThrow();

        fallbackException.Should().NotBeNull()
            .And.BeOfType(typeof(ArgumentNullException));
    }

    [Fact]
    public void Should_call_fallbackAction_with_the_matched_inner_exception_unwrapped()
    {
        Exception? fallbackException = null;

        Action<Exception, Context, CancellationToken> fallbackAction = (ex, _, _) => { fallbackException = ex; };

        Action<Exception, Context> onFallback = (_, _) => { };

        FallbackPolicy fallbackPolicy = Policy
            .HandleInner<ArgumentNullException>()
            .Fallback(fallbackAction, onFallback);

        Exception instanceToCapture = new ArgumentNullException("myParam");
        Exception instanceToThrow = new Exception(string.Empty, instanceToCapture);
        fallbackPolicy.Invoking(p => p.RaiseException(instanceToThrow))
            .Should().NotThrow();

        fallbackException.Should().Be(instanceToCapture);
    }

    [Fact]
    public void Should_call_fallbackAction_with_the_matched_inner_of_aggregate_exception_unwrapped()
    {
        Exception? fallbackException = null;

        Action<Exception, Context, CancellationToken> fallbackAction = (ex, _, _) => { fallbackException = ex; };

        Action<Exception, Context> onFallback = (_, _) => { };

        FallbackPolicy fallbackPolicy = Policy
            .HandleInner<ArgumentNullException>()
            .Fallback(fallbackAction, onFallback);

        Exception instanceToCapture = new ArgumentNullException("myParam");
        Exception instanceToThrow = new AggregateException(instanceToCapture);
        fallbackPolicy.Invoking(p => p.RaiseException(instanceToThrow))
            .Should().NotThrow();

        fallbackException.Should().Be(instanceToCapture);
    }

    [Fact]
    public void Should_not_call_fallbackAction_with_the_exception_if_exception_unhandled()
    {
        Exception? fallbackException = null;

        Action<Exception, Context, CancellationToken> fallbackAction = (ex, _, _) =>
        {
            fallbackException = ex;
        };

        Action<Exception, Context> onFallback = (_, _) => { };

        FallbackPolicy fallbackPolicy = Policy
            .Handle<DivideByZeroException>()
            .Fallback(fallbackAction, onFallback);

        fallbackPolicy.Invoking(p => p.Execute(() => throw new ArgumentNullException()))
            .Should().Throw<ArgumentNullException>();

        fallbackException.Should().BeNull();
    }

    #endregion

    #region Cancellation tests

    [Fact]
    public void Should_execute_action_when_non_faulting_and_cancellationToken_not_cancelled()
    {
        bool fallbackActionExecuted = false;
        Action fallbackAction = () => { fallbackActionExecuted = true; };

        FallbackPolicy policy = Policy
                                .Handle<DivideByZeroException>()
                                .Fallback(fallbackAction);

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        Scenario scenario = new Scenario
        {
            NumberOfTimesToRaiseException = 0,
            AttemptDuringWhichToCancel = null,
        };

        using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource())
        {
            policy.Invoking(x => x.RaiseExceptionAndOrCancellation<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .Should().NotThrow();
        }

        attemptsInvoked.Should().Be(1);

        fallbackActionExecuted.Should().BeFalse();
    }

    [Fact]
    public void Should_execute_fallback_when_faulting_and_cancellationToken_not_cancelled()
    {
        bool fallbackActionExecuted = false;
        Action fallbackAction = () => { fallbackActionExecuted = true; };

        FallbackPolicy policy = Policy
                                .Handle<DivideByZeroException>()
                                .Fallback(fallbackAction);

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        Scenario scenario = new Scenario
        {
            NumberOfTimesToRaiseException = 1,
            AttemptDuringWhichToCancel = null,
        };

        using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource())
        {
            policy.Invoking(x => x.RaiseExceptionAndOrCancellation<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .Should().NotThrow();
        }

        attemptsInvoked.Should().Be(1);

        fallbackActionExecuted.Should().BeTrue();
    }

    [Fact]
    public void Should_not_execute_action_when_cancellationToken_cancelled_before_execute()
    {
        bool fallbackActionExecuted = false;
        Action fallbackAction = () => { fallbackActionExecuted = true; };

        FallbackPolicy policy = Policy
                                .Handle<DivideByZeroException>()
                                .Fallback(fallbackAction);

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        Scenario scenario = new Scenario
        {
            NumberOfTimesToRaiseException = 1,
            AttemptDuringWhichToCancel = null, // Cancellation token cancelled manually below - before any scenario execution.
        };

        using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            cancellationTokenSource.Cancel();

            policy.Invoking(x => x.RaiseExceptionAndOrCancellation<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .Should().Throw<OperationCanceledException>()
                .And.CancellationToken.Should().Be(cancellationToken);
        }

        attemptsInvoked.Should().Be(0);

        fallbackActionExecuted.Should().BeFalse();

    }

    [Fact]
    public void Should_report_cancellation_and_not_execute_fallback_during_otherwise_non_faulting_action_execution_when_user_delegate_observes_cancellationToken_and_fallback_does_not_handle_cancellations()
    {
        bool fallbackActionExecuted = false;
        Action fallbackAction = () => { fallbackActionExecuted = true; };

        FallbackPolicy policy = Policy
                                .Handle<DivideByZeroException>()
                                .Fallback(fallbackAction);

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        Scenario scenario = new Scenario
        {
            NumberOfTimesToRaiseException = 0,
            AttemptDuringWhichToCancel = 1,
            ActionObservesCancellation = true
        };

        using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;
            policy.Invoking(x => x.RaiseExceptionAndOrCancellation<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .Should().Throw<OperationCanceledException>()
                .And.CancellationToken.Should().Be(cancellationToken);
        }

        attemptsInvoked.Should().Be(1);

        fallbackActionExecuted.Should().BeFalse();
    }

    [Fact]
    public void Should_handle_cancellation_and_execute_fallback_during_otherwise_non_faulting_action_execution_when_user_delegate_observes_cancellationToken_and_fallback_handles_cancellations()
    {
        bool fallbackActionExecuted = false;
        Action fallbackAction = () => { fallbackActionExecuted = true; };

        FallbackPolicy policy = Policy
                                .Handle<DivideByZeroException>()
                                .Or<OperationCanceledException>()
                                .Fallback(fallbackAction);

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        Scenario scenario = new Scenario
        {
            NumberOfTimesToRaiseException = 0,
            AttemptDuringWhichToCancel = 1,
            ActionObservesCancellation = true
        };

        using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource())
        {
            policy.Invoking(x => x.RaiseExceptionAndOrCancellation<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .Should().NotThrow();
        }

        attemptsInvoked.Should().Be(1);

        fallbackActionExecuted.Should().BeTrue();
    }

    [Fact]
    public void Should_not_report_cancellation_and_not_execute_fallback_if_non_faulting_action_execution_completes_and_user_delegate_does_not_observe_the_set_cancellationToken()
    {
        bool fallbackActionExecuted = false;
        Action fallbackAction = () => { fallbackActionExecuted = true; };

        FallbackPolicy policy = Policy
                                .Handle<DivideByZeroException>()
                                .Fallback(fallbackAction);

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        Scenario scenario = new Scenario
        {
            NumberOfTimesToRaiseException = 0,
            AttemptDuringWhichToCancel = 1,
            ActionObservesCancellation = false
        };

        using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource())
        {
            policy.Invoking(x => x.RaiseExceptionAndOrCancellation<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .Should().NotThrow();
        }

        attemptsInvoked.Should().Be(1);
        fallbackActionExecuted.Should().BeFalse();
    }

    [Fact]
    public void Should_report_unhandled_fault_and_not_execute_fallback_if_action_execution_raises_unhandled_fault_and_user_delegate_does_not_observe_the_set_cancellationToken()
    {
        bool fallbackActionExecuted = false;
        Action fallbackAction = () => { fallbackActionExecuted = true; };

        FallbackPolicy policy = Policy
                                .Handle<DivideByZeroException>()
                                .Fallback(fallbackAction);

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        Scenario scenario = new Scenario
        {
            NumberOfTimesToRaiseException = 1,
            AttemptDuringWhichToCancel = 1,
            ActionObservesCancellation = false
        };

        using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource())
        {
            policy.Invoking(x => x.RaiseExceptionAndOrCancellation<NullReferenceException>(scenario, cancellationTokenSource, onExecute))
                .Should().Throw<NullReferenceException>();
        }

        attemptsInvoked.Should().Be(1);

        fallbackActionExecuted.Should().BeFalse();
    }

    [Fact]
    public void Should_handle_handled_fault_and_execute_fallback_following_faulting_action_execution_when_user_delegate_does_not_observe_cancellationToken()
    {
        bool fallbackActionExecuted = false;
        Action fallbackAction = () => { fallbackActionExecuted = true; };

        FallbackPolicy policy = Policy
                                .Handle<DivideByZeroException>()
                                .Fallback(fallbackAction);

        int attemptsInvoked = 0;
        Action onExecute = () => attemptsInvoked++;

        Scenario scenario = new Scenario
        {
            NumberOfTimesToRaiseException = 1,
            AttemptDuringWhichToCancel = 1,
            ActionObservesCancellation = false
        };

        using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource())
        {
            policy.Invoking(x => x.RaiseExceptionAndOrCancellation<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .Should().NotThrow();
        }

        attemptsInvoked.Should().Be(1);

        fallbackActionExecuted.Should().BeTrue();
    }

    #endregion
}
