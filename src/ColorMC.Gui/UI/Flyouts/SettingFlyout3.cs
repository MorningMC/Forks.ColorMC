﻿using Avalonia.Controls;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UI.Model.Setting;

namespace ColorMC.Gui.UI.Flyouts;

public class SettingFlyout3
{
    public SettingFlyout3(Control con, SettingModel model, DnsItemModel data)
    {
        _ = new FlyoutsControl(
        [
            new FlyoutMenuObj(App.Lang("Button.Delete"), true, ()=>
            {
                 model.DeleteDns(data);
            }),
        ], con);
    }
}
