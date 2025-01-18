using Polly.Simmy.Utils;

namespace Polly.Core.Tests.Simmy.Utils;

public class GeneratorHelperTests
{
    [Fact]
    public void CreateGenerator_NoGenerators_Ok()
    {
        var helper = new GeneratorHelper<int>(_ => 10);

        helper.CreateGenerator()(ResilienceContextPool.Shared.Get()).ShouldBeNull();
    }

    [Fact]
    public void AddOutcome_EnsureWeightRespected()
    {
        int weight = 0;
        int maxWeight = 0;
        var context = ResilienceContextPool.Shared.Get();

        var helper = new GeneratorHelper<int>(max =>
        {
            maxWeight = max;
            return weight;
        });

        helper.AddOutcome(_ => Outcome.FromResult(1), 40);
        helper.AddOutcome(_ => Outcome.FromResult(2), 80);

        var generator = helper.CreateGenerator();

        weight = 0;
        generator(context)!.Value.Result.ShouldBe(1);
        weight = 39;
        generator(context)!.Value.Result.ShouldBe(1);

        weight = 40;
        generator(context)!.Value.Result.ShouldBe(2);

        maxWeight.ShouldBe(120);
    }

    [Fact]
    public void Generator_OutsideRange_ReturnsNull()
    {
        var context = ResilienceContextPool.Shared.Get();
        var helper = new GeneratorHelper<int>(_ => 1000);

        helper.AddOutcome(_ => Outcome.FromResult(1), 40);

        var generator = helper.CreateGenerator();
        generator(context).ShouldBeNull();
    }
}
