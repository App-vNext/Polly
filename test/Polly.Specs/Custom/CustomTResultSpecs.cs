namespace Polly.Specs.Custom;

public class CustomTResultSpecs
{
    [Fact]
    public void Should_be_able_to_construct_active_policy()
    {
        Action construct = () =>
        {
            PreExecutePolicy<ResultPrimitive> policy = PreExecutePolicy<ResultPrimitive>.Create(() => Console.WriteLine("Do something"));
        };

        Should.NotThrow(construct);
    }

    [Fact]
    public void Active_policy_should_execute()
    {
        bool preExecuted = false;
        PreExecutePolicy<ResultPrimitive> policy = PreExecutePolicy<ResultPrimitive>.Create(() => preExecuted = true);

        bool executed = false;

        Should.NotThrow(() => policy.Execute(() =>
        {
            executed = true;
            return ResultPrimitive.Undefined;
        }));

        executed.ShouldBeTrue();
        preExecuted.ShouldBeTrue();
    }

    [Fact]
    public void Should_be_able_to_construct_reactive_policy()
    {
        Action construct = () =>
        {
            AddBehaviourIfHandlePolicy<ResultPrimitive> policy = Policy.HandleResult(ResultPrimitive.Fault).WithBehaviour(outcome => Console.WriteLine("Handling " + outcome.Result));
        };

        Should.NotThrow(construct);
    }

    [Fact]
    public void Reactive_policy_should_handle_result()
    {
        ResultPrimitive handled = ResultPrimitive.Undefined;
        AddBehaviourIfHandlePolicy<ResultPrimitive> policy = Policy.HandleResult(ResultPrimitive.Fault).WithBehaviour(outcome => handled = outcome.Result);

        ResultPrimitive toReturn = ResultPrimitive.Fault;
        bool executed = false;

        policy.Execute(() =>
            {
                executed = true;
                return toReturn;
            })
            .ShouldBe(toReturn);

        executed.ShouldBeTrue();
        handled.ShouldBe(toReturn);
    }

    [Fact]
    public void Reactive_policy_should_be_able_to_ignore_unhandled_result()
    {
        ResultPrimitive? handled = null;
        AddBehaviourIfHandlePolicy<ResultPrimitive> policy = Policy.HandleResult(ResultPrimitive.Fault).WithBehaviour(outcome => handled = outcome.Result);

        ResultPrimitive toReturn = ResultPrimitive.FaultYetAgain;
        bool executed = false;

        policy.Execute(() =>
            {
                executed = true;
                return toReturn;
            })
            .ShouldBe(toReturn);

        executed.ShouldBeTrue();
        handled.ShouldBe(null);
    }
}
