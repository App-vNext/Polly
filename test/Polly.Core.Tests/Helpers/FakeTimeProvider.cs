// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#pragma warning disable

// Replace with Microsoft.Extensions.TimeProvider.Testing when TimeProvider is used (see https://github.com/App-vNext/Polly/pull/1144)
// Based on https://github.com/dotnet/extensions/blob/14917b87e8fc81f10d44ceea52d9b24e50e26550/src/Libraries/Microsoft.Extensions.TimeProvider.Testing/FakeTimeProvider.cs

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading;
using Microsoft.Extensions.Time.Testing;

namespace Microsoft.Extensions.Time.Testing;

/// <summary>
/// A synthetic time provider used to enable deterministic behavior in tests.
/// </summary>
internal class FakeTimeProvider : TimeProvider
{
    internal readonly HashSet<Waiter> Waiters = new();
    private DateTimeOffset _now = new(2000, 1, 1, 0, 0, 0, 0, TimeSpan.Zero);
    private TimeZoneInfo _localTimeZone = TimeZoneInfo.Utc;
    private int _wakeWaitersGate;
    private TimeSpan _autoAdvanceAmount;

    /// <summary>
    /// Initializes a new instance of the <see cref="FakeTimeProvider"/> class.
    /// </summary>
    /// <remarks>
    /// This creates a provider whose time is initially set to midnight January 1st 2000.
    /// The provider is set to not automatically advance time each time it is read.
    /// </remarks>
    public FakeTimeProvider()
    {
        Start = _now;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FakeTimeProvider"/> class.
    /// </summary>
    /// <param name="startDateTime">The initial time and date reported by the provider.</param>
    /// <remarks>
    /// The provider is set to not automatically advance time each time it is read.
    /// </remarks>
    public FakeTimeProvider(DateTimeOffset startDateTime)
    {
        _now = startDateTime;
        Start = _now;
    }

    /// <summary>
    /// Gets the starting date and time for this provider.
    /// </summary>
    public DateTimeOffset Start { get; }

    /// <summary>
    /// Gets or sets the amount of time by which time advances whenever the clock is read.
    /// </summary>
    /// <remarks>
    /// This defaults to <see cref="TimeSpan.Zero"/>.
    /// </remarks>
    public TimeSpan AutoAdvanceAmount
    {
        get => _autoAdvanceAmount;
        set
        {
            _autoAdvanceAmount = value;
        }
    }

    /// <inheritdoc />
    public override DateTimeOffset GetUtcNow()
    {
        DateTimeOffset result;

        lock (Waiters)
        {
            result = _now;
            _now += _autoAdvanceAmount;
        }

        WakeWaiters();
        return result;
    }

    /// <summary>
    /// Sets the date and time in the UTC time zone.
    /// </summary>
    /// <param name="value">The date and time in the UTC time zone.</param>
    public void SetUtcNow(DateTimeOffset value)
    {
        lock (Waiters)
        {
            if (value < _now)
            {
                throw new ArgumentOutOfRangeException(nameof(value), $"Cannot go back in time. Current time is {_now}.");
            }

            _now = value;
        }

        WakeWaiters();
    }

    /// <summary>
    /// Advances time by a specific amount.
    /// </summary>
    /// <param name="delta">The amount of time to advance the clock by.</param>
    /// <remarks>
    /// Advancing time affects the timers created from this provider, and all other operations that are directly or
    /// indirectly using this provider as a time source. Whereas when using <see cref="TimeProvider.System"/>, time
    /// marches forward automatically in hardware, for the fake time provider the application is responsible for
    /// doing this explicitly by calling this method.
    /// </remarks>
    public void Advance(TimeSpan delta)
    {
        lock (Waiters)
        {
            _now += delta;
        }

        WakeWaiters();
    }

    /// <inheritdoc />
    public override long GetTimestamp()
    {
        // Notionally we're multiplying by frequency and dividing by ticks per second,
        // which are the same value for us. Don't actually do the math as the full
        // precision of ticks (a long) cannot be represented in a double during division.
        // For test stability we want a reproducible result.
        //
        // The same issue could occur converting back, in GetElapsedTime(). Unfortunately
        // that isn't virtual so we can't do the same trick. However, if tests advance
        // the clock in multiples of 1ms or so this loss of precision will not be visible.
        Debug.Assert(TimestampFrequency == TimeSpan.TicksPerSecond, "Assuming frequency equals ticks per second");
        return _now.Ticks;
    }

    /// <inheritdoc />
    public override TimeZoneInfo LocalTimeZone => _localTimeZone;

    /// <summary>
    /// Sets the local time zone.
    /// </summary>
    /// <param name="localTimeZone">The local time zone.</param>
    public void SetLocalTimeZone(TimeZoneInfo localTimeZone) => _localTimeZone = localTimeZone;

    /// <summary>
    /// Gets the amount by which the value from <see cref="GetTimestamp"/> increments per second.
    /// </summary>
    /// <remarks>
    /// This is fixed to the value of <see cref="TimeSpan.TicksPerSecond"/>.
    /// </remarks>
    public override long TimestampFrequency => TimeSpan.TicksPerSecond;

    /// <summary>
    /// Returns a string representation this provider's idea of current time.
    /// </summary>
    /// <returns>A string representing the provider's current time.</returns>
    public override string ToString() => GetUtcNow().ToString("yyyy-MM-ddTHH:mm:ss.fff", CultureInfo.InvariantCulture);

    internal void RemoveWaiter(Waiter waiter)
    {
        lock (Waiters)
        {
            _ = Waiters.Remove(waiter);
        }
    }

    internal void AddWaiter(Waiter waiter, long dueTime)
    {
        lock (Waiters)
        {
            waiter.ScheduledOn = _now.Ticks;
            waiter.WakeupTime = _now.Ticks + dueTime;
            _ = Waiters.Add(waiter);
        }

        WakeWaiters();
    }

    private void WakeWaiters()
    {
        if (Interlocked.CompareExchange(ref _wakeWaitersGate, 1, 0) == 1)
        {
            // some other thread is already in here, so let it take care of things
            return;
        }

        while (true)
        {
            Waiter? candidate = null;
            lock (Waiters)
            {
                // find an expired waiter
                foreach (var waiter in Waiters)
                {
                    if (waiter.WakeupTime > _now.Ticks)
                    {
                        // not expired yet
                    }
                    else if (candidate is null)
                    {
                        // our first candidate
                        candidate = waiter;
                    }
                    else if (waiter.WakeupTime < candidate.WakeupTime)
                    {
                        // found a waiter with an earlier wake time, it's our new candidate
                        candidate = waiter;
                    }
                    else if (waiter.WakeupTime > candidate.WakeupTime)
                    {
                        // the waiter has a later wake time, so keep the current candidate
                    }
                    else if (waiter.ScheduledOn < candidate.ScheduledOn)
                    {
                        // the new waiter has the same wake time aa the candidate, pick whichever was scheduled earliest to maintain order
                        candidate = waiter;
                    }
                }
            }

            if (candidate == null)
            {
                // didn't find a candidate to wake, we're done
                _wakeWaitersGate = 0;
                return;
            }

            // invoke the callback
            candidate.InvokeCallback();

            // see if we need to reschedule the waiter
            if (candidate.Period > 0)
            {
                // update the waiter's state
                candidate.ScheduledOn = _now.Ticks;
                candidate.WakeupTime += candidate.Period;
            }
            else
            {
                // this waiter is never running again, so remove from the set.
                RemoveWaiter(candidate);
            }
        }
    }
}

// We keep all timer state here in order to prevent Timer instances from being self-referential,
// which would block them being collected when someone forgets to call Dispose on the timer. With
// this arrangement, the Timer object will always be collectible, which will end up calling Dispose
// on this object due to the timer's finalizer.
internal sealed class Waiter
{
    private readonly TimerCallback _callback;
    private readonly object? _state;

    public long ScheduledOn { get; set; } = -1;
    public long WakeupTime { get; set; } = -1;
    public long Period { get; }

    public Waiter(TimerCallback callback, object? state, long period)
    {
        _callback = callback;
        _state = state;
        Period = period;
    }

    public void InvokeCallback()
    {
        _callback(_state);
    }
}
