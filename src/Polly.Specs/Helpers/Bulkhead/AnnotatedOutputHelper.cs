using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using Xunit.Abstractions;

namespace Polly.Specs.Helpers.Bulkhead
{
    public class AnnotatedOutputHelper : ITestOutputHelper
    {
        private class Item
        {
            private static int monotonicSequence;

            public Item(string format, object[] args)
            {
                TimeStamp = DateTimeOffset.UtcNow;
                Position = Interlocked.Increment(ref monotonicSequence);

                Format = format;
                Args = args;
            }

            public int Position { get; }
            public DateTimeOffset TimeStamp { get; }
            public string Format { get; }
            public object[] Args { get; }
        }

        private readonly ConcurrentDictionary<Guid, Item> items = new ConcurrentDictionary<Guid, Item>();

        private readonly object[] noArgs = Array.Empty<object>();

        private readonly ITestOutputHelper innerOutputHelper;

        public AnnotatedOutputHelper(ITestOutputHelper innerOutputHelper)
        {
            this.innerOutputHelper = innerOutputHelper ?? throw new ArgumentNullException(nameof(innerOutputHelper));
        }

        public void Flush()
        {
            // Some IDEs limit the number of lines of output displayed in a test result. Display the lines in reverse order so that we always see the most recent.
            var toOutput = items.Select(kvp => kvp.Value).OrderBy(i => i.Position).Reverse();
            foreach (var item in toOutput)
            {
                innerOutputHelper.WriteLine(item.TimeStamp.ToString("o") + ": " + item.Format, item.Args);
            }

            items.Clear();
        }

        public void WriteLine(string message)
        {
            items.TryAdd(Guid.NewGuid(), new Item(message ?? string.Empty, noArgs));
        }

        public void WriteLine(string format, params object[] args)
        {
            items.TryAdd(Guid.NewGuid(), new Item(format ?? string.Empty, args == null || args.Length == 0 ? noArgs : args));
        }
    }
}
