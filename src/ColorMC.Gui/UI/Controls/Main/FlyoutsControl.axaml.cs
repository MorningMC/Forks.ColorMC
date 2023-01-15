using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Windows;

namespace ColorMC.Gui.UI.Controls.Main;

public partial class FlyoutsControl : UserControl
{
    private GameSettingObj Obj;
    private FlyoutBase FlyoutBase;
    private MainWindow Win;
    public FlyoutsControl()
    {
        InitializeComponent();

        Button1.Click += Button1_Click;
        Button2.Click += Button2_Click;
        Button3.Click += Button3_Click;
        Button4.Click += Button4_Click;
        Button5.Click += Button5_Click;
    }

    private void Button5_Click(object? sender, RoutedEventArgs e)
    {
        FlyoutBase.Hide();
        Win.EditGroup(Obj);
    }

    private void Button4_Click(object? sender, RoutedEventArgs e)
    {
        FlyoutBase.Hide();
        App.ShowGameEdit(Obj, 3);
    }

    private void Button3_Click(object? sender, RoutedEventArgs e)
    {
        FlyoutBase.Hide();
        App.ShowGameEdit(Obj, 2);
    }

    private void Button2_Click(object? sender, RoutedEventArgs e)
    {
        FlyoutBase.Hide();
        App.ShowGameEdit(Obj, 1);
    }

    private void Button1_Click(object? sender, RoutedEventArgs e)
    {
        FlyoutBase.Hide();
        Win.Launch(false);
    }

    public void Set(FlyoutBase fb, GameSettingObj obj, MainWindow win)
    {
        Obj = obj;
        FlyoutBase = fb;
        Win = win;
    }
}

public class MainFlyout : FlyoutBase
{
    private GameSettingObj Obj;
    private MainWindow Win;
    public MainFlyout(MainWindow win, GameSettingObj obj)
    {
        Win = win;
        Obj = obj;
    }
    protected override Control CreatePresenter()
    {
        var control = new FlyoutsControl();
        control.Set(this, Obj, Win);
        return control;
    }
}