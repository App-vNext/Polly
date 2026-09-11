namespace Polly.Specs;

public static class ContextDictionarySpecs
{
    [Fact]
    public static void Generic_dictionary_members_should_delegate_to_the_wrapped_dictionary()
    {
        var context = new Context("operation-key");
        IDictionary<string, object> dictionary = context;
        ICollection<KeyValuePair<string, object>> collection = context;
        IReadOnlyDictionary<string, object> readOnlyDictionary = context;

        collection.IsReadOnly.ShouldBeFalse();

        dictionary.Add("one", 1);
        context.Add("two", 2);
        collection.Add(new KeyValuePair<string, object>("three", 3));

        dictionary.ContainsKey("one").ShouldBeTrue();
        dictionary.TryGetValue("two", out object? value).ShouldBeTrue();
        value.ShouldBe(2);

        context.Keys.ShouldContain("one");
        context.Values.ShouldContain(3);
        readOnlyDictionary.Keys.ShouldContain("two");
        readOnlyDictionary.Values.ShouldContain(1);

        var copied = new KeyValuePair<string, object>[collection.Count];
        collection.CopyTo(copied, 0);
        copied.Any(kvp => kvp.Key == "three" && Equals(kvp.Value, 3)).ShouldBeTrue();

        using IEnumerator<KeyValuePair<string, object>> genericEnumerator = context.GetEnumerator();
        genericEnumerator.MoveNext().ShouldBeTrue();

        IEnumerator nongenericEnumerator = ((IEnumerable)context).GetEnumerator();
        nongenericEnumerator.MoveNext().ShouldBeTrue();

        collection.Contains(new KeyValuePair<string, object>("three", 3)).ShouldBeTrue();
        collection.Remove(new KeyValuePair<string, object>("three", 3)).ShouldBeTrue();
        dictionary.Remove("two").ShouldBeTrue();

        context.Clear();
        context.Count.ShouldBe(0);
    }

    [Fact]
    public static void Nongeneric_dictionary_members_should_delegate_to_the_wrapped_dictionary()
    {
        IDictionary dictionary = new Context("operation-key");
        ICollection collection = dictionary;

        dictionary.IsFixedSize.ShouldBeFalse();
        dictionary.IsReadOnly.ShouldBeFalse();
        collection.IsSynchronized.ShouldBeFalse();
        collection.SyncRoot.ShouldNotBeNull();

        dictionary.Add("one", 1);
        dictionary["two"] = 2;

        dictionary.Contains("one").ShouldBeTrue();
        dictionary.Keys.Cast<object>().ShouldContain("two");
        dictionary.Values.Cast<object>().ShouldContain(1);

        var copied = new DictionaryEntry[dictionary.Count];
        dictionary.CopyTo(copied, 0);
        copied.Any(entry => Equals(entry.Key, "one") && Equals(entry.Value, 1)).ShouldBeTrue();

        IDictionaryEnumerator enumerator = dictionary.GetEnumerator();
        enumerator.MoveNext().ShouldBeTrue();

        dictionary.Remove("one");
        dictionary.Contains("one").ShouldBeFalse();
    }
}
