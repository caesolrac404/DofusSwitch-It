using DofusSwitchIt.Core;
using FluentAssertions;

namespace DofusSwitchIt.Tests;

/// <summary>
/// Tests del Core Loop (SPEC-001, plan T-1/T-2/T-3/T-5).
/// Sin UI ni P/Invoke: enumerador y foreground son fakes.
/// </summary>
public sealed class WindowCycleServiceTests
{
    private sealed class FakeEnumerator : IWindowEnumerator
    {
        public List<GameWindow> Windows { get; set; } = new();
        public IReadOnlyList<GameWindow> EnumerateDofusWindows() => Windows.AsReadOnly();
    }

    private sealed class FakeForeground : IForegroundService
    {
        public List<GameWindow> BroughtToFront { get; } = new();
        public void BringToFront(GameWindow window) => BroughtToFront.Add(window);
    }

    private static GameWindow W(int id, string title) => new((nint)id, title, (uint)id);

    [Fact] // T-1 / CA-1: el servicio solo conoce lo que el enumerador (filtrado Dofus) devuelve.
    public void Refresh_ExposesOnlyEnumeratedDofusWindows()
    {
        var enumerator = new FakeEnumerator
        {
            Windows = new() { W(1, "Dofus - Cra"), W(2, "Dofus - Eni"), W(3, "Dofus - Osa") }
        };
        var sut = new WindowCycleService(enumerator, new FakeForeground());

        sut.Windows.Should().HaveCount(3);
        sut.Windows.Select(w => w.Title).Should().NotContain(t => t.Contains("Discord") || t.Contains("Chrome"));
    }

    [Fact] // T-2 / CA-2: siguiente circular C -> A.
    public void MoveNext_WrapsFromLastToFirst()
    {
        var enumerator = new FakeEnumerator
        {
            Windows = new() { W(1, "A"), W(2, "B"), W(3, "C") }
        };
        var foreground = new FakeForeground();
        var sut = new WindowCycleService(enumerator, foreground);

        sut.MoveNext().Should().BeTrue(); // B (ctor deja índice en A)
        sut.MoveNext().Should().BeTrue(); // C
        sut.MoveNext().Should().BeTrue(); // wrap -> A

        sut.Current.Should().NotBeNull();
        sut.Current!.Hwnd.Should().Be((nint)1);
        foreground.BroughtToFront.Should().HaveCount(3);
        foreground.BroughtToFront[^1].Hwnd.Should().Be((nint)1);
    }

    [Fact] // T-3 / CA-3: anterior circular A -> C.
    public void MovePrevious_WrapsFromFirstToLast()
    {
        var enumerator = new FakeEnumerator
        {
            Windows = new() { W(1, "A"), W(2, "B"), W(3, "C") }
        };
        var foreground = new FakeForeground();
        var sut = new WindowCycleService(enumerator, foreground);

        // Refresh deja el índice en A (0); retroceder debe ir a C.
        sut.MovePrevious().Should().BeTrue();

        sut.Current!.Hwnd.Should().Be((nint)3);
        foreground.BroughtToFront.Should().ContainSingle().Which.Hwnd.Should().Be((nint)3);
    }

    [Fact] // T-5 / CA-5: sin ventanas = no-op, sin excepción.
    public void MoveNextAndPrevious_WithNoWindows_AreNoOps()
    {
        var sut = new WindowCycleService(new FakeEnumerator(), new FakeForeground());

        sut.MoveNext().Should().BeFalse();
        sut.MovePrevious().Should().BeFalse();
        sut.Current.Should().BeNull();
    }

    [Fact] // RF-6: refresca antes de cada salto (nuevas ventanas visibles sin reiniciar).
    public void MoveNext_RefreshesListBeforeJump()
    {
        var enumerator = new FakeEnumerator { Windows = new() { W(1, "A") } };
        var foreground = new FakeForeground();
        var sut = new WindowCycleService(enumerator, foreground);

        sut.MoveNext().Should().BeTrue();
        enumerator.Windows.Add(W(2, "B")); // se abre un segundo cliente

        sut.MoveNext().Should().BeTrue();
        sut.Windows.Should().HaveCount(2);
        sut.Current!.Hwnd.Should().Be((nint)2);
    }
}
