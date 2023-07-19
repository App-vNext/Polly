namespace Polly.Utils;

internal static class Constants
{
    public const string OptionsValidation = """
    This call validates the options using the data annotations attributes.
    Make sure that the options are included using the '[DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(OptionsType))]' attribute on top of calling method.
    """;
}
