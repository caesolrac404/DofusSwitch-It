namespace DofusSwitchIt.Core;

/// <summary>
/// Abstrae el cambio de foco Win32 (ShowWindow + SetForegroundWindow).
/// Los tests la sustituyen con un fake que registra llamadas.
/// </summary>
public interface IForegroundService
{
    void BringToFront(GameWindow window);
}
