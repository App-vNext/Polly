namespace Polly.Specs.Helpers;

public static class ObjectExtensions
{
#pragma warning disable S4225
    public static IDictionary<string, object> AsDictionary(this object source) =>
#pragma warning restore S4225
        source.GetType().GetRuntimeProperties().ToDictionary(
            propInfo => propInfo.Name,
            propInfo => propInfo.GetValue(source, null)!);
}
