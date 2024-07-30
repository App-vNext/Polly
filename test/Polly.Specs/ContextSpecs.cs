namespace Polly.Specs;

public class ContextSpecs
{
    [Fact]
    public void Should_assign_OperationKey_from_constructor()
    {
        Context context = new Context("SomeKey");

        context.OperationKey.Should().Be("SomeKey");

        context.Keys.Count.Should().Be(0);
    }

    [Fact]
    public void Should_assign_OperationKey_and_context_data_from_constructor()
    {
        Context context = new Context("SomeKey", CreateDictionary("key1", "value1", "key2", "value2"));

        context.OperationKey.Should().Be("SomeKey");
        context["key1"].Should().Be("value1");
        context["key2"].Should().Be("value2");
    }

    [Fact]
    public void NoArgsCtor_should_assign_no_OperationKey()
    {
        Context context = [];

        context.OperationKey.Should().BeNull();
    }

    [Fact]
    public void Should_assign_CorrelationId_when_accessed()
    {
        Context context = new Context("SomeKey");

        context.CorrelationId.Should().NotBeEmpty();
    }

    [Fact]
    public void Should_return_consistent_CorrelationId()
    {
        Context context = new Context("SomeKey");

        Guid retrieved1 = context.CorrelationId;
        Guid retrieved2 = context.CorrelationId;

        retrieved1.Should().Be(retrieved2);
    }
}
