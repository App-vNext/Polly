namespace Polly.Specs.Helpers;

/// <summary>
/// A helper class supporting tests on how Policy&lt;TResult&gt; policies handle return results which are class types (as opposed to primitive types).
/// </summary>
internal class ResultClass(ResultPrimitive resultCode, string? someString)
{
    public ResultClass(ResultPrimitive resultCode)
        : this(resultCode, null)
    {
    }

    public ResultPrimitive ResultCode { get; set; } = resultCode;

    public string? SomeString { get; set; } = someString;
}
