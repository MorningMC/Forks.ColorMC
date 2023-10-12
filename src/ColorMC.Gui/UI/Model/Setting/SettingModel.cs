﻿using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;

namespace ColorMC.Gui.UI.Model.Setting;

public partial class SettingModel : MenuModel
{
    protected override List<MenuObj> TabItems { get; init; } = new()
    {
        new() { Icon = "/Resource/Icon/Setting/item1.svg",
            Text = App.GetLanguage("SettingWindow.Tabs.Text2") },
        new() { Icon = "/Resource/Icon/Setting/item2.svg",
            Text = App.GetLanguage("SettingWindow.Tabs.Text3") },
        new() { Icon = "/Resource/Icon/Setting/item3.svg",
            Text = App.GetLanguage("SettingWindow.Tabs.Text4") },
        new() { Icon = "/Resource/Icon/Setting/item4.svg",
            Text = App.GetLanguage("SettingWindow.Tabs.Text5") },
        new() { Icon = "/Resource/Icon/Setting/item5.svg",
            Text = App.GetLanguage("SettingWindow.Tabs.Text6") },
        new() { Icon = "/Resource/Icon/Setting/item6.svg",
            Text = App.GetLanguage("SettingWindow.Tabs.Text1") },
        new() { Icon = "/Resource/Icon/Setting/item7.svg",
            Text = App.GetLanguage("SettingWindow.Tabs.Text7") },
    };

    public bool Phone { get; } = false;

    public SettingModel(BaseModel model) : base(model)
    {
        if (SystemInfo.Os == OsType.Linux)
        {
            _enableWindowMode = false;
        }
        else if (SystemInfo.Os == OsType.Android)
        {
            Phone = true;
            _enableWindowMode = false;
        }
    }

    protected override void Close()
    {
        FontList.Clear();
        JavaList.Clear();
        _uuids.Clear();
        GameList.Clear();
    }
}
