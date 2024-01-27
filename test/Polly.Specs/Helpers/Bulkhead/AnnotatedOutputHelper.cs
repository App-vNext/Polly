namespace Polly.Specs.Helpers.Bulkhead;

public class AnnotatedOutputHelper : ITestOutputHelper
{
    private class Item
    {
        private static int MonotonicSequence;

        public Item(string format, object[] args)
        {
            TimeStamp = DateTimeOffset.UtcNow;
            Position = Interlocked.Increment(ref MonotonicSequence);

            Format = format;
            Args = args;
        }

        public int Position { get; }
        public DateTimeOffset TimeStamp { get; }
        public string Format { get; }
        public object[] Args { get; }
    }

    private readonly ConcurrentDictionary<Guid, Item> _items = new();

    private readonly object[] _noArgs = [];

    private readonly ITestOutputHelper _innerOutputHelper;

    public AnnotatedOutputHelper(ITestOutputHelper innerOutputHelper) =>
        _innerOutputHelper = innerOutputHelper ?? throw new ArgumentNullException(nameof(innerOutputHelper));

    public void Flush()
    {
        // Some IDEs limit the number of lines of output displayed in a test result. Display the lines in reverse order so that we always see the most recent.
        var toOutput = _items.Select(kvp => kvp.Value).OrderBy(i => i.Position).Reverse();
        foreach (var item in toOutput)
        {
            _innerOutputHelper.WriteLine(item.TimeStamp.ToString("o") + ": " + item.Format, item.Args);
        }

        _items.Clear();
    }

    public void WriteLine(string message) =>
        _items.TryAdd(Guid.NewGuid(), new Item(message ?? string.Empty, _noArgs));

    public void WriteLine(string format, params object[] args) =>
        _items.TryAdd(Guid.NewGuid(), new Item(format ?? string.Empty, args == null || args.Length == 0 ? _noArgs : args));
}
