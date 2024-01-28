using Polly.Utils;

namespace Polly.Core.Tests.Utils;

public class ObjectPoolTests
{
    [Fact]
    public void GetAnd_ReturnObject_SameInstance()
    {
        // Arrange
        var pool = new ObjectPool<object>(() => new object(), _ => { });

        var obj1 = pool.Get();
        pool.Return(obj1);

        // Act
        var obj2 = pool.Get();

        // Assert
        Assert.Same(obj1, obj2);
    }

    [Fact]
    public void MaxCapacity_Ok()
    {
        ObjectPool<object>.MaxCapacity.Should().Be((Environment.ProcessorCount * 2) - 1);
    }

    [Fact]
    public void MaxCapacity_Respected()
    {
        // Arrange
        var pool = new ObjectPool<object>(() => new object(), _ => true);
        var items1 = GetStoreReturn(pool);

        // Act
        var items2 = GetStoreReturn(pool);

        // Assert
        items1.Should().BeEquivalentTo(items2);
    }

    [Fact]
    public void MaxCapacityOverflow_Respected()
    {
        // Arrange
        var count = ObjectPool<object>.MaxCapacity + 10;
        var pool = new ObjectPool<object>(() => new object(), _ => true);
        var items1 = GetStoreReturn(pool, count);

        // Act
        var items2 = GetStoreReturn(pool, count);

        // Assert
        items1[items1.Count - 1].Should().NotBeSameAs(items2[items2.Count - 1]);
    }

    [Fact]
    public void CreatedByPolicy()
    {
        // Arrange
        var policy = new ListPolicy();
        var pool = new ObjectPool<List<int>>(ListPolicy.Create, ListPolicy.Return);

        // Act
        var list = pool.Get();

        // Assert
        Assert.Equal(17, list.Capacity);
    }

    [Fact]
    public void Return_RejectedByPolicy()
    {
        // Arrange
        var policy = new ListPolicy();
        var pool = new ObjectPool<List<int>>(ListPolicy.Create, ListPolicy.Return);
        var list1 = pool.Get();
        list1.Capacity = 20;

        // Act
        pool.Return(list1);
        var list2 = pool.Get();

        // Assert
        Assert.NotSame(list1, list2);
    }

    private static List<object> GetStoreReturn(ObjectPool<object> pool, int? count = null)
    {
        var items = new List<object>();
        for (int i = 0; i < (count ?? ObjectPool<object>.MaxCapacity); i++)
        {
            items.Add(pool.Get());
        }

        foreach (var item in items)
        {
            pool.Return(item);
        }

        return items;
    }

    private class ListPolicy
    {
        public static List<int> Create() => new(17);

        public static bool Return(List<int> obj) => obj.Capacity == 17;
    }
}
