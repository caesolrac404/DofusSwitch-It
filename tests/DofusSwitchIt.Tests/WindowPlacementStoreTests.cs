using DofusSwitchIt.Core;
using FluentAssertions;

namespace DofusSwitchIt.Tests;

/// <summary>SPEC-007, T-23: persistencia de la posición de la ventana.</summary>
public sealed class WindowPlacementStoreTests : IDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"dsit-test-{Guid.NewGuid():N}.json");

    public void Dispose()
    {
        try { File.Delete(_path); } catch { }
    }

    [Fact]
    public void Save_ThenLoad_RoundtripsPosition()
    {
        var store = new WindowPlacementStore(_path);

        store.Save(new WindowPlacement(100, 200));

        store.Load().Should().Be(new WindowPlacement(100, 200));
    }

    [Fact]
    public void Load_MissingFile_ReturnsNull()
    {
        new WindowPlacementStore(_path).Load().Should().BeNull();
    }

    [Fact]
    public void Load_CorruptFile_ReturnsNull()
    {
        File.WriteAllText(_path, "{no-json");

        new WindowPlacementStore(_path).Load().Should().BeNull();
    }
}
