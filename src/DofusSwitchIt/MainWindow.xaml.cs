using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using DofusSwitchIt.Core;
using DofusSwitchIt.Native;

namespace DofusSwitchIt;

/// <summary>
/// Ventana principal (SPEC-002): lista didáctica de ventanas Dofus + hotkeys globales F2/F3.
/// Code-behind mínimo; la lógica vive en <see cref="WindowCycleService"/> (testeado).
/// </summary>
public partial class MainWindow : Window
{
    private WindowCycleService? _cycle;
    private IForegroundService? _foreground;
    private HotKeyRegistrar? _hotKeys;
    private readonly ObservableCollection<string> _items = new();
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromSeconds(1) };

    public MainWindow()
    {
        InitializeComponent();
        WindowList.ItemsSource = _items;
        _timer.Tick += (_, _) => RefreshList();
        Closed += (_, _) =>
        {
            _timer.Stop();
            _hotKeys?.Dispose();
        };
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        var enumerator = new Win32WindowEnumerator(
            (title, pid) => Win32WindowEnumerator.DefaultPredicate(title, pid)
                            && pid != (uint)Environment.ProcessId); // excluir la propia app ("DofusSwitch-It" contiene "Dofus")
        _foreground = new Win32ForegroundService();
        _cycle = new WindowCycleService(enumerator, _foreground);

        var source = (HwndSource)PresentationSource.FromVisual(this);
        _hotKeys = new HotKeyRegistrar(source);
        TryRegister(HotKeyDefaults.PreviousId, HotKeyDefaults.Modifiers, HotKeyDefaults.PreviousKey,
            "F2 (anterior)", () => { _cycle.MovePrevious(); RefreshList(); });
        TryRegister(HotKeyDefaults.NextId, HotKeyDefaults.Modifiers, HotKeyDefaults.NextKey,
            "F3 (siguiente)", () => { _cycle.MoveNext(); RefreshList(); });

        _timer.Start();
        RefreshList();
    }

    private void TryRegister(int id, uint modifiers, uint key, string label, Action handler)
    {
        try
        {
            _hotKeys!.Register(id, modifiers, key, () => Dispatcher.Invoke(handler));
        }
        catch (Win32Exception ex)
        {
            StatusText.Text += $" No se pudo registrar {label}: {ex.Message}. Los botones siguen funcionando.";
        }
    }

    private void RefreshList(nint? selectHwnd = null)
    {
        if (_cycle is null)
            return;

        _cycle.Refresh();
        var windows = _cycle.Windows;

        _items.Clear();
        if (windows.Count == 0)
        {
            StatusText.Text = "No se detectaron ventanas de Dofus. Abre al menos un cliente del juego.";
            return;
        }

        for (int i = 0; i < windows.Count; i++)
            _items.Add($"{i + 1}. {windows[i].Title}");

        nint? target = selectHwnd ?? _cycle.Current?.Hwnd;
        WindowList.SelectedIndex = target is not null
            ? windows.ToList().FindIndex(w => w.Hwnd == target.Value)
            : -1;
        StatusText.Text = $"{windows.Count} ventana(s) detectada(s). F2 = anterior, F3 = siguiente.";
    }

    private void PreviousButton_Click(object sender, RoutedEventArgs e)
    {
        _cycle?.MovePrevious();
        RefreshList();
    }

    private void NextButton_Click(object sender, RoutedEventArgs e)
    {
        _cycle?.MoveNext();
        RefreshList();
    }

    private void RefreshButton_Click(object sender, RoutedEventArgs e) => RefreshList();

    private void MoveUpButton_Click(object sender, RoutedEventArgs e) => MoveSelected(-1);

    private void MoveDownButton_Click(object sender, RoutedEventArgs e) => MoveSelected(+1);

    private void MoveSelected(int delta)
    {
        if (_cycle is null || WindowList.SelectedIndex < 0)
            return;

        _cycle.Refresh();
        if (WindowList.SelectedIndex >= _cycle.Windows.Count)
            return;

        var selected = _cycle.Windows[WindowList.SelectedIndex];
        if (delta < 0)
            _cycle.MoveUp(selected);
        else
            _cycle.MoveDown(selected);

        RefreshList(selected.Hwnd); // la selección sigue al elemento movido
    }

    private void WindowList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (_cycle is null || _foreground is null || WindowList.SelectedIndex < 0)
            return;

        _cycle.Refresh();
        if (WindowList.SelectedIndex < _cycle.Windows.Count)
            _foreground.BringToFront(_cycle.Windows[WindowList.SelectedIndex]);
        RefreshList();
    }
}
