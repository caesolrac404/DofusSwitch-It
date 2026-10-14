using DofusSwitchIt.Native;
using FluentAssertions;

namespace DofusSwitchIt.Tests;

/// <summary>SPEC-010, T-28: el predicado solo acepta títulos de cliente Dofus reales.</summary>
public sealed class DetectionPredicateTests
{
    [Theory]
    [InlineData("Dofus 2.73.5 - Release")]
    [InlineData("Dofus 2.73.5.12 - Release")]
    [InlineData("Dofus 2.73.5 - Release ")] // espacio final tolerado
    [InlineData("Dofus 2.73.5-release")]     // case-insensitive, sin espacios
    public void DefaultPredicate_RealClientTitles_Match(string title)
    {
        Win32WindowEnumerator.DefaultPredicate(title, 1234).Should().BeTrue();
    }

    [Theory]
    [InlineData("Release Notes.txt")]       // señuelo: sin versión
    [InlineData("MyApp - Release")]         // señuelo: sin versión
    [InlineData("DofusSwitch-It")]          // la propia app nunca matchea
    [InlineData("Dofus 2.73 - Release")]    // límite conocido: solo 3-4 partes
    [InlineData("")]
    [InlineData("   ")]
    public void DefaultPredicate_NonClientTitles_DoNotMatch(string title)
    {
        Win32WindowEnumerator.DefaultPredicate(title, 1234).Should().BeFalse();
    }
}
