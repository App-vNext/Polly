using System.Globalization;

namespace Polly.Extensions.Telemetry;

internal static class ResilienceContextExtensions
{
    public static string GetResultType(this ResilienceContext context) => context.IsVoid ? "void" : context.ResultType.Name.ToString(CultureInfo.InvariantCulture);

    public static string GetExecutionHealth(this ResilienceContext context) => context.IsExecutionHealthy() ? "Healthy" : "Unhealthy";

    public static bool IsExecutionHealthy(this ResilienceContext context) => context.ResilienceEvents.Count == 0;
}
