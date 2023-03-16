using System.ComponentModel.DataAnnotations;

namespace Polly.Builder;

/// <summary>
/// The builder options used by <see cref="IResilienceStrategyBuilder"/>.
/// </summary>
public class ResilienceStrategyBuilderOptions
{
    /// <summary>
    /// Gets or sets the name of the builder.
    /// </summary>
    [Required(AllowEmptyStrings = true)]
    public string BuilderName { get; set; } = string.Empty;
}
