namespace DofusSwitchIt.Core;

/// <summary>
/// Mapeo de hotkeys globales por defecto (SPEC-002). Sin dependencias WPF: unit-testeable.
/// F2 = anterior/atrás, F3 = siguiente/adelante, sin modificadores (hotkeys directas).
/// </summary>
public static class HotKeyDefaults
{
    public const int PreviousId = 1;
    public const int NextId = 2;

    public const uint Modifiers = 0; // sin Ctrl/Alt/Shift/Win

    public const uint PreviousKey = 0x71; // VK_F2
    public const uint NextKey = 0x72;     // VK_F3
}
