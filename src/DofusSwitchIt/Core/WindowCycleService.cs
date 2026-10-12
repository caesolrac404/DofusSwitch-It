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
    /// Re-enumera las ventanas (RF-6). El orden manual es la fuente de verdad (SPEC-004):
    /// conserva las posiciones existentes, elimina cerradas y agrega las nuevas al final.
    /// El orden inicial es estable (título + HWND) porque el orden Z de EnumWindows cambia
    /// con cada SetForegroundWindow y provocaba ping-pong en MoveNext (SPEC-003).
    /// </summary>
    public void Refresh()
    {
        var currentHwnd = Current?.Hwnd;
        var fresh = _enumerator.EnumerateDofusWindows()
            .OrderBy(w => w.Title, StringComparer.OrdinalIgnoreCase)
            .ThenBy(w => w.Hwnd)
            .ToList();

        if (fresh.Count == 0)
        {
            _windows.Clear();
            _index = -1;
            return;
        }

        var freshByHwnd = fresh.ToDictionary(w => w.Hwnd);
        var kept = _windows.Where(w => freshByHwnd.ContainsKey(w.Hwnd))
            .Select(w => freshByHwnd[w.Hwnd])
            .ToList();
        var known = new HashSet<nint>(kept.Select(w => w.Hwnd));
        kept.AddRange(fresh.Where(w => !known.Contains(w.Hwnd)));
        _windows = kept;

        _index = currentHwnd is not null
            ? _windows.FindIndex(w => w.Hwnd == currentHwnd.Value)
            : -1;

        if (_index < 0)
            _index = 0;
    }

    /// <summary>Sube una ventana en el orden del ciclo (SPEC-004). No-op en el primer puesto.</summary>
    public void MoveUp(GameWindow window) => Move(window, -1);

    /// <summary>Baja una ventana en el orden del ciclo (SPEC-004). No-op en el último puesto.</summary>
    public void MoveDown(GameWindow window) => Move(window, +1);

    /// <summary>
    /// Mueve una ventana a una posición absoluta (SPEC-005, drag &amp; drop).
    /// Índices fuera de rango hacen clamp; ventana inexistente = no-op.
    /// </summary>
    public void MoveTo(GameWindow window, int newIndex)
    {
        ArgumentNullException.ThrowIfNull(window);

        int i = _windows.FindIndex(w => w.Hwnd == window.Hwnd);
        if (i < 0 || _windows.Count == 0)
            return;

        var currentHwnd = Current?.Hwnd;
        var item = _windows[i];
        _windows.RemoveAt(i);
        _windows.Insert(Math.Clamp(newIndex, 0, _windows.Count), item);

        _index = currentHwnd is not null
            ? _windows.FindIndex(w => w.Hwnd == currentHwnd.Value)
            : -1;
    }

    private void Move(GameWindow window, int delta)
    {
        ArgumentNullException.ThrowIfNull(window);

        var currentHwnd = Current?.Hwnd;
        int i = _windows.FindIndex(w => w.Hwnd == window.Hwnd);
        int j = i + delta;
        if (i < 0 || j < 0 || j >= _windows.Count)
            return;

        (_windows[i], _windows[j]) = (_windows[j], _windows[i]);

        _index = currentHwnd is not null
            ? _windows.FindIndex(w => w.Hwnd == currentHwnd.Value)
            : -1;
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
