namespace Snippets.Docs;

internal static partial class Migration
{
    private const string Key1 = nameof(Key1);
    private const string Key2 = nameof(Key2);
    public static void Context_V7()
    {
        #region migration-context-v7

        // Create context
        Context context = new Context();

        // Create context with operation key
        context = new Context("my-operation-key");

        // Attach custom properties
        context[Key1] = "value-1";
        context[Key2] = 100;

        // Retrieve custom properties
        string value1 = (string)context[Key1];
        int value2 = (int)context[Key2];

        // Bulk attach
        context = new Context("my-operation-key", new Dictionary<string, object>
        {
            { Key1 , "value-1" },
            { Key2 , 100 }
        });

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
        ResiliencePropertyKey<string> propertyKey1 = new(Key1);
        context.Properties.Set(propertyKey1, "value-1");

        ResiliencePropertyKey<int> propertyKey2 = new(Key2);
        context.Properties.Set(propertyKey2, 100);

        // Bulk attach
        context.Properties.SetProperties(new Dictionary<string, object?>
        {
            { Key1 , "value-1" },
            { Key2 , 100 }
        }, out var oldProperties);

        // Retrieve custom properties
        string value1 = context.Properties.GetValue(propertyKey1, "default");
        int value2 = context.Properties.GetValue(propertyKey2, 0);

        // Return the context to the pool
        ResilienceContextPool.Shared.Return(context);

        #endregion
    }
}
