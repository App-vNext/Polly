using System.ComponentModel.DataAnnotations;

namespace Polly.Utils;

internal static class Constants
{
    public const string ValidationTrimmingMessage = "The default options validation requires reflection. Use custom 'ResilienceStrategyBuilder.Validator' callback for validation.";

    public const string BuilderTrimmingMessage = $"The default validator uses reflection for validator. " +
        $"Assign custom '{nameof(Validator)}' validation delegate if your library needs to be AOT friendly.";
}
