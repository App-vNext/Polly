using System.ComponentModel.DataAnnotations;

namespace Polly.Core.Tests.Utils;

public class ValidationContextExtensionsTests
{
    [Fact]
    public void GetMemberName_Ok()
    {
        ValidationContext? context = null;
        context.GetMemberName().Should().BeNull();

        context = new ValidationContext(new object());
        context.GetMemberName().Should().BeNull();

        context = new ValidationContext(new object()) { MemberName = "X" };
        context.GetMemberName().Should().NotBeNull();
        context.GetMemberName()![0].Should().Be("X");
    }

    [Fact]
    public void GetDisplayName_Ok()
    {
        ValidationContext? context = null;
        context.GetDisplayName().Should().Be("");

        context = new ValidationContext(new object());
        context.GetDisplayName().Should().Be("Object");

        context = new ValidationContext(new object()) { DisplayName = "X" };
        context.GetDisplayName().Should().Be("X");
    }
}
