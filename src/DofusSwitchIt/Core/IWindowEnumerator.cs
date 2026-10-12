namespace DofusSwitchIt.Core;

/// <summary>
/// Abstrae la enumeración Win32 (EnumWindows). Los tests la sustituyen con fakes.
/// </summary>
public interface IWindowEnumerator
{
    IReadOnlyList<GameWindow> EnumerateDofusWindows();
}
