using System.IO;
using System.Text.Json;

namespace DofusSwitchIt.Core;

/// <summary>Posición de la ventana en pantalla (SPEC-007).</summary>
public sealed record WindowPlacement(double Left, double Top);

/// <summary>
/// Persiste la posición de la ventana en %AppData%/DofusSwitchIt/window.json.
/// Todo fallo de IO se traga: nunca debe romper el arranque (RNF-1).
/// La ruta es inyectable para hacerlo testeable.
/// </summary>
public sealed class WindowPlacementStore
{
    private readonly string _path;

    public WindowPlacementStore(string? path = null) =>
        _path = path ?? DefaultPath;

    public static string DefaultPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "DofusSwitchIt", "window.json");

    public WindowPlacement? Load()
    {
        try
        {
            if (!File.Exists(_path))
                return null;
            var placement = JsonSerializer.Deserialize<WindowPlacement>(File.ReadAllText(_path));
            return placement is not null && IsValid(placement) ? placement : null;
        }
        catch
        {
            return null;
        }
    }

    public void Save(WindowPlacement placement)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
            File.WriteAllText(_path, JsonSerializer.Serialize(placement));
        }
        catch
        {
            // no-op: persistir la posición nunca debe fallar la app
        }
    }

    private static bool IsValid(WindowPlacement p) =>
        double.IsFinite(p.Left) && double.IsFinite(p.Top);
}
