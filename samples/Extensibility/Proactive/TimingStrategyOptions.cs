using Polly;
using System.ComponentModel.DataAnnotations;

namespace Extensibility.Proactive;

#region ext-proactive-options

public class TimingStrategyOptions : ResilienceStrategyOptions
{
    public TimingStrategyOptions()
    {
        // Assign a default name to the options for more detailed telemetry insights.
        Name = "Timing";
    }

    // Apply validation attributes to guarantee the options' validity.
    // The pipeline will handle validation automatically during its construction.
    [Range(typeof(TimeSpan), "00:00:00", "1.00:00:00")]
    [Required]
    public TimeSpan? Threshold { get; set; }

    // Provide the delegate to be called when the threshold is surpassed.
    // Ideally, arguments should share the delegate's name, but with an "Arguments" suffix.
    public Func<OnThresholdExceededArguments, ValueTask>? OnThresholdExceeded { get; set; }
}

#endregion
