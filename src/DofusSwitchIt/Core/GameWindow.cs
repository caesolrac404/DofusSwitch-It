// DTO inmutable que identifica una ventana del juego.
// HWND se representa como nint (agnóstico 32/64 bits).
namespace DofusSwitchIt.Core;

public sealed record GameWindow(nint Hwnd, string Title, uint ProcessId)
{
    /// <summary>
    /// Nombre corto visible en la lista: primer token del título (nombre del personaje).
    /// Si no hay espacios, devuelve el título completo (SPEC-006).
    /// </summary>
    public string DisplayName => GetDisplayName(Title);

    public static string GetDisplayName(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return title;
        int i = title.IndexOf(' ');
        return i <= 0 ? title : title.Substring(0, i);
    }
}
