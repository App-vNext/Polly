using System.Collections.Generic;
using System.Globalization;

namespace Polly.Extensions.Telemetry;

internal static class ResilienceContextExtensions
{
    public static string GetResultType(this ResilienceContext context)
    {
        return context.IsVoid ? "void" : context.ResultType.Name.ToString(CultureInfo.InvariantCulture);
    }

    public static string GetExecutionHealth(this ResilienceContext context)
    {
        return context.ResilienceEvents.Count == 0 ? "Healthy" : "Unhealthy";
    }
}
