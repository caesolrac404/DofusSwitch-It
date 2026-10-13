using DofusSwitchIt.Core;
using FluentAssertions;

namespace DofusSwitchIt.Tests;

/// <summary>SPEC-006, T-20: nombre visible = primer token del título (nombre del personaje).</summary>
public sealed class DisplayNameTests
{
    [Theory]
    [InlineData("Craette - Dofus 2.73", "Craette")]
    [InlineData("Yopuka", "Yopuka")]
    [InlineData("", "")]
    public void GetDisplayName_ReturnsFirstToken_OrFullTitleAsFallback(string title, string expected)
    {
        GameWindow.GetDisplayName(title).Should().Be(expected);
    }

    [Fact]
    public void DisplayName_UsesTitle()
    {
        new GameWindow((nint)1, "Eniripsa - Dofus", 1).DisplayName.Should().Be("Eniripsa");
    }
}
