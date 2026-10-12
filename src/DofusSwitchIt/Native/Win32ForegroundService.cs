using DofusSwitchIt.Core;

namespace DofusSwitchIt.Native;

/// <summary>
/// Implementación real de <see cref="IForegroundService"/> (RF-4):
/// si está minimizada → ShowWindow(SW_RESTORE) → SetForegroundWindow.
/// </summary>
public sealed class Win32ForegroundService : IForegroundService
{
    public void BringToFront(GameWindow window)
    {
        ArgumentNullException.ThrowIfNull(window);

        if (NativeMethods.IsIconic(window.Hwnd))
            NativeMethods.ShowWindow(window.Hwnd, NativeMethods.SW_RESTORE);

        NativeMethods.SetForegroundWindow(window.Hwnd);
    }
}
