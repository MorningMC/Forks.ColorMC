﻿using System.Threading.Tasks;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Manager;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.Main;

public partial class MainModel : TopModel, IMutTop
{
    public const string SwitchView = "SwitchView";

    public bool IsLaunch;
    public bool IsFirst = true;

    public bool IsPhone { get; } = SystemInfo.Os == OsType.Android;

    private bool _isCancel;

    [ObservableProperty]
    private (string, ushort) _server;

    [ObservableProperty]
    private bool _motdDisplay;
    [ObservableProperty]
    private bool _isGameError;

    [ObservableProperty]
    private bool _sideDisplay = true;
    [ObservableProperty]
    private bool _musicDisplay;
    [ObservableProperty]
    private bool _backDisplay;
    [ObservableProperty]
    private bool _playerDisplay = true;
    [ObservableProperty]
    private bool _menuDisplay = true;

    [ObservableProperty]
    private bool _render = true;

    [ObservableProperty]
    private bool _cardNews;
    [ObservableProperty]
    private bool _haveCard = true;

    [ObservableProperty]
    private string _helloText;

    [ObservableProperty]
    private string _audioIcon = _icons[1];

    private bool _isGetNewInfo;
    private int _helloClick;

    public MainModel(BaseModel model) : base(model)
    {
        ImageManager.SkinChange += SkinChange;
        UserBinding.UserEdit += LoadUser;

        MusicVolume = GuiConfigUtils.Config.ServerCustom.Volume;

        ShowHello();
    }

    [RelayCommand]
    public void ShowCount()
    {
        WindowManager.ShowCount();
    }

    [RelayCommand]
    public void ShowSkin()
    {
        WindowManager.ShowSkin();
    }

    [RelayCommand]
    public void ShowUser()
    {
        WindowManager.ShowUser();
    }

    [RelayCommand]
    public void ShowSetting()
    {
        WindowManager.ShowSetting(SettingType.Normal);
    }

    [RelayCommand]
    public async Task OpenGuide()
    {
        var list = LanguageBinding.GetGuide();
        var res = await Model.ShowCombo(App.Lang("SettingWindow.Tab7.Info3"), list);
        if (res.Cancel)
        {
            return;
        }
        WebBinding.OpenWeb(res.Index == 0 ? WebType.Guide1 : WebType.Guide);
    }

    [RelayCommand]
    public void OpenNetFrp()
    {
        if (UserBinding.HaveOnline())
        {
            WindowManager.ShowNetFrp();
        }
        else
        {
            Model.Show(App.Lang("MainWindow.Error6"));
        }
    }

    [RelayCommand]
    public void OpenNews()
    {
        WindowManager.ShowNews();
    }

    protected override void MinModeChange()
    {
        foreach (var item in GameGroups)
        {
            item.SetMinMode(MinMode);
        }
    }

    public void HelloClick()
    {
        _helloClick++;
        if (_helloClick == 25)
        {
            HelloText = "锟斤拷锟斤拷烫烫烫烫烫烫烫烫烫";
        }
        else
        {
            HelloText = App.Lang("Hello.Text1");
        }
    }

    private void ShowHello()
    {
        HelloText = App.Lang("Hello.Text1");
    }

    private void SkinChange()
    {
        Head = ImageManager.HeadBitmap!;

        IsHeadLoad = false;
    }

    public void LoadMotd()
    {
        var config = GuiConfigUtils.Config.ServerCustom;
        if (config != null && config?.Motd == true &&
            !string.IsNullOrWhiteSpace(config?.IP))
        {
            MotdDisplay = true;

            Server = (config.IP, config.Port);
        }
        else
        {
            MotdDisplay = false;
        }
    }

    public void LoadDone()
    {
        LoadGameItem();
        LoadUser();
        LoadMotd();
        LoadNews();
        LoadMusic();

        var config = GuiConfigUtils.Config;
        if (config.Live2D?.LowFps == true)
        {
            LowFps = true;
        }
        if (config?.CheckUpdate == true)
        {
            CheckUpdate();
        }

        LoadCard();
    }

    private void LoadCard()
    {
        var config = GuiConfigUtils.Config.Card;
        CardNews = config.News;
        if (!config.Online)
        {
            IsOnlineMode = false;
        }
        if (!config.Last)
        {
            HaveLast = false;
        }

        if (!CardNews && !IsOnlineMode && !HaveLast && !HaveUpdate)
        {
            HaveCard = false;
        }
        else
        {
            HaveCard = true;
        }
    }

    public void ExportCmd(GameSettingObj obj)
    {
        GameBinding.ExportCmd(obj, Model);
    }

    public override void Close()
    {
        GroupList.Clear();
        foreach (var item in GameGroups)
        {
            item.Close();
        }
        GameGroups.Clear();
        Launchs.Clear();
    }
}
