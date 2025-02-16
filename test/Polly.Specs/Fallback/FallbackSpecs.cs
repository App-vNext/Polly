using Scenario = Polly.Specs.Helpers.PolicyExtensions.ExceptionAndOrCancellationScenario;

namespace Polly.Specs.Fallback;

public class FallbackSpecs
{
    #region Configuration guard condition tests

    [Fact]
    public void Should_not_throw_when_fallback_action_is__not_null()
    {
        Action policy = () => Policy
            .Handle<DivideByZeroException>()
            .Fallback(() => { });

        Should.NotThrow(policy);

        policy = () => Policy
            .Handle<DivideByZeroException>()
            .Fallback((_) => { });

        Should.NotThrow(policy);
    }

    [Fact]
    public void Should_throw_when_fallback_action_is_null()
    {
        Action fallbackAction = null!;

        Action policy = () => Policy
            .Handle<DivideByZeroException>()
            .Fallback(fallbackAction);

        Should.Throw<ArgumentNullException>(policy)
            .ParamName.ShouldBe("fallbackAction");

        Action<CancellationToken> fallbackActionToken = null!;

        policy = () => Policy
            .Handle<DivideByZeroException>()
            .Fallback(fallbackActionToken);

        Should.Throw<ArgumentNullException>(policy)
            .ParamName.ShouldBe("fallbackAction");
    }

    [Fact]
    public void Should_throw_when_fallback_action_with_cancellation_is_null()
    {
        Action<CancellationToken> fallbackAction = null!;

        Action policy = () => Policy
            .Handle<DivideByZeroException>()
            .Fallback(fallbackAction);

        Should.Throw<ArgumentNullException>(policy)
            .ParamName.ShouldBe("fallbackAction");
    }

    [Fact]
    public void Should_throw_when_fallback_action_is_null_with_onFallback()
    {
        Action fallbackAction = null!;
        Action<Exception> onFallback = _ => { };

        Action policy = () => Policy
            .Handle<DivideByZeroException>()
            .Fallback(fallbackAction, onFallback);

        Should.Throw<ArgumentNullException>(policy)
            .ParamName.ShouldBe("fallbackAction");
    }

    [Fact]
    public void Should_throw_when_fallback_action_with_cancellation_is_null_with_onFallback()
    {
        Action<CancellationToken> fallbackAction = null!;
        Action<Exception> onFallback = _ => { };

        Action policy = () => Policy
            .Handle<DivideByZeroException>()
            .Fallback(fallbackAction, onFallback);

        Should.Throw<ArgumentNullException>(policy)
            .ParamName.ShouldBe("fallbackAction");
    }

    [Fact]
    public void Should_throw_when_fallback_action_is_null_with_onFallback_with_context()
    {
        Action<Context> fallbackAction = null!;
        Action<Exception, Context> onFallback = (_, _) => { };

        Action policy = () => Policy
            .Handle<DivideByZeroException>()
            .Fallback(fallbackAction, onFallback);

        Should.Throw<ArgumentNullException>(policy)
            .ParamName.ShouldBe("fallbackAction");
    }

    [Fact]
    public void Should_throw_when_fallback_action_with_cancellation_is_null_with_onFallback_with_context()
    {
        Action<Context, CancellationToken> fallbackAction = null!;
        Action<Exception, Context> onFallback = (_, _) => { };

        Action policy = () => Policy
            .Handle<DivideByZeroException>()
            .Fallback(fallbackAction, onFallback);

        Should.Throw<ArgumentNullException>(policy)
            .ParamName.ShouldBe("fallbackAction");
    }

    [Fact]
    public void Should_throw_when_onFallback_delegate_is_null()
    {
        Action fallbackAction = () => { };
        Action<Exception> onFallback = null!;

        Action policy = () => Policy
            .Handle<DivideByZeroException>()
            .Fallback(fallbackAction, onFallback);

        Should.Throw<ArgumentNullException>(policy)
            .ParamName.ShouldBe("onFallback");
    }

    [Fact]
    public void Should_throw_when_onFallback_delegate_is_null_with_action_with_cancellation()
    {
        Action<CancellationToken> fallbackAction = _ => { };
        Action<Exception> onFallback = null!;

        Action policy = () => Policy
            .Handle<DivideByZeroException>()
            .Fallback(fallbackAction, onFallback);

        Should.Throw<ArgumentNullException>(policy)
            .ParamName.ShouldBe("onFallback");
    }

    [Fact]
    public void Should_throw_when_onFallback_delegate_is_null_with_context()
    {
        Action<Context> fallbackAction = _ => { };
        Action<Exception, Context> onFallback = null!;

        Action policy = () => Policy
            .Handle<DivideByZeroException>()
            .Fallback(fallbackAction, onFallback);

        Should.Throw<ArgumentNullException>(policy)
            .ParamName.ShouldBe("onFallback");
    }

    [Fact]
    public void Should_throw_when_onFallback_delegate_is_null_with_context_with_action_with_cancellation()
    {
        Action<Context, CancellationToken> fallbackAction = (_, _) => { };
        Action<Exception, Context> onFallback = null!;

        Action policy = () => Policy
            .Handle<DivideByZeroException>()
            .Fallback(fallbackAction, onFallback);

        Should.Throw<ArgumentNullException>(policy)
            .ParamName.ShouldBe("onFallback");
    }

    #endregion

    #region Policy operation tests

    [Fact]
    public void Should_not_execute_fallback_when_executed_delegate_does_not_throw()
    {
        bool fallbackActionExecuted = false;
        Action fallbackAction = () => fallbackActionExecuted = true;

        FallbackPolicy fallbackPolicy = Policy
            .Handle<DivideByZeroException>()
            .Fallback(fallbackAction);

        fallbackPolicy.Execute(() => { });

        fallbackActionExecuted.ShouldBeFalse();
    }

    [Fact]
    public void Should_not_execute_fallback_when_executed_delegate_throws_exception_not_handled_by_policy()
    {
        bool fallbackActionExecuted = false;
        Action fallbackAction = () => fallbackActionExecuted = true;

        FallbackPolicy fallbackPolicy = Policy
            .Handle<DivideByZeroException>()
            .Fallback(fallbackAction);

        Should.Throw<ArgumentNullException>(() => fallbackPolicy.RaiseException<ArgumentNullException>());

        fallbackActionExecuted.ShouldBeFalse();
    }

    [Fact]
    public void Should_execute_fallback_when_executed_delegate_throws_exception_handled_by_policy()
    {
        bool fallbackActionExecuted = false;
        Action fallbackAction = () => fallbackActionExecuted = true;

        FallbackPolicy fallbackPolicy = Policy
            .Handle<DivideByZeroException>()
            .Fallback(fallbackAction);

        Should.NotThrow(() => fallbackPolicy.RaiseException<DivideByZeroException>());

        fallbackActionExecuted.ShouldBeTrue();
    }

    [Fact]
    public void Should_execute_fallback_when_executed_delegate_throws_one_of_exceptions_handled_by_policy()
    {
        bool fallbackActionExecuted = false;
        Action fallbackAction = () => fallbackActionExecuted = true;

        FallbackPolicy fallbackPolicy = Policy
            .Handle<DivideByZeroException>()
            .Or<ArgumentException>()
            .Fallback(fallbackAction);

        Should.NotThrow(() => fallbackPolicy.RaiseException<ArgumentException>());

        fallbackActionExecuted.ShouldBeTrue();
    }

    [Fact]
    public void Should_not_execute_fallback_when_executed_delegate_throws_exception_not_one_of_exceptions_handled_by_policy()
    {
        bool fallbackActionExecuted = false;
        Action fallbackAction = () => fallbackActionExecuted = true;

        FallbackPolicy fallbackPolicy = Policy
            .Handle<DivideByZeroException>()
            .Or<NullReferenceException>()
            .Fallback(fallbackAction);

        Should.Throw<ArgumentNullException>(() => fallbackPolicy.RaiseException<ArgumentNullException>());

        fallbackActionExecuted.ShouldBeFalse();
    }

    [Fact]
    public void Should_not_execute_fallback_when_exception_thrown_does_not_match_handling_predicates()
    {
        bool fallbackActionExecuted = false;
        Action fallbackAction = () => fallbackActionExecuted = true;

        FallbackPolicy fallbackPolicy = Policy
            .Handle<DivideByZeroException>(_ => false)
            .Fallback(fallbackAction);

        Should.Throw<DivideByZeroException>(() => fallbackPolicy.RaiseException<DivideByZeroException>());

        fallbackActionExecuted.ShouldBeFalse();
    }

    [Fact]
    public void Should_not_execute_fallback_when_exception_thrown_does_not_match_any_of_handling_predicates()
    {
        bool fallbackActionExecuted = false;
        Action fallbackAction = () => fallbackActionExecuted = true;

        FallbackPolicy fallbackPolicy = Policy
            .Handle<DivideByZeroException>(_ => false)
            .Or<ArgumentNullException>(_ => false)
            .Fallback(fallbackAction);

        Should.Throw<DivideByZeroException>(() => fallbackPolicy.RaiseException<DivideByZeroException>());

        fallbackActionExecuted.ShouldBeFalse();
    }

    [Fact]
    public void Should_execute_fallback_when_exception_thrown_matches_handling_predicates()
    {
        bool fallbackActionExecuted = false;
        Action fallbackAction = () => fallbackActionExecuted = true;

        FallbackPolicy fallbackPolicy = Policy
            .Handle<DivideByZeroException>(_ => true)
            .Fallback(fallbackAction);

        Should.NotThrow(() => fallbackPolicy.RaiseException<DivideByZeroException>());

        fallbackActionExecuted.ShouldBeTrue();
    }

    [Fact]
    public void Should_execute_fallback_when_exception_thrown_matches_one_of_handling_predicates()
    {
        bool fallbackActionExecuted = false;
        Action fallbackAction = () => fallbackActionExecuted = true;

        FallbackPolicy fallbackPolicy = Policy
            .Handle<DivideByZeroException>(_ => true)
            .Or<ArgumentNullException>()
            .Fallback(fallbackAction);

        Should.NotThrow(() => fallbackPolicy.RaiseException<DivideByZeroException>());

        fallbackActionExecuted.ShouldBeTrue();
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

        Should.Throw<DivideByZeroException>(() => fallbackPolicy.RaiseException<DivideByZeroException>((e, _) => e.HelpLink = "FromExecuteDelegate"))
            .HelpLink.ShouldBe("FromFallbackAction");

        fallbackActionExecuted.ShouldBeTrue();
    }

    [Fact]
    public void Should_throw_for_generic_method_execution_on_non_generic_policy()
    {
        FallbackPolicy fallbackPolicy = Policy
            .Handle<DivideByZeroException>()
            .Fallback(() => { });

        Should.Throw<InvalidOperationException>(() => fallbackPolicy.Execute(() => 0));
    }

    #endregion

    #region HandleInner tests, inner of normal exceptions

    [Fact]
    public void Should_not_execute_fallback_when_executed_delegate_throws_inner_exception_where_policy_doesnt_handle_inner()
    {
        bool fallbackActionExecuted = false;
        Action fallbackAction = () => fallbackActionExecuted = true;

        FallbackPolicy fallbackPolicy = Policy
            .Handle<DivideByZeroException>()
            .Fallback(fallbackAction);

        Exception withInner = new Exception(string.Empty, new DivideByZeroException());

        Should.Throw<Exception>(() => fallbackPolicy.RaiseException(withInner)).InnerException.ShouldBeOfType<DivideByZeroException>();

        fallbackActionExecuted.ShouldBeFalse();
    }

    [Fact]
    public void Should_execute_fallback_when_policy_handles_inner_and_executed_delegate_throws_as_non_inner()
    {
        bool fallbackActionExecuted = false;
        Action fallbackAction = () => fallbackActionExecuted = true;

        FallbackPolicy fallbackPolicy = Policy
            .HandleInner<DivideByZeroException>()
            .Fallback(fallbackAction);

        Exception nonInner = new DivideByZeroException();

        Should.NotThrow(() => fallbackPolicy.RaiseException(nonInner));

        fallbackActionExecuted.ShouldBeTrue();
    }

    [Fact]
    public void Should_execute_fallback_when_executed_delegate_throws_inner_exception_handled_by_policy()
    {
        bool fallbackActionExecuted = false;
        Action fallbackAction = () => fallbackActionExecuted = true;

        FallbackPolicy fallbackPolicy = Policy
            .HandleInner<DivideByZeroException>()
            .Fallback(fallbackAction);

        Exception withInner = new Exception(string.Empty, new DivideByZeroException());

        Should.NotThrow(() => fallbackPolicy.RaiseException(withInner));

        fallbackActionExecuted.ShouldBeTrue();
    }

    [Fact]
    public void Should_execute_fallback_when_executed_delegate_throws_one_of_inner_exceptions_handled_by_policy()
    {
        bool fallbackActionExecuted = false;
        Action fallbackAction = () => fallbackActionExecuted = true;

        FallbackPolicy fallbackPolicy = Policy
            .Handle<DivideByZeroException>()
            .OrInner<ArgumentException>()
            .Fallback(fallbackAction);

        Exception withInner = new Exception(string.Empty, new ArgumentException());

        Should.NotThrow(() => fallbackPolicy.RaiseException(withInner));

        fallbackActionExecuted.ShouldBeTrue();
    }

    [Fact]
    public void Should_execute_fallback_when_executed_delegate_throws_nested_inner_exception_handled_by_policy()
    {
        bool fallbackActionExecuted = false;
        Action fallbackAction = () => fallbackActionExecuted = true;

        FallbackPolicy fallbackPolicy = Policy
            .HandleInner<DivideByZeroException>()
            .Fallback(fallbackAction);

        Exception withInner = new Exception(string.Empty, new Exception(string.Empty, new Exception(string.Empty, new DivideByZeroException())));

        Should.NotThrow(() => fallbackPolicy.RaiseException(withInner));

        fallbackActionExecuted.ShouldBeTrue();
    }

    [Fact]
    public void Should_not_execute_fallback_when_executed_delegate_throws_inner_exception_not_handled_by_policy()
    {
        bool fallbackActionExecuted = false;
        Action fallbackAction = () => fallbackActionExecuted = true;

        FallbackPolicy fallbackPolicy = Policy
            .HandleInner<DivideByZeroException>()
            .Fallback(fallbackAction);

        Exception withInner = new Exception(string.Empty, new ArgumentException());

        Should.Throw<Exception>(() => fallbackPolicy.RaiseException(withInner)).InnerException.ShouldBeOfType<ArgumentException>();

        fallbackActionExecuted.ShouldBeFalse();
    }

    [Fact]
    public void Should_execute_fallback_when_inner_exception_thrown_matches_handling_predicates()
    {
        bool fallbackActionExecuted = false;
        Action fallbackAction = () => fallbackActionExecuted = true;

        FallbackPolicy fallbackPolicy = Policy
            .HandleInner<DivideByZeroException>(_ => true)
            .Fallback(fallbackAction);

        Exception withInner = new Exception(string.Empty, new DivideByZeroException());

        Should.NotThrow(() => fallbackPolicy.RaiseException(withInner));

        fallbackActionExecuted.ShouldBeTrue();
    }

    [Fact]
    public void Should_execute_fallback_when_inner_nested_exception_thrown_matches_handling_predicates()
    {
        bool fallbackActionExecuted = false;
        Action fallbackAction = () => fallbackActionExecuted = true;

        FallbackPolicy fallbackPolicy = Policy
            .HandleInner<DivideByZeroException>(_ => true)
            .Fallback(fallbackAction);

        Exception withInner = new Exception(string.Empty, new Exception(string.Empty, new Exception(string.Empty, new DivideByZeroException())));

        Should.NotThrow(() => fallbackPolicy.RaiseException(withInner));

        fallbackActionExecuted.ShouldBeTrue();
    }

    [Fact]
    public void Should_execute_fallback_when_inner_exception_thrown_matches_one_of_handling_predicates()
    {
        bool fallbackActionExecuted = false;
        Action fallbackAction = () => fallbackActionExecuted = true;

        FallbackPolicy fallbackPolicy = Policy
            .HandleInner<DivideByZeroException>(_ => true)
            .OrInner<ArgumentNullException>(_ => true)
            .Fallback(fallbackAction);

        Exception withInner = new Exception(string.Empty, new ArgumentNullException());

        Should.NotThrow(() => fallbackPolicy.RaiseException(withInner));

        fallbackActionExecuted.ShouldBeTrue();
    }

    [Fact]
    public void Should_not_execute_fallback_when_inner_exception_thrown_does_not_match_handling_predicates()
    {
        bool fallbackActionExecuted = false;
        Action fallbackAction = () => fallbackActionExecuted = true;

        FallbackPolicy fallbackPolicy = Policy
            .HandleInner<DivideByZeroException>(_ => false)
            .Fallback(fallbackAction);

        Exception withInner = new Exception(string.Empty, new DivideByZeroException());

        Should.Throw<Exception>(() => fallbackPolicy.RaiseException(withInner)).InnerException.ShouldBeOfType<DivideByZeroException>();

        fallbackActionExecuted.ShouldBeFalse();
    }

    [Fact]
    public void Should_not_execute_fallback_when_inner_exception_thrown_does_not_match_any_of_handling_predicates()
    {
        bool fallbackActionExecuted = false;
        Action fallbackAction = () => fallbackActionExecuted = true;

        FallbackPolicy fallbackPolicy = Policy
            .HandleInner<DivideByZeroException>(_ => false)
            .OrInner<ArgumentNullException>(_ => false)
            .Fallback(fallbackAction);

        Exception withInner = new Exception(string.Empty, new ArgumentNullException());

        Should.Throw<Exception>(() => fallbackPolicy.RaiseException(withInner)).InnerException.ShouldBeOfType<ArgumentNullException>();

        fallbackActionExecuted.ShouldBeFalse();
    }

    #endregion

    #region HandleInner tests, inner of aggregate exceptions

    [Fact]
    public void Should_not_execute_fallback_when_executed_delegate_throws_inner_of_aggregate_exception_where_policy_doesnt_handle_inner()
    {
        bool fallbackActionExecuted = false;
        Action fallbackAction = () => fallbackActionExecuted = true;

        FallbackPolicy fallbackPolicy = Policy
            .Handle<DivideByZeroException>()
            .Fallback(fallbackAction);

        Exception withInner = new AggregateException(new DivideByZeroException());

        Should.Throw<AggregateException>(() => fallbackPolicy.RaiseException(withInner)).InnerExceptions.Count(e => e is DivideByZeroException).ShouldBe(1);

        fallbackActionExecuted.ShouldBeFalse();
    }

    [Fact]
    public void Should_execute_fallback_when_executed_delegate_throws_inner_of_aggregate_exception_handled_by_policy()
    {
        bool fallbackActionExecuted = false;
        Action fallbackAction = () => fallbackActionExecuted = true;

        FallbackPolicy fallbackPolicy = Policy
            .HandleInner<DivideByZeroException>()
            .Fallback(fallbackAction);

        Exception withInner = new AggregateException(new DivideByZeroException());

        Should.NotThrow(() => fallbackPolicy.RaiseException(withInner));

        fallbackActionExecuted.ShouldBeTrue();
    }

    [Fact]
    public void Should_execute_fallback_when_executed_delegate_throws_one_of_inner_of_aggregate_exceptions_handled_by_policy()
    {
        bool fallbackActionExecuted = false;
        Action fallbackAction = () => fallbackActionExecuted = true;

        FallbackPolicy fallbackPolicy = Policy
            .Handle<DivideByZeroException>()
            .OrInner<ArgumentException>()
            .Fallback(fallbackAction);

        Exception withInner = new AggregateException(new ArgumentException());

        Should.NotThrow(() => fallbackPolicy.RaiseException(withInner));

        fallbackActionExecuted.ShouldBeTrue();
    }

    [Fact]
    public void Should_execute_fallback_when_executed_delegate_throws_aggregate_exception_with_inner_handled_by_policy_amongst_other_inners()
    {
        bool fallbackActionExecuted = false;
        Action fallbackAction = () => fallbackActionExecuted = true;

        FallbackPolicy fallbackPolicy = Policy
            .HandleInner<DivideByZeroException>()
            .Fallback(fallbackAction);

        Exception withInner = new AggregateException(new ArgumentException(), new DivideByZeroException(), new ArgumentNullException());

        Should.NotThrow(() => fallbackPolicy.RaiseException(withInner));

        fallbackActionExecuted.ShouldBeTrue();
    }

    [Fact]
    public void Should_execute_fallback_when_executed_delegate_throws_nested_inner_of_aggregate_exception_handled_by_policy()
    {
        bool fallbackActionExecuted = false;
        Action fallbackAction = () => fallbackActionExecuted = true;

        FallbackPolicy fallbackPolicy = Policy
            .HandleInner<DivideByZeroException>()
            .Fallback(fallbackAction);

        Exception withInner = new AggregateException(new AggregateException(new DivideByZeroException()));

        Should.NotThrow(() => fallbackPolicy.RaiseException(withInner));

        fallbackActionExecuted.ShouldBeTrue();
    }

    [Fact]
    public void Should_not_execute_fallback_when_executed_delegate_throws_inner_of_aggregate_exception_not_handled_by_policy()
    {
        bool fallbackActionExecuted = false;
        Action fallbackAction = () => fallbackActionExecuted = true;

        FallbackPolicy fallbackPolicy = Policy
            .HandleInner<DivideByZeroException>()
            .Fallback(fallbackAction);

        Exception withInner = new AggregateException(new ArgumentException());

        Should.Throw<AggregateException>(() => fallbackPolicy.RaiseException(withInner)).InnerExceptions
            .Count(e => e is ArgumentException)
            .ShouldBe(1);

        fallbackActionExecuted.ShouldBeFalse();
    }

    [Fact]
    public void Should_execute_fallback_when_inner_of_aggregate_exception_thrown_matches_handling_predicates()
    {
        bool fallbackActionExecuted = false;
        Action fallbackAction = () => fallbackActionExecuted = true;

        FallbackPolicy fallbackPolicy = Policy
            .HandleInner<DivideByZeroException>(_ => true)
            .Fallback(fallbackAction);

        Exception withInner = new AggregateException(new DivideByZeroException());

        Should.NotThrow(() => fallbackPolicy.RaiseException(withInner));

        fallbackActionExecuted.ShouldBeTrue();
    }

    [Fact]
    public void Should_execute_fallback_when_inner_of_aggregate_nested_exception_thrown_matches_handling_predicates()
    {
        bool fallbackActionExecuted = false;
        Action fallbackAction = () => fallbackActionExecuted = true;

        FallbackPolicy fallbackPolicy = Policy
            .HandleInner<DivideByZeroException>(_ => true)
            .Fallback(fallbackAction);

        Exception withInner = new AggregateException(new AggregateException(new DivideByZeroException()));

        Should.NotThrow(() => fallbackPolicy.RaiseException(withInner));

        fallbackActionExecuted.ShouldBeTrue();
    }

    [Fact]
    public void Should_execute_fallback_when_inner_of_aggregate_exception_thrown_matches_one_of_handling_predicates()
    {
        bool fallbackActionExecuted = false;
        Action fallbackAction = () => fallbackActionExecuted = true;

        FallbackPolicy fallbackPolicy = Policy
            .HandleInner<DivideByZeroException>(_ => true)
            .OrInner<ArgumentNullException>(_ => true)
            .Fallback(fallbackAction);

        Exception withInner = new AggregateException(new ArgumentNullException());

        Should.NotThrow(() => fallbackPolicy.RaiseException(withInner));

        fallbackActionExecuted.ShouldBeTrue();
    }

    [Fact]
    public void Should_not_execute_fallback_when_inner_of_aggregate_exception_thrown_does_not_match_handling_predicates()
    {
        bool fallbackActionExecuted = false;
        Action fallbackAction = () => fallbackActionExecuted = true;

        FallbackPolicy fallbackPolicy = Policy
            .HandleInner<DivideByZeroException>(_ => false)
            .Fallback(fallbackAction);

        Exception withInner = new AggregateException(new DivideByZeroException());

        Should.Throw<AggregateException>(() => fallbackPolicy.RaiseException(withInner)).InnerExceptions.Count(e => e is DivideByZeroException).ShouldBe(1);

        fallbackActionExecuted.ShouldBeFalse();
    }

    [Fact]
    public void Should_not_execute_fallback_when_inner_of_aggregate_exception_thrown_does_not_match_any_of_handling_predicates()
    {
        bool fallbackActionExecuted = false;
        Action fallbackAction = () => fallbackActionExecuted = true;

        FallbackPolicy fallbackPolicy = Policy
            .HandleInner<DivideByZeroException>(_ => false)
            .OrInner<ArgumentNullException>(_ => false)
            .Fallback(fallbackAction);

        Exception withInner = new AggregateException(new ArgumentNullException());

        Should.Throw<AggregateException>(() => fallbackPolicy.RaiseException(withInner)).InnerExceptions.Count(e => e is ArgumentNullException).ShouldBe(1);

        fallbackActionExecuted.ShouldBeFalse();
    }

    #endregion

    #region onPolicyEvent delegate tests

    [Fact]
    public void Should_call_onFallback_passing_exception_triggering_fallback()
    {
        bool fallbackActionExecuted = false;
        Action fallbackAction = () => fallbackActionExecuted = true;

        Exception? exceptionPassedToOnFallback = null;
        Action<Exception> onFallback = ex => exceptionPassedToOnFallback = ex;

        FallbackPolicy fallbackPolicy = Policy
            .Handle<ArgumentNullException>()
            .Fallback(fallbackAction, onFallback);

        Exception instanceToThrow = new ArgumentNullException("myParam");
        fallbackPolicy.RaiseException(instanceToThrow);

        fallbackActionExecuted.ShouldBeTrue();
        exceptionPassedToOnFallback.ShouldBeOfType<ArgumentNullException>();
        exceptionPassedToOnFallback.ShouldBe(instanceToThrow);
    }

    [Fact]
    public void Should_not_call_onFallback_when_executed_delegate_does_not_throw()
    {
        Action fallbackAction = () => { };

        bool onFallbackExecuted = false;
        Action<Exception> onFallback = _ => onFallbackExecuted = true;

        FallbackPolicy fallbackPolicy = Policy
            .Handle<DivideByZeroException>()
            .Fallback(fallbackAction, onFallback);

        fallbackPolicy.Execute(() => { });

        onFallbackExecuted.ShouldBeFalse();
    }

    #endregion

    #region Context passing tests

    [Fact]
    public void Should_call_onFallback_with_the_passed_context()
    {
        Action<Context> fallbackAction = _ => { };

        IDictionary<string, object>? contextData = null;

        Action<Exception, Context> onFallback = (_, ctx) => contextData = ctx;

        FallbackPolicy fallbackPolicy = Policy
            .Handle<ArgumentNullException>()
            .Fallback(fallbackAction, onFallback);

        Should.NotThrow(
            () =>
                fallbackPolicy.Execute(
                    _ => throw new ArgumentNullException(),
                    CreateDictionary("key1", "value1", "key2", "value2")));

        contextData.ShouldNotBeNull();
        contextData.ShouldContainKeyAndValue("key1", "value1");
        contextData.ShouldContainKeyAndValue("key2", "value2");
    }

    [Fact]
    public void Should_call_onFallback_with_the_passed_context_when_execute_and_capture()
    {
        Action<Context> fallbackAction = _ => { };

        IDictionary<string, object>? contextData = null;

        Action<Exception, Context> onFallback = (_, ctx) => contextData = ctx;

        FallbackPolicy fallbackPolicy = Policy
            .Handle<ArgumentNullException>()
            .Fallback(fallbackAction, onFallback);

        Should.NotThrow(
            () =>
                fallbackPolicy.ExecuteAndCapture(
                    _ => throw new ArgumentNullException(),
                    CreateDictionary("key1", "value1", "key2", "value2")));

        contextData.ShouldNotBeNull();
        contextData.ShouldContainKeyAndValue("key1", "value1");
        contextData.ShouldContainKeyAndValue("key2", "value2");
    }

    [Fact]
    public void Should_call_onFallback_with_independent_context_for_independent_calls()
    {
        Action<Context> fallbackAction = _ => { };

        IDictionary<Type, object> contextData = new Dictionary<Type, object>();

        Action<Exception, Context> onFallback = (ex, ctx) => contextData[ex.GetType()] = ctx["key"];

        FallbackPolicy fallbackPolicy = Policy
            .Handle<ArgumentNullException>()
            .Or<DivideByZeroException>()
            .Fallback(fallbackAction, onFallback);

        Should.NotThrow(() => fallbackPolicy.Execute(_ => throw new ArgumentNullException(), CreateDictionary("key", "value1")));
        Should.NotThrow(() => fallbackPolicy.Execute(_ => throw new DivideByZeroException(), CreateDictionary("key", "value2")));

        contextData.Count.ShouldBe(2);
        contextData.Keys.ShouldContain(typeof(ArgumentNullException));
        contextData.Keys.ShouldContain(typeof(DivideByZeroException));
        contextData[typeof(ArgumentNullException)].ShouldBe("value1");
        contextData[typeof(DivideByZeroException)].ShouldBe("value2");

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

        onFallbackExecuted.ShouldBeTrue();
        capturedContext.ShouldBeEmpty();
    }

    [Fact]
    public void Should_call_fallbackAction_with_the_passed_context()
    {
        IDictionary<string, object>? contextData = null;

        Action<Context, CancellationToken> fallbackAction = (ctx, _) => contextData = ctx;

        Action<Exception, Context> onFallback = (_, _) => { };

        FallbackPolicy fallbackPolicy = Policy
            .Handle<ArgumentNullException>()
            .Fallback(fallbackAction, onFallback);

        Should.NotThrow(
            () =>
                fallbackPolicy.Execute(
                    _ => throw new ArgumentNullException(),
                    CreateDictionary("key1", "value1", "key2", "value2")));

        contextData.ShouldNotBeNull();
        contextData.ShouldContainKeyAndValue("key1", "value1");
        contextData.ShouldContainKeyAndValue("key2", "value2");
    }

    [Fact]
    public void Should_call_fallbackAction_with_the_passed_context_when_execute_and_capture()
    {
        IDictionary<string, object>? contextData = null;

        Action<Context, CancellationToken> fallbackAction = (ctx, _) => contextData = ctx;

        Action<Exception, Context> onFallback = (_, _) => { };

        FallbackPolicy fallbackPolicy = Policy
            .Handle<ArgumentNullException>()
            .Fallback(fallbackAction, onFallback);

        Should.NotThrow(
            () =>
                fallbackPolicy.ExecuteAndCapture(
                    _ => throw new ArgumentNullException(),
                    CreateDictionary("key1", "value1", "key2", "value2")));

        contextData.ShouldNotBeNull();
        contextData.ShouldContainKeyAndValue("key1", "value1");
        contextData.ShouldContainKeyAndValue("key2", "value2");
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

        fallbackExecuted.ShouldBeTrue();
        capturedContext.ShouldBeEmpty();
    }

    #endregion

    #region Exception passing tests

    [Fact]
    public void Should_call_fallbackAction_with_the_exception()
    {
        Exception? fallbackException = null;

        Action<Exception, Context, CancellationToken> fallbackAction = (ex, _, _) => fallbackException = ex;

        Action<Exception, Context> onFallback = (_, _) => { };

        FallbackPolicy fallbackPolicy = Policy
            .Handle<ArgumentNullException>()
            .Fallback(fallbackAction, onFallback);

        Exception instanceToThrow = new ArgumentNullException("myParam");
        Should.NotThrow(() => fallbackPolicy.RaiseException(instanceToThrow));

        fallbackException.ShouldBe(instanceToThrow);
    }

    [Fact]
    public void Should_call_fallbackAction_with_the_exception_when_execute_and_capture()
    {
        Exception? fallbackException = null;

        Action<Exception, Context, CancellationToken> fallbackAction = (ex, _, _) => fallbackException = ex;

        Action<Exception, Context> onFallback = (_, _) => { };

        FallbackPolicy fallbackPolicy = Policy
            .Handle<ArgumentNullException>()
            .Fallback(fallbackAction, onFallback);

        Should.NotThrow(() => fallbackPolicy.ExecuteAndCapture(() => throw new ArgumentNullException()));

        fallbackException.ShouldNotBeNull()
            .ShouldBeOfType<ArgumentNullException>();
    }

    [Fact]
    public void Should_call_fallbackAction_with_the_matched_inner_exception_unwrapped()
    {
        Exception? fallbackException = null;

        Action<Exception, Context, CancellationToken> fallbackAction = (ex, _, _) => fallbackException = ex;

        Action<Exception, Context> onFallback = (_, _) => { };

        FallbackPolicy fallbackPolicy = Policy
            .HandleInner<ArgumentNullException>()
            .Fallback(fallbackAction, onFallback);

        Exception instanceToCapture = new ArgumentNullException("myParam");
        Exception instanceToThrow = new Exception(string.Empty, instanceToCapture);
        Should.NotThrow(() => fallbackPolicy.RaiseException(instanceToThrow));

        fallbackException.ShouldBe(instanceToCapture);
    }

    [Fact]
    public void Should_call_fallbackAction_with_the_matched_inner_of_aggregate_exception_unwrapped()
    {
        Exception? fallbackException = null;

        Action<Exception, Context, CancellationToken> fallbackAction = (ex, _, _) => fallbackException = ex;

        Action<Exception, Context> onFallback = (_, _) => { };

        FallbackPolicy fallbackPolicy = Policy
            .HandleInner<ArgumentNullException>()
            .Fallback(fallbackAction, onFallback);

        Exception instanceToCapture = new ArgumentNullException("myParam");
        Exception instanceToThrow = new AggregateException(instanceToCapture);
        Should.NotThrow(() => fallbackPolicy.RaiseException(instanceToThrow));

        fallbackException.ShouldBe(instanceToCapture);
    }

    [Fact]
    public void Should_not_call_fallbackAction_with_the_exception_if_exception_unhandled()
    {
        Exception? fallbackException = null;

        Action<Exception, Context, CancellationToken> fallbackAction = (ex, _, _) => fallbackException = ex;

        Action<Exception, Context> onFallback = (_, _) => { };

        FallbackPolicy fallbackPolicy = Policy
            .Handle<DivideByZeroException>()
            .Fallback(fallbackAction, onFallback);

        Should.Throw<ArgumentNullException>(() => fallbackPolicy.Execute(() => throw new ArgumentNullException()));

        fallbackException.ShouldBeNull();
    }

    #endregion

    #region Cancellation tests

    [Fact]
    public void Should_execute_action_when_non_faulting_and_cancellationToken_not_cancelled()
    {
        bool fallbackActionExecuted = false;
        Action fallbackAction = () => fallbackActionExecuted = true;

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

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            Should.NotThrow(() => policy.RaiseExceptionAndOrCancellation<DivideByZeroException>(scenario, cancellationTokenSource, onExecute));
        }

        attemptsInvoked.ShouldBe(1);

        fallbackActionExecuted.ShouldBeFalse();
    }

    [Fact]
    public void Should_execute_fallback_when_faulting_and_cancellationToken_not_cancelled()
    {
        bool fallbackActionExecuted = false;
        Action fallbackAction = () => fallbackActionExecuted = true;

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

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            Should.NotThrow(() => policy.RaiseExceptionAndOrCancellation<DivideByZeroException>(scenario, cancellationTokenSource, onExecute));
        }

        attemptsInvoked.ShouldBe(1);

        fallbackActionExecuted.ShouldBeTrue();
    }

    [Fact]
    public void Should_not_execute_action_when_cancellationToken_cancelled_before_execute()
    {
        bool fallbackActionExecuted = false;
        Action fallbackAction = () => fallbackActionExecuted = true;

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

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            cancellationTokenSource.Cancel();

            Should.Throw<OperationCanceledException>(() => policy.RaiseExceptionAndOrCancellation<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .CancellationToken.ShouldBe(cancellationToken);
        }

        attemptsInvoked.ShouldBe(0);

        fallbackActionExecuted.ShouldBeFalse();

    }

    [Fact]
    public void Should_report_cancellation_and_not_execute_fallback_during_otherwise_non_faulting_action_execution_when_user_delegate_observes_cancellationToken_and_fallback_does_not_handle_cancellations()
    {
        bool fallbackActionExecuted = false;
        Action fallbackAction = () => fallbackActionExecuted = true;

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

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;
            Should.Throw<OperationCanceledException>(() => policy.RaiseExceptionAndOrCancellation<DivideByZeroException>(scenario, cancellationTokenSource, onExecute))
                .CancellationToken.ShouldBe(cancellationToken);
        }

        attemptsInvoked.ShouldBe(1);

        fallbackActionExecuted.ShouldBeFalse();
    }

    [Fact]
    public void Should_handle_cancellation_and_execute_fallback_during_otherwise_non_faulting_action_execution_when_user_delegate_observes_cancellationToken_and_fallback_handles_cancellations()
    {
        bool fallbackActionExecuted = false;
        Action fallbackAction = () => fallbackActionExecuted = true;

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

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            Should.NotThrow(() => policy.RaiseExceptionAndOrCancellation<DivideByZeroException>(scenario, cancellationTokenSource, onExecute));
        }

        attemptsInvoked.ShouldBe(1);

        fallbackActionExecuted.ShouldBeTrue();
    }

    [Fact]
    public void Should_not_report_cancellation_and_not_execute_fallback_if_non_faulting_action_execution_completes_and_user_delegate_does_not_observe_the_set_cancellationToken()
    {
        bool fallbackActionExecuted = false;
        Action fallbackAction = () => fallbackActionExecuted = true;

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

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            Should.NotThrow(() => policy.RaiseExceptionAndOrCancellation<DivideByZeroException>(scenario, cancellationTokenSource, onExecute));
        }

        attemptsInvoked.ShouldBe(1);
        fallbackActionExecuted.ShouldBeFalse();
    }

    [Fact]
    public void Should_report_unhandled_fault_and_not_execute_fallback_if_action_execution_raises_unhandled_fault_and_user_delegate_does_not_observe_the_set_cancellationToken()
    {
        bool fallbackActionExecuted = false;
        Action fallbackAction = () => fallbackActionExecuted = true;

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

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            Should.Throw<NullReferenceException>(() => policy.RaiseExceptionAndOrCancellation<NullReferenceException>(scenario, cancellationTokenSource, onExecute));
        }

        attemptsInvoked.ShouldBe(1);

        fallbackActionExecuted.ShouldBeFalse();
    }

    [Fact]
    public void Should_handle_handled_fault_and_execute_fallback_following_faulting_action_execution_when_user_delegate_does_not_observe_cancellationToken()
    {
        bool fallbackActionExecuted = false;
        Action fallbackAction = () => fallbackActionExecuted = true;

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

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            Should.NotThrow(() => policy.RaiseExceptionAndOrCancellation<DivideByZeroException>(scenario, cancellationTokenSource, onExecute));
        }

        attemptsInvoked.ShouldBe(1);

        fallbackActionExecuted.ShouldBeTrue();
    }

    #endregion
}
