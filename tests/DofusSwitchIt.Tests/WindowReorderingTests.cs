using DofusSwitchIt.Core;
using FluentAssertions;

namespace DofusSwitchIt.Tests;

/// <summary>SPEC-004, T-13/T-14/T-15: reordenamiento manual y reconciliación en Refresh().</summary>
public sealed class WindowReorderingTests
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

    private static (WindowCycleService Sut, FakeEnumerator En) CreateSut()
    {
        var enumerator = new FakeEnumerator
        {
            Windows = new() { W(1, "A"), W(2, "B"), W(3, "C") }
        };
        return (new WindowCycleService(enumerator, new FakeForeground()), enumerator);
    }

    private static string Order(WindowCycleService sut) =>
        string.Concat(sut.Windows.Select(w => w.Title));

    [Fact] // T-13 / CA-1: bajar intercambia con el siguiente y el ciclo lo respeta.
    public void MoveDown_SwapsWithNext_AndCycleFollowsNewOrder()
    {
        var (sut, _) = CreateSut();

        sut.MoveDown(sut.Windows[0]);

        Order(sut).Should().Be("BAC");
        sut.MovePrevious(); // desde A (índice 1) -> B: el ciclo respeta el orden manual
        sut.Current!.Title.Should().Be("B");
        sut.MoveNext(); // B -> A
        sut.MoveNext(); // A -> C
        sut.Current!.Title.Should().Be("C");
    }

    [Fact] // T-13: subir intercambia con el anterior.
    public void MoveUp_SwapsWithPrevious()
    {
        var (sut, _) = CreateSut();

        sut.MoveUp(sut.Windows[2]);

        Order(sut).Should().Be("ACB");
    }

    [Fact] // T-13 / CA-2: extremos son no-op.
    public void MoveUpOnFirst_AndMoveDownOnLast_AreNoOps()
    {
        var (sut, _) = CreateSut();

        sut.MoveUp(sut.Windows[0]);
        sut.MoveDown(sut.Windows[^1]);

        Order(sut).Should().Be("ABC");
    }

    [Fact] // T-14 / CA-3: el orden manual sobrevive a Refresh; cerradas fuera, nuevas al final.
    public void Refresh_KeepsManualOrder_RemovesClosed_AppendsNewAtEnd()
    {
        var (sut, enumerator) = CreateSut();
        sut.MoveUp(sut.Windows[2]); // ABC -> ACB

        enumerator.Windows = new() { W(2, "B"), W(3, "C"), W(4, "D") }; // A cerrada, D nueva
        sut.Refresh();

        Order(sut).Should().Be("CBD");
    }

    [Fact] // T-15 / RF-2: Current (por Hwnd) intacto tras mover.
    public void MoveUp_KeepsCurrentPointingAtSameWindow()
    {
        var (sut, _) = CreateSut(); // Current = A
        sut.MoveNext();             // Current = B

        sut.MoveUp(sut.Windows[1]); // BAC -> ABC... B sube: orden ABC? [A,B,C] subir B -> [B,A,C]

        sut.Current!.Hwnd.Should().Be((nint)2);
    }
}
