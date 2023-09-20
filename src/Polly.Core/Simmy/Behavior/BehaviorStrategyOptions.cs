﻿using System.ComponentModel.DataAnnotations;

namespace Polly.Simmy.Behavior;

/// <summary>
/// Represents the options for the Behavior chaos strategy.
/// </summary>
internal class BehaviorStrategyOptions : MonkeyStrategyOptions
{
    /// <summary>
    /// Gets or sets the delegate that's raised when the custom behavior is injected.
    /// </summary>
    /// <remarks>
    /// Defaults to <see langword="null"/>.
    /// </remarks>
    public Func<OnBehaviorInjectedArguments, ValueTask>? OnBehaviorInjected { get; set; }

    /// <summary>
    /// Gets or sets the custom behavior that is going to be injected for a given execution.
    /// </summary>
    /// <remarks>
    /// Defaults to <see langword="null"/>.
    /// </remarks>
    [Required]
    public Func<BehaviorActionArguments, ValueTask>? BehaviorAction { get; set; }
}
