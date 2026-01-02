namespace Polly.Tests;

// The tests can only be strong named if they only depend on assemblies that are strong named,
// therefore if the tests are strong named then the libraries we ship are strong named.

public static class StrongNameTests
{
    [Fact]
    public static void Tests_Are_Strong_Named()
    {
        // Arrange
        var assembly = typeof(StrongNameTests).Assembly;
        var name = assembly.GetName();

        // Act
        var actual = name.GetPublicKey();

        // Assert
        Assert.NotNull(actual);
        Assert.NotEmpty(actual);

        Assert.Equal(
            "0024000004800000940000000602000000240000525341310004000001000100150819E3494F97263A3ABDD18E5E0C47B04E6C0EDE44A6C51D50B545D403CEEB7CBB32D18DBBBCDD1D88A87D7B73206B126BE134B0609C36AA3CB31DD2E47E393293102809B8D77F192F3188618A42E651C14EBF05F8F5B76AA91B431642B23497ED82B65D63791CDAA31D4282A2D6CBABC3FE0745B6B6690C417CABF6A1349C",
            ToHexString(actual));

        // Act
        actual = name.GetPublicKeyToken();

        // Assert
        Assert.NotNull(actual);
        Assert.NotEmpty(actual);

        Assert.Equal("C8A3FFC3F8F825CC", ToHexString(actual));
    }

    private static string ToHexString(byte[] bytes)
    {
#if NET
        return Convert.ToHexString(bytes);
#else
        var builder = new System.Text.StringBuilder(bytes.Length * 2);

        foreach (var b in bytes)
        {
            builder.Append(b.ToString("X2", System.Globalization.CultureInfo.InvariantCulture));
        }

        return builder.ToString();
#endif
    }
}
