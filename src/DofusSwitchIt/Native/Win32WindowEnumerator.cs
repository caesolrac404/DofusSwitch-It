using DofusSwitchIt.Core;

namespace DofusSwitchIt.Native;

/// <summary>
/// Implementación real de <see cref="IWindowEnumerator"/> con EnumWindows (RF-1).
/// El predicado de "es ventana Dofus" es inyectable para validar con Spy++ (SPEC-001 riesgo abierto).
/// </summary>
public sealed class Win32WindowEnumerator : IWindowEnumerator
{
    private readonly Func<string, uint, bool> _isDofusWindow;

    public Win32WindowEnumerator(Func<string, uint, bool>? isDofusWindow = null)
    {
        _isDofusWindow = isDofusWindow ?? DefaultPredicate;
    }

    internal static bool DefaultPredicate(string title, uint processId) =>
        title.Contains("Release", StringComparison.OrdinalIgnoreCase);

    public IReadOnlyList<GameWindow> EnumerateDofusWindows()
    {
        var result = new List<GameWindow>();

        NativeMethods.EnumWindows((hWnd, _) =>
        {
            if (!NativeMethods.IsWindowVisible(hWnd))
                return true; // continuar enumerando

            int length = NativeMethods.GetWindowTextLengthW(hWnd);
            if (length == 0)
                return true;

            string title;
            unsafe
            {
                char* buffer = stackalloc char[length + 1];
                int copied = NativeMethods.GetWindowTextW(hWnd, buffer, length + 1);
                title = new string(buffer, 0, copied);
            }

            NativeMethods.GetWindowThreadProcessId(hWnd, out uint processId);

            if (_isDofusWindow(title, processId))
                result.Add(new GameWindow(hWnd, title, processId));

            return true;
        }, nint.Zero);

        return result;
    }
}
