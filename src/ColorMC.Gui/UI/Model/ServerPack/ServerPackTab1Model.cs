﻿using ColorMC.Core.Objs;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.ServerPack;

public partial class ServerPackModel : MenuModel
{
    [ObservableProperty]
    private string _text;

    private bool _loadConfig;

    partial void OnTextChanged(string value)
    {
        SaveConfig();
    }

    public void LoadConfig()
    {
        _loadConfig = true;
        Text = Obj.Text;
        _loadConfig = false;
    }

    private void SaveConfig()
    {
        if (_loadConfig)
            return;

        GameBinding.SaveServerPack(Obj);
    }
}
