// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

#pragma warning disable S3872 // Parameter names should not duplicate the names of their methods

namespace System
{
    // Temporary, will be removed
    // Copied from https://github.com/dotnet/runtime/blob/main/src/libraries/Common/src/System/TimeProvider.cs and trimmed some fat which is not relevant for internal stuff

    [ExcludeFromCodeCoverage]
    internal abstract class TimeProvider
    {
        public static TimeProvider System { get; } = new SystemTimeProvider();

        protected TimeProvider()
        {
        }

        public virtual DateTimeOffset GetUtcNow() => DateTimeOffset.UtcNow;

        private static readonly long MinDateTicks = DateTime.MinValue.Ticks;
        private static readonly long MaxDateTicks = DateTime.MaxValue.Ticks;

        public DateTimeOffset GetLocalNow()
        {
            DateTimeOffset utcDateTime = GetUtcNow();
            TimeZoneInfo zoneInfo = LocalTimeZone;
            if (zoneInfo is null)
            {
                throw new InvalidOperationException();
            }

            TimeSpan offset = zoneInfo.GetUtcOffset(utcDateTime);

            long localTicks = utcDateTime.Ticks + offset.Ticks;
            if ((ulong)localTicks > (ulong)MaxDateTicks)
            {
                localTicks = localTicks < MinDateTicks ? MinDateTicks : MaxDateTicks;
            }

            return new DateTimeOffset(localTicks, offset);
        }

        public virtual TimeZoneInfo LocalTimeZone => TimeZoneInfo.Local;

        public virtual long TimestampFrequency => Stopwatch.Frequency;

        public virtual long GetTimestamp() => Stopwatch.GetTimestamp();

        // This one is not on TimeProvider, temporarly we need to use it
        public virtual Task Delay(TimeSpan delay, CancellationToken cancellationToken = default) => Task.Delay(delay, cancellationToken);

        // This one is not on TimeProvider, temporarly we need to use it
        public virtual void CancelAfter(CancellationTokenSource source, TimeSpan delay) => source.CancelAfter(delay);

        public TimeSpan GetElapsedTime(long startingTimestamp, long endingTimestamp)
        {
            long timestampFrequency = TimestampFrequency;
            if (timestampFrequency <= 0)
            {
                throw new InvalidOperationException();
            }

            return new TimeSpan((long)((endingTimestamp - startingTimestamp) * ((double)TimeSpan.TicksPerSecond / timestampFrequency)));
        }

        public TimeSpan GetElapsedTime(long startingTimestamp) => GetElapsedTime(startingTimestamp, GetTimestamp());

        private sealed class SystemTimeProvider : TimeProvider
        {
            internal SystemTimeProvider()
            {
            }
        }
    }
}
