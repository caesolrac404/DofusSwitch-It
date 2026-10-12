using DofusSwitchIt.Core;
using FluentAssertions;

namespace DofusSwitchIt.Tests;

/// <summary>SPEC-002, T-6 / CA-5: mapeo de hotkeys por defecto F2 = anterior, F3 = siguiente.</summary>
public sealed class HotKeyDefaultsTests
{
    [Fact]
    public void Previous_IsF2_WithoutModifiers()
    {
        HotKeyDefaults.PreviousKey.Should().Be(0x71); // VK_F2
        HotKeyDefaults.Modifiers.Should().Be(0);
    }

    [Fact]
    public void Next_IsF3_WithoutModifiers()
    {
        HotKeyDefaults.NextKey.Should().Be(0x72); // VK_F3
        HotKeyDefaults.Modifiers.Should().Be(0);
    }

    [Fact]
    public void Ids_AreDistinct()
    {
        HotKeyDefaults.PreviousId.Should().NotBe(HotKeyDefaults.NextId);
    }
}
