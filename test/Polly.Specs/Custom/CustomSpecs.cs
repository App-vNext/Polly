namespace Polly.Specs.Custom;

public class CustomSpecs
{
    [Fact]
    public void Should_be_able_to_construct_active_policy()
    {
        Action construct = () =>
        {
            PreExecutePolicy policy = PreExecutePolicy.Create(() => Console.WriteLine("Do something"));
        };

        Should.NotThrow(construct);
    }

    [Fact]
    public void Active_policy_should_execute()
    {
        bool preExecuted = false;
        PreExecutePolicy policy = PreExecutePolicy.Create(() => preExecuted = true);

        bool executed = false;

        Should.NotThrow(() => policy.Execute(() => executed = true));

        executed.ShouldBeTrue();
        preExecuted.ShouldBeTrue();
    }

    [Fact]
    public void Should_be_able_to_construct_reactive_policy()
    {
        Action construct = () =>
        {
            AddBehaviourIfHandlePolicy policy = Policy.Handle<Exception>().WithBehaviour(ex => Console.WriteLine("Handling " + ex.Message));
        };

        Should.NotThrow(construct);
    }

    [Fact]
    public void Reactive_policy_should_handle_exception()
    {
        Exception? handled = null;
        AddBehaviourIfHandlePolicy policy = Policy.Handle<InvalidOperationException>().WithBehaviour(ex => handled = ex);

        Exception toThrow = new InvalidOperationException();
        bool executed = false;

        Should.Throw<Exception>(() => policy.Execute(() =>
        {
            executed = true;
            throw toThrow;
        })).ShouldBe(toThrow);

        executed.ShouldBeTrue();
        handled.ShouldBe(toThrow);
    }

    [Fact]
    public void Reactive_policy_should_be_able_to_ignore_unhandled_exception()
    {
        Exception? handled = null;
        AddBehaviourIfHandlePolicy policy = Policy.Handle<InvalidOperationException>().WithBehaviour(ex => handled = ex);

        Exception toThrow = new NotImplementedException();
        bool executed = false;

        Should.Throw<Exception>(() => policy.Execute(() =>
        {
            executed = true;
            throw toThrow;
        })).ShouldBe(toThrow);

        executed.ShouldBeTrue();
        handled.ShouldBeNull();
    }
}
