using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Utils;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model.Items;

public partial class ServerPackItemModel : ObservableObject
{
    [ObservableProperty]
    private string _url;
    [ObservableProperty]
    private string? _pID;
    [ObservableProperty]
    private string? fID;
    [ObservableProperty]
    public string _sha256;
    [ObservableProperty]
    public bool _check;
    [ObservableProperty]
    public string _fileName;

    public string Source
    {
        get
        {
            if (string.IsNullOrWhiteSpace(FID) || string.IsNullOrWhiteSpace(PID))
            {
                return "";
            }
            else if (FuntionUtils.CheckNotNumber(PID) || FuntionUtils.CheckNotNumber(FID))
            {
                return SourceType.Modrinth.GetName();
            }
            else
            {
                return SourceType.CurseForge.GetName();
            }
        }
    }

    public ModDisplayModel Mod;
    public ResourcepackObj Resourcepack;
}
