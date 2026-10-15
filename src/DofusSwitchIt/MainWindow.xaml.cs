using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using DofusSwitchIt.Core;
using DofusSwitchIt.Native;

namespace DofusSwitchIt;

/// <summary>
/// Tira delgada siempre visible (SPEC-005): lista numerada con drag &amp; drop, etiquetas F2/F3
/// y botón de refresco con icono. Sin botones de texto. La lógica vive en
/// <see cref="WindowCycleService"/> (testeado).
/// </summary>
public partial class MainWindow : Window
{
    private const string HwndFormat = "DofusSwitchIt.Hwnd";

    private WindowCycleService? _cycle;
    private IForegroundService? _foreground;
    private HotKeyRegistrar? _hotKeys;
    private readonly ObservableCollection<string> _items = new();

    private nint? _dragHwnd;
    private Point _dragStart;
    private string _hotkeyWarning = "";
    private HelpWindow? _help;

    public MainWindow()
    {
        InitializeComponent();
        RestorePosition();
        WindowList.ItemsSource = _items;
        Closed += (_, _) =>
        {
            new WindowPlacementStore().Save(new WindowPlacement(Left, Top));
            _hotKeys?.Dispose();
        };
    }

    private void RestorePosition()
    {
        var placement = new WindowPlacementStore().Load();
        if (placement is null || !IsOnScreen(placement))
            return;
        Left = placement.Left;
        Top = placement.Top;
    }

    private static bool IsOnScreen(WindowPlacement placement) =>
        placement.Left >= SystemParameters.VirtualScreenLeft
        && placement.Top >= SystemParameters.VirtualScreenTop
        && placement.Left < SystemParameters.VirtualScreenLeft + SystemParameters.VirtualScreenWidth
        && placement.Top < SystemParameters.VirtualScreenTop + SystemParameters.VirtualScreenHeight;

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        var enumerator = new Win32WindowEnumerator(
            (title, pid) => Win32WindowEnumerator.DefaultPredicate(title, pid)
                            && pid != (uint)Environment.ProcessId); // excluir la propia app
        _foreground = new Win32ForegroundService();
        _cycle = new WindowCycleService(enumerator, _foreground);

        var source = (HwndSource)PresentationSource.FromVisual(this);
        _hotKeys = new HotKeyRegistrar(source);
        TryRegister(HotKeyDefaults.PreviousId, HotKeyDefaults.Modifiers, HotKeyDefaults.PreviousKey,
            "F2 (anterior)", () => { _cycle.MovePrevious(); RefreshList(); });
        TryRegister(HotKeyDefaults.NextId, HotKeyDefaults.Modifiers, HotKeyDefaults.NextKey,
            "F3 (siguiente)", () => { _cycle.MoveNext(); RefreshList(); });

        RefreshList();
    }

    private void TryRegister(int id, uint modifiers, uint key, string label, Action handler)
    {
        try
        {
            _hotKeys!.Register(id, modifiers, key, () => Dispatcher.Invoke(handler));
        }
        catch (Win32Exception)
        {
            _hotkeyWarning = " ⚠";
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
            StatusText.Text = "0 pjs." + _hotkeyWarning;
            return;
        }

        for (int i = 0; i < windows.Count; i++)
            _items.Add($"{i + 1}. {windows[i].DisplayName}");

        nint? target = selectHwnd ?? _cycle.Current?.Hwnd;
        WindowList.SelectedIndex = target is not null
            ? windows.ToList().FindIndex(w => w.Hwnd == target.Value)
            : -1;
        StatusText.Text = $"{windows.Count} pjs." + _hotkeyWarning;
    }

    private void RefreshButton_Click(object sender, RoutedEventArgs e) => RefreshList();

    private void HelpButton_Click(object sender, RoutedEventArgs e)
    {
        if (_help is null)
        {
            _help = new HelpWindow { Owner = this };
            _help.Closed += (_, _) => _help = null;
            _help.Show();
        }
        else
        {
            if (_help.WindowState == WindowState.Minimized)
                _help.WindowState = WindowState.Normal;
            _help.Activate();
        }
    }

    private void WindowList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (_cycle is null || _foreground is null || WindowList.SelectedIndex < 0)
            return;

        _cycle.Refresh();
        if (WindowList.SelectedIndex < _cycle.Windows.Count)
            _foreground.BringToFront(_cycle.Windows[WindowList.SelectedIndex]);
        RefreshList();
    }

    private void WindowList_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _dragStart = e.GetPosition(WindowList);
        _dragHwnd = GetHwndAtPoint(_dragStart);
    }

    private void WindowList_MouseMove(object sender, MouseEventArgs e)
    {
        if (_dragHwnd is null || e.LeftButton != MouseButtonState.Pressed)
            return;

        var current = e.GetPosition(WindowList);
        if (Math.Abs(current.X - _dragStart.X) < SystemParameters.MinimumHorizontalDragDistance
            && Math.Abs(current.Y - _dragStart.Y) < SystemParameters.MinimumVerticalDragDistance)
            return;

        DragDrop.DoDragDrop(WindowList, new DataObject(HwndFormat, _dragHwnd.Value.ToInt64()),
            DragDropEffects.Move);
        _dragHwnd = null;
    }

    private void WindowList_DragOver(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(HwndFormat) ? DragDropEffects.Move : DragDropEffects.None;
        e.Handled = true;
    }

    private void WindowList_Drop(object sender, DragEventArgs e)
    {
        if (_cycle is null || !e.Data.GetDataPresent(HwndFormat))
            return;

        var hwnd = new nint((long)e.Data.GetData(HwndFormat)!);
        if (!TryGetDroppedWindow(hwnd, out var moved))
            return;

        _cycle.MoveTo(moved, GetIndexAtPoint(e.GetPosition(WindowList)));
        RefreshList(hwnd);
    }

    /// <summary>Refresca y re-resuelve la ventana arrastrada; false si ya no existe.</summary>
    private bool TryGetDroppedWindow(nint hwnd, out GameWindow window)
    {
        ArgumentNullException.ThrowIfNull(_cycle);
        _cycle.Refresh();

        var found = _cycle.Windows.FirstOrDefault(w => w.Hwnd == hwnd);
        window = found!;
        return found is not null;
    }

    private nint? GetHwndAtPoint(Point point)
    {
        if (_cycle is null)
            return null;

        int index = GetIndexAtPoint(point);
        return index >= 0 && index < _cycle.Windows.Count ? _cycle.Windows[index].Hwnd : null;
    }

    private int GetIndexAtPoint(Point point)
    {
        if (_cycle is null)
            return -1;

        var element = WindowList.InputHitTest(point) as DependencyObject;
        var container = element is not null
            ? ItemsControl.ContainerFromElement(WindowList, element) as ListBoxItem
            : null;

        // Soltar bajo el último elemento = mover al final.
        return container is not null
            ? WindowList.ItemContainerGenerator.IndexFromContainer(container)
            : _cycle.Windows.Count;
    }
}
