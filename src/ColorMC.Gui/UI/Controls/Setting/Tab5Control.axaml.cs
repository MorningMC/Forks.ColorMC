using Avalonia.Controls;
using Avalonia.Threading;
using ColorMC.Gui.UI.Flyouts;
using ColorMC.Gui.UI.Model.Setting;
using ColorMC.Gui.Utils;

namespace ColorMC.Gui.UI.Controls.Setting;

public partial class Tab5Control : UserControl
{
    public Tab5Control()
    {
        InitializeComponent();

        DataGrid1.CellPointerPressed += DataGrid1_CellPointerPressed;
    }

    public void Opened()
    {
        DataGrid1.SetFontColor();
    }

    private void DataGrid1_CellPointerPressed(object? sender,
        DataGridCellPointerPressedEventArgs e)
    {
        if (e.PointerPressedEventArgs.GetCurrentPoint(this).Properties.IsRightButtonPressed)
        {
            Flyout((sender as Control)!);
        }

        LongPressed.Pressed(() => Flyout((sender as Control)!));
    }

    private void Flyout()
    {
        Dispatcher.UIThread.Post(() =>
        {
            var model = (DataContext as SettingModel)!;
            _ = new SettingFlyout1(this, model, DataGrid1.SelectedItems);
        });
    }
}
