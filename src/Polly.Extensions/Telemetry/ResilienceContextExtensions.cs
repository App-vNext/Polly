namespace Polly.Telemetry;

internal static class ResilienceContextExtensions
{
    public static string GetExecutionHealth(this ResilienceContext context) => context.IsExecutionHealthy() ? "Healthy" : "Unhealthy";

    public static bool IsExecutionHealthy(this ResilienceContext context)
    {
        for (int i = 0; i < context.ResilienceEvents.Count; i++)
        {
            if (context.ResilienceEvents[i].Severity > ResilienceEventSeverity.Information)
            {
                return false;
            }
        }

        return true;
    }
}
