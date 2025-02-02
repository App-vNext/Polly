namespace Polly.Specs;

public class ContextSpecs
{
    [Fact]
    public void Should_assign_OperationKey_from_constructor()
    {
        Context context = new Context("SomeKey");

        context.OperationKey.ShouldBe("SomeKey");

        context.Keys.Count.ShouldBe(0);
    }

    [Fact]
    public void Should_assign_OperationKey_and_context_data_from_constructor()
    {
        Context context = new Context("SomeKey", CreateDictionary("key1", "value1", "key2", "value2"));

        context.OperationKey.ShouldBe("SomeKey");
        context["key1"].ShouldBe("value1");
        context["key2"].ShouldBe("value2");
    }

    [Fact]
    public void NoArgsCtor_should_assign_no_OperationKey()
    {
        Context context = [];

        context.OperationKey.ShouldBeNull();
    }

    [Fact]
    public void Should_assign_CorrelationId_when_accessed()
    {
        Context context = new Context("SomeKey");

        context.OperationKey.ShouldBe("SomeKey");
    }

    [Fact]
    public void Should_return_consistent_CorrelationId()
    {
        Context context = new Context("SomeKey");

        Guid retrieved1 = context.CorrelationId;
        Guid retrieved2 = context.CorrelationId;

        retrieved1.ShouldBe(retrieved2);
    }
}
