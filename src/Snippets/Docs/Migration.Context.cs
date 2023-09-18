namespace Snippets.Docs;

internal static partial class Migration
{
    public static void Context_V7()
    {
        #region migration-context-v7

        // Create context
        Context context = new Context();

        // Create context with operation key
        context = new Context("my-operation-key");

        // Attach custom properties
        context["prop-1"] = "value-1";
        context["prop-2"] = 100;

        // Retrieve custom properties
        string value1 = (string)context["prop-1"];
        int value2 = (int)context["prop-2"];

        #endregion
    }

    public static void Context_V8()
    {
        #region migration-context-v8

        // Create context
        ResilienceContext context = ResilienceContextPool.Shared.Get();

        // Create context with operation key
        context = ResilienceContextPool.Shared.Get("my-operation-key");

        // Attach custom properties
        context.Properties.Set(new ResiliencePropertyKey<string>("prop-1"), "value-1");
        context.Properties.Set(new ResiliencePropertyKey<int>("prop-2"), 100);

        // Retrieve custom properties
        string value1 = context.Properties.GetValue(new ResiliencePropertyKey<string>("prop-1"), "default");
        int value2 = context.Properties.GetValue(new ResiliencePropertyKey<int>("prop-2"), 0);

        // Return the context to the pool
        ResilienceContextPool.Shared.Return(context);

        #endregion
    }
}
