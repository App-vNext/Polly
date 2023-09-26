namespace Snippets.Docs.Utils;

internal class SomeExceptionType : Exception
{
    public SomeExceptionType(string message)
        : base(message)
    {
    }

    public SomeExceptionType(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public SomeExceptionType()
    {
    }
}
