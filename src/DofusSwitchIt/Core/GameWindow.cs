// DTO inmutable que identifica una ventana del juego.
// HWND se representa como nint (agnóstico 32/64 bits).
namespace DofusSwitchIt.Core;

public sealed record GameWindow(nint Hwnd, string Title, uint ProcessId);
