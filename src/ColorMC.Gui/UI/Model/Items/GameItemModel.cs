﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Joystick;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Flyouts;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogHostAvalonia;

namespace ColorMC.Gui.UI.Model.Items;

public partial class GameItemModel : GameModel
{
    private static readonly string[] s_star = ["/Resource/Icon/Item/star.svg", "/Resource/Icon/Item/star_1.svg"];

    [ObservableProperty]
    private bool _isSelect;
    [ObservableProperty]
    private bool _isLaunch;
    [ObservableProperty]
    private bool _isLaunching;
    [ObservableProperty]
    private bool _isLoad;
    [ObservableProperty]
    private bool _isDrop;
    [ObservableProperty]
    private bool _isOver;
    [ObservableProperty]
    private bool _isNew;
    [ObservableProperty]
    private bool _isCheck;

    [ObservableProperty]
    private bool _buttonShow;
    [ObservableProperty]
    private bool _showCheck;

    [ObservableProperty]
    private bool _isDisplay = true;

    [ObservableProperty]
    private string _tips;

    [ObservableProperty]
    private TextWrapping _wrap = TextWrapping.NoWrap;
    [ObservableProperty]
    private TextTrimming _trim = TextTrimming.CharacterEllipsis;

    private readonly IMainTop? _top;

    public string Name => Obj.Name;
    public string UUID => Obj.UUID;

    [ObservableProperty]
    private string _star = s_star[1];
    [ObservableProperty]
    private bool _isStar;
    [ObservableProperty]
    private bool _starVis;

    [ObservableProperty]
    private bool _oneGame;

    [ObservableProperty]
    private Bitmap _pic;

    private readonly string? _group;

    public int Index
    {
        set
        {
            switch (value)
            {
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                    {
                        using var asset = AssetLoader.Open(new Uri($"resm:ColorMC.Gui.Resource.Pic.{value}.png"));
                        Pic = new Bitmap(asset);
                    }
                    break;
            }
        }
    }

    public GameItemModel(BaseModel model, string? group) : base(model, new() { })
    {
        _group = group;
        _isNew = true;
    }

    public GameItemModel(BaseModel model, IMainTop? top, GameSettingObj obj) : base(model, obj)
    {
        _top = top;
        _group = obj.GroupName;
        LoadIcon();
        IsStar = GameManager.IsStar(obj);
    }

    partial void OnIsStarChanged(bool value)
    {
        Star = s_star[value ? 0 : 1];
        StarVis = (value || IsOver) && !IsNew;
    }

    partial void OnOneGameChanged(bool value)
    {
        if (value)
        {
            IsSelect = false;
        }
    }

    partial void OnIsLaunchChanged(bool value)
    {
        if (OneGame)
        {
            return;
        }
        if (value)
        {
            IsLaunching = true;
        }
        else
        {
            if (!IsLoad)
            {
                IsLaunching = false;
            }
        }
    }

    partial void OnIsLoadChanged(bool value)
    {
        if (value)
        {
            IsLaunching = true;
        }
        else
        {
            if (!IsLoad)
            {
                IsLaunching = false;
            }
        }
    }

    partial void OnIsOverChanged(bool value)
    {
        if (!ShowCheck)
        {
            StarVis = (value || IsStar) && !IsNew;
            ButtonShow = value || IsSelect;
        }
    }

    partial void OnIsSelectChanged(bool value)
    {
        if (value == false && OneGame)
        {
            return;
        }
        Wrap = value ? TextWrapping.Wrap : TextWrapping.NoWrap;
        Trim = value ? TextTrimming.None : TextTrimming.CharacterEllipsis;
        IsDrop = false;
        if (SystemInfo.Os == OsType.Android)
        {
            IsOver = value;
        }

        ButtonShow = value || IsOver;
    }

    [RelayCommand]
    public void DoStar()
    {
        if (ShowCheck)
        {
            return;
        }
        _top?.DoStar(this);
        IsOver = false;
    }

    [RelayCommand]
    public void AddGame()
    {
        if (ShowCheck)
        {
            return;
        }
        WindowManager.ShowAddGame(_group);
    }

    [RelayCommand]
    public void Launch()
    {
        if (ShowCheck)
        {
            return;
        }
        if (IsLaunch)
        {
            return;
        }

        _top?.Launch(this);
    }

    [RelayCommand]
    public void EditGame()
    {
        WindowManager.ShowGameEdit(Obj);
    }

    public void StartCheck()
    {
        IsSelect = false;
        IsCheck = false;
        ShowCheck = true;
    }

    public void StopCheck()
    {
        ShowCheck = false;
    }

    public void Select()
    {
        if (OneGame)
        {
            return;
        }

        IsSelect = true;
    }

    public void Unselect()
    {
        if (OneGame)
        {
            return;
        }

        IsSelect = false;
    }

    public void LoadIcon()
    {
        Pic = GetImage();
    }

    public void ReloadIcon()
    {
        Pic = ReloadImage();
    }

    public void Reload()
    {
        IsLaunch = GameManager.IsGameRun(Obj);

        SetTips();
    }

    public void SetTips()
    {
        if (IsNew)
        {
            return;
        }
        var time1 = Obj.LaunchData.AddTime;
        var time2 = Obj.LaunchData.LastTime;
        var time3 = Obj.LaunchData.LastPlay;
        var time4 = Obj.LaunchData.GameTime;
        Tips = string.Format(App.Lang("ToolTip.Text125"),
            time1.Ticks == 0 ? "" : time1.ToString(),
            time2.Ticks == 0 ? "" : time2.ToString(),
            time3.Ticks == 0 ? "" :
            $"{time3.TotalHours:#}:{time3.Minutes:00}:{time3.Seconds:00}",
            time4.Ticks == 0 ? "" :
            $"{time4.TotalHours:#}:{time4.Minutes:00}:{time4.Seconds:00}");
    }

    public async void Move(TopLevel? top, PointerEventArgs e)
    {
        if (ShowCheck)
        {
            return;
        }
        var dragData = new DataObject();
        dragData.Set(BaseBinding.DrapType, Obj.UUID);
        IsDrop = true;

        if (SystemInfo.Os != OsType.Android)
        {
            var files = new List<IStorageFolder>();
            if (top == null)
            {
                return;
            }
            var item = await top.StorageProvider
                   .TryGetFolderFromPathAsync(Obj.GetBasePath());
            files.Add(item!);
            dragData.Set(DataFormats.Files, files);
        }
        Dispatcher.UIThread.Post(() =>
        {
            DragDrop.DoDragDrop(e, dragData, DragDropEffects.Move | DragDropEffects.Link | DragDropEffects.Copy);
        });
    }

    public void SetSelect()
    {
        _top?.Select(this);
    }

    public void Flyout(Control con)
    {
        _ = new MainFlyout(con, this);
    }

    private Bitmap GetImage()
    {
        var icon = ImageManager.GetGameIcon(Obj);
        return icon ?? ImageManager.GameIcon;
    }

    private Bitmap ReloadImage()
    {
        var icon = ImageManager.ReloadImage(Obj);
        return icon ?? ImageManager.GameIcon;
    }

    public async void Rename()
    {
        var (Cancel, Text1) = await Model.ShowEdit(App.Lang("MainWindow.Info23"), Obj.Name);
        if (Cancel)
        {
            return;
        }
        if (string.IsNullOrWhiteSpace(Text1))
        {
            Model.Show(App.Lang("MainWindow.Error3"));
            return;
        }

        GameBinding.SetGameName(Obj, Text1);
    }

    public void SetJoystick()
    {
        if (GameJoystick.NowGameJoystick.TryGetValue(Obj.UUID, out var value))
        {
            var model = value.MakeConfig();
            model.TopCancel = () => { DialogHost.Close("MainCon"); };
            model.TopConfirm = () =>
            {
                DialogHost.Close("MainCon");
                value.ChangeConfig(model);
                Model.Notify(App.Lang("MainWindow.Info39"));
            };
            DialogHost.Show(model, "MainCon");
        }
    }

    public async void Copy()
    {
        var (Cancel, Text1) = await Model.ShowEdit(App.Lang("MainWindow.Info23"),
            Obj.Name + App.Lang("MainWindow.Info24"));
        if (Cancel)
        {
            return;
        }
        if (string.IsNullOrWhiteSpace(Text1))
        {
            Model.Show(App.Lang("MainWindow.Error3"));
            return;
        }

        var res = await GameBinding.CopyGame(Obj, Text1, Model.ShowWait, GameOverwirte);
        if (!res)
        {
            Model.Show(App.Lang("MainWindow.Error5"));
            return;
        }
        else
        {
            Model.Notify(App.Lang("MainWindow.Info25"));
        }
    }

    public async void DeleteGame()
    {
        var res = await Model.ShowWait(string.Format(App.Lang("MainWindow.Info19"), Obj.Name));
        if (!res)
        {
            return;
        }

        Model.Progress(App.Lang("GameEditWindow.Tab1.Info11"));
        res = await GameBinding.DeleteGame(Obj);
        Model.ProgressClose();
        Model.InputClose();
        if (!res)
        {
            Model.Show(App.Lang("MainWindow.Info37"));
        }
    }

    public void EditGroup()
    {
        _top?.EditGroup(this);
    }

    /// <summary>
    /// 请求
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    private async Task<bool> GameOverwirte(GameSettingObj obj)
    {
        return await Model.ShowWait(
            string.Format(App.Lang("AddGameWindow.Info2"), obj.Name));
    }

    public void ExportCmd()
    {
        _top?.ExportCmd(Obj);
    }

    public override void Close()
    {

    }
}
