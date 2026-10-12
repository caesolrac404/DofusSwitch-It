using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace DofusSwitchIt.Native;

/// <summary>
/// Registra hotkeys globales (RegisterHotKey) y los libera en Dispose (AGENT.md §4.1, RF-5).
/// Uso típico en MainWindow: registrar con el HwndSource de la ventana y suscribirse a Pressed.
/// </summary>
public sealed class HotKeyRegistrar : IDisposable
{
    private readonly HwndSource _source;
    private readonly Dictionary<int, Action> _handlers = new();
    private bool _disposed;

    public HotKeyRegistrar(HwndSource source)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _source.AddHook(WndProc);
    }

    /// <summary>Registra un hotkey global. Lanza Win32Exception si falla (ej. ya registrado).</summary>
    public void Register(int id, uint modifiers, uint virtualKey, Action handler)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(handler);

        if (!NativeMethods.RegisterHotKey(_source.Handle, id, modifiers, virtualKey))
            throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());

        _handlers[id] = handler;
    }

    private nint WndProc(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
    {
        if (msg == NativeMethods.WM_HOTKEY && _handlers.TryGetValue(wParam.ToInt32(), out var handler))
        {
            handler();
            handled = true;
        }
        return nint.Zero;
    }

    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;

        _source.RemoveHook(WndProc);
        foreach (var id in _handlers.Keys)
            NativeMethods.UnregisterHotKey(_source.Handle, id);
        _handlers.Clear();

        GC.SuppressFinalize(this);
    }
}
