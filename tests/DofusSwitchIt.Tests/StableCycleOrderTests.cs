using DofusSwitchIt.Core;
using FluentAssertions;

namespace DofusSwitchIt.Tests;

/// <summary>
/// SPEC-003, T-10/T-11: regresión del bug "F3 solo alterna 2 de 3 ventanas".
/// Simula el comportamiento real de EnumWindows: devuelve las ventanas en orden Z y
/// la última enfocada pasa al frente, por lo que el orden cambia tras cada salto.
/// </summary>
public sealed class StableCycleOrderTests
{
    private sealed class ZOrderEnumerator : IWindowEnumerator
    {
        public List<GameWindow> Order { get; } = new();
        public IReadOnlyList<GameWindow> EnumerateDofusWindows() => Order.AsReadOnly();

        public void MoveToFront(GameWindow w)
        {
            Order.RemoveAll(x => x.Hwnd == w.Hwnd);
            Order.Insert(0, w);
        }
    }

    private sealed class ZOrderForeground : IForegroundService
    {
        private readonly ZOrderEnumerator _enumerator;
        public List<GameWindow> Calls { get; } = new();

        public ZOrderForeground(ZOrderEnumerator enumerator) => _enumerator = enumerator;

        public void BringToFront(GameWindow window)
        {
            Calls.Add(window);
            _enumerator.MoveToFront(window); // como el orden Z real tras SetForegroundWindow
        }
    }

    private static (WindowCycleService Sut, ZOrderForeground Fg) CreateSut()
    {
        var enumerator = new ZOrderEnumerator();
        enumerator.Order.AddRange(new[]
        {
            new GameWindow((nint)1, "A", 1),
            new GameWindow((nint)2, "B", 2),
            new GameWindow((nint)3, "C", 3),
        });
        var foreground = new ZOrderForeground(enumerator);
        return (new WindowCycleService(enumerator, foreground), foreground);
    }

    [Fact] // T-10 / CA-1: F3 debe visitar las 3 antes de repetir, aun con reordenamiento Z.
    public void MoveNext_VisitsAllThree_BeforeRepeating_DespiteZOrderChanges()
    {
        var (sut, fg) = CreateSut();

        sut.MoveNext();
        sut.MoveNext();
        sut.MoveNext();

        fg.Calls.Select(w => w.Hwnd).Should().Equal((nint)2, (nint)3, (nint)1);
        sut.Current!.Hwnd.Should().Be((nint)1);
    }

    [Fact] // T-11 / CA-2: F2 sigue visitando las 3 (sin regresión).
    public void MovePrevious_VisitsAllThree_BeforeRepeating_DespiteZOrderChanges()
    {
        var (sut, fg) = CreateSut();

        sut.MovePrevious();
        sut.MovePrevious();
        sut.MovePrevious();

        fg.Calls.Select(w => w.Hwnd).Should().Equal((nint)3, (nint)2, (nint)1);
        sut.Current!.Hwnd.Should().Be((nint)1);
    }
}
