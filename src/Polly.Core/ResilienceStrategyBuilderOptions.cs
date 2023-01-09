using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Polly;

public class ResilienceStrategyBuilderOptions
{
    public string StrategyName { get; set; } = string.Empty;

    public ILoggerFactory LoggerFactory { get; set; } = NullLoggerFactory.Instance;
}
