namespace DofusSwitchIt.Core;

/// <summary>
/// Ciclo circular sobre las ventanas de Dofus (SPEC-001).
/// Clase pura: sin referencias a System.Windows ni P/Invoke. Totalmente unit-testeable.
/// </summary>
public sealed class WindowCycleService
{
    private readonly IWindowEnumerator _enumerator;
    private readonly IForegroundService _foreground;
    private List<GameWindow> _windows = new();
    private int _index = -1;

    public WindowCycleService(IWindowEnumerator enumerator, IForegroundService foreground)
    {
        _enumerator = enumerator ?? throw new ArgumentNullException(nameof(enumerator));
        _foreground = foreground ?? throw new ArgumentNullException(nameof(foreground));
        Refresh();
    }

    public IReadOnlyList<GameWindow> Windows => _windows.AsReadOnly();

    public GameWindow? Current =>
        _index >= 0 && _index < _windows.Count ? _windows[_index] : null;

    /// <summary>
    /// Re-enumera las ventanas (RF-6). Conserva el índice si la ventana actual sigue existiendo.
    /// </summary>
    public void Refresh()
    {
        var current = Current;
        _windows = _enumerator.EnumerateDofusWindows().ToList();
        if (_windows.Count == 0)
        {
            _index = -1;
            return;
        }

        _index = current is not null
            ? _windows.FindIndex(w => w.Hwnd == current.Hwnd)
            : -1;

        if (_index < 0)
            _index = 0;
    }

    /// <summary>Siguiente ventana (circular). No-op si no hay ventanas (RNF-4).</summary>
    public bool MoveNext()
    {
        Refresh();
        if (_windows.Count == 0)
            return false;

        _index = _index < 0 ? 0 : (_index + 1) % _windows.Count;
        _foreground.BringToFront(_windows[_index]);
        return true;
    }

    /// <summary>Ventana anterior (circular). No-op si no hay ventanas (RNF-4).</summary>
    public bool MovePrevious()
    {
        Refresh();
        if (_windows.Count == 0)
            return false;

        _index = _index < 0
            ? _windows.Count - 1
            : (_index - 1 + _windows.Count) % _windows.Count;
        _foreground.BringToFront(_windows[_index]);
        return true;
    }
}
