using Polly;
using System.ComponentModel.DataAnnotations;

namespace Extensibility.Proactive;

#region ext-proactive-options

public class TimingStrategyOptions : ResilienceStrategyOptions
{
    public TimingStrategyOptions()
    {
        // It's recommended to set the default name for the options so
        // the consumer can get additional information in the telemetry.
        Name = "Timing";
    }

    // You can use the validation attributes to ensure the options are valid.
    // The validation will be performed automatically when building the pipeline.
    [Range(typeof(TimeSpan), "00:00:00", "1.00:00:00")]
    [Required]
    public TimeSpan? Threshold { get; set; }

    // Expose the delegate that will be invoked when the threshold is exceeded.
    // The recommendation is that the arguments should have the same name as the delegate but with "Arguments" suffix.
    // Notice that the delegate is not required.
    public Func<ThresholdExceededArguments, ValueTask>? ThresholdExceeded { get; set; }
}

#endregion
