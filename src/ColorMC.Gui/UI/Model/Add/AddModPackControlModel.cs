using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using AvaloniaEdit.Utils;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Objs.Modrinth;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UI.Model.Main;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.Add;

public partial class AddModPackControlModel : TopModel, IAddWindow
{
    public string[] SourceList { get; init; } = LanguageBinding.GetSourceList();
    public ObservableCollection<FileVersionItemModel> FileList { get; init; } = [];
    public ObservableCollection<string> GameVersionList { get; init; } = [];
    public ObservableCollection<string> CategorieList { get; init; } = [];
    public ObservableCollection<string> SortTypeList { get; init; } = [];
    public ObservableCollection<FileItemModel> DisplayList { get; init; } = [];

    private readonly Dictionary<int, string> _categories = [];
    private FileItemModel? _last;
    private bool _load = false;

    [ObservableProperty]
    private FileVersionItemModel _item;

    [ObservableProperty]
    private int _source = -1;
    [ObservableProperty]
    private int _categorie;
    [ObservableProperty]
    private int _sortType;
    [ObservableProperty]
    private int? _page = 0;
    [ObservableProperty]
    private int? _page1 = 0;
    [ObservableProperty]
    private int _maxPage;
    [ObservableProperty]
    private int _maxPage1;
    [ObservableProperty]
    private string? _gameVersion;
    [ObservableProperty]
    private string? _gameVersion1;
    [ObservableProperty]
    private string? _text;
    [ObservableProperty]
    private bool _enable1 = true;
    [ObservableProperty]
    private bool _isSelect = false;
    [ObservableProperty]
    private bool _display = false;
    [ObservableProperty]
    private bool _emptyDisplay = true;
    [ObservableProperty]
    private bool _sourceLoad;
    [ObservableProperty]
    private bool _emptyVersionDisplay;
    [ObservableProperty]
    private bool _enableNextPage;
    [ObservableProperty]
    private bool _enableNextPage1;

    private bool _keep = false;

    private readonly string _useName;

    public AddModPackControlModel(BaseModel model) : base(model)
    {
        _useName = ToString() ?? "AddModPackControlModel";
    }

    partial void OnDisplayChanged(bool value)
    {
        if (value)
        {
            Model.PushBack(back: () =>
            {
                Display = false;
            });
        }
        else
        {
            Model.PopBack();
        }
    }

    partial void OnGameVersion1Changed(string? value)
    {
        Load1();
    }

    partial void OnPage1Changed(int? value)
    {
        Load1();
    }

    partial void OnCategorieChanged(int value)
    {
        if (_load)
            return;

        Load();
    }

    partial void OnSortTypeChanged(int value)
    {
        if (_load)
            return;

        Load();
    }

    partial void OnGameVersionChanged(string? value)
    {
        if (_load)
            return;


        GameVersion1 = value;

        Load();
    }

    partial void OnPageChanged(int? value)
    {
        if (_load)
            return;

        Load();
    }

    partial void OnSourceChanged(int value)
    {
        LoadSourceData();
    }

    [RelayCommand]
    public void Select()
    {
        if (_last == null)
        {
            Model.Show(App.Lang("AddModPackWindow.Error1"));
            return;
        }

        Install();
    }

    [RelayCommand]
    public void Reload()
    {
        if (!string.IsNullOrWhiteSpace(Text) && Page != 0)
        {
            Page = 0;
            return;
        }

        Load();
    }

    [RelayCommand]
    public void Search()
    {
        Load1();
    }

    [RelayCommand]
    public async Task Download()
    {
        if (Item == null)
            return;

        var res = await Model.ShowWait(
            string.Format(App.Lang("AddModPackWindow.Info1"), Item.Name));
        if (res)
        {
            Install(Item);
        }
    }

    public async void LoadSourceData()
    {
        if (_load)
        {
            return;
        }

        SourceLoad = false;
        _load = true;

        IsSelect = false;

        CategorieList.Clear();
        SortTypeList.Clear();

        GameVersionList.Clear();
        _categories.Clear();

        ClearList();

        switch (Source)
        {
            case 0:
            case 1:
                SortTypeList.AddRange(Source == 0 ?
                    LanguageBinding.GetCurseForgeSortTypes() :
                    LanguageBinding.GetModrinthSortTypes());

                Model.Progress(App.Lang("AddModPackWindow.Info4"));
                var list = Source == 0 ?
                    await GameBinding.GetCurseForgeGameVersions() :
                    await GameBinding.GetModrinthGameVersions();
                var list1 = Source == 0 ?
                    await GameBinding.GetCurseForgeCategories() :
                    await GameBinding.GetModrinthCategories();
                Model.ProgressClose();
                if (list == null || list1 == null)
                {
                    _load = false;
                    LoadFail();
                    return;
                }
                GameVersionList.AddRange(list);

                _categories.Add(0, "");
                var a = 1;
                foreach (var item in list1)
                {
                    _categories.Add(a++, item.Key);
                }

                var list2 = new List<string>()
                {
                    ""
                };

                list2.AddRange(list1.Values);

                CategorieList.AddRange(list2);

                Categorie = 0;
                GameVersion = GameVersionList.FirstOrDefault();
                SortType = Source == 0 ? 1 : 0;

                Load();
                break;
        }

        SourceLoad = true;
        _load = false;
    }

    public void SetSelect(FileItemModel last)
    {
        IsSelect = true;
        if (_last != null)
        {
            _last.IsSelect = false;
        }
        _last = last;
        _last.IsSelect = true;
    }

    public void Install()
    {
        Display = true;
        Load1();
    }

    private void ZipUpdate(string text, int size, int all)
    {
        string temp = App.Lang("AddGameWindow.Tab1.Info21");
        Dispatcher.UIThread.Post(() => Model.ProgressUpdate($"{temp} {text} {size}/{all}"));
    }

    /// <summary>
    /// 请求
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    private async Task<bool> GameRequest(string text)
    {
        Model.ProgressClose();
        var test = await Model.ShowWait(text);
        Model.Progress();
        return test;
    }

    /// <summary>
    /// 请求
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    private async Task<bool> GameOverwirte(GameSettingObj obj)
    {
        Model.ProgressClose();
        var test = await Model.ShowWait(
            string.Format(App.Lang("AddGameWindow.Info2"), obj.Name));
        Model.Progress();
        return test;
    }

    /// <summary>
    /// 添加进度
    /// </summary>
    /// <param name="state"></param>
    private void PackState(CoreRunState state)
    {
        if (state == CoreRunState.Read)
        {
            Model.Progress(App.Lang("AddGameWindow.Tab2.Info1"));
        }
        else if (state == CoreRunState.Init)
        {
            Model.ProgressUpdate(App.Lang("AddGameWindow.Tab2.Info2"));
        }
        else if (state == CoreRunState.GetInfo)
        {
            Model.ProgressUpdate(App.Lang("AddGameWindow.Tab2.Info3"));
        }
        else if (state == CoreRunState.Download)
        {
            Model.ProgressUpdate(-1);
            if (!ConfigBinding.WindowMode())
            {
                Model.ProgressUpdate(App.Lang("AddGameWindow.Tab2.Info4"));
            }
            else
            {
                Model.ProgressClose();
            }
        }
        else if (state == CoreRunState.DownloadDone)
        {
            if (ConfigBinding.WindowMode())
            {
                Model.Progress(App.Lang("AddGameWindow.Tab2.Info4"));
            }
        }
    }

    private void UpdateProcess(int size, int now)
    {
        Model.ProgressUpdate((double)now / size);
    }

    public async void Install(FileVersionItemModel data)
    {
        if (data.IsDownload)
        {
            return;
        }

        var select = _last;
        string? group = WindowManager.AddGameWindow?.GetGroup();
        if (data.SourceType == SourceType.CurseForge)
        {
            Model.Progress(App.Lang("AddGameWindow.Tab1.Info8"));

            var res = await GameBinding.InstallCurseForge((data.Data as CurseForgeModObj.DataObj)!,
                (select!.Data as CurseForgeObjList.DataObj)!, group,
                ZipUpdate, GameRequest, GameOverwirte, UpdateProcess, PackState);
            Model.ProgressClose();

            if (!res.State)
            {
                Model.Show(App.Lang("AddGameWindow.Tab1.Error8"));
            }
            else
            {
                Done(res.Game!.UUID);
            }
        }
        else if (data.SourceType == SourceType.Modrinth)
        {
            Model.Progress(App.Lang("AddGameWindow.Tab1.Info8"));
            var res = await GameBinding.InstallModrinth((data.Data as ModrinthVersionObj)!,
                (select!.Data as ModrinthSearchObj.HitObj)!, group,
                ZipUpdate, GameRequest, GameOverwirte, UpdateProcess, PackState);
            Model.ProgressClose();

            if (!res.State)
            {
                Model.Show(App.Lang("AddGameWindow.Tab1.Error8"));
            }
            else
            {
                Done(res.Game!.UUID);
            }
        }
    }

    /// <summary>
    /// 添加完成
    /// </summary>
    private async void Done(string? uuid)
    {
        Model.Notify(App.Lang("AddGameWindow.Tab1.Info7"));

        Display = false;

        if (_keep)
        {
            return;
        }

        var model = WindowManager.MainWindow?.DataContext as MainModel;
        model?.Select(uuid);

        var res = await Model.ShowWait(App.Lang("AddGameWindow.Tab1.Info25"));
        if (res != true)
        {
            Dispatcher.UIThread.Post(WindowClose);
        }
        else
        {
            _keep = true;
        }
    }

    private async void LoadFail()
    {
        var res = await Model.ShowWait(App.Lang("AddModPackWindow.Error4"));
        if (res)
        {
            LoadSourceData();
            return;
        }

        if (Source < SourceList.Length)
        {
            res = await Model.ShowWait(App.Lang("AddModPackWindow.Info5"));
            if (res)
            {
                Source++;
            }
        }
    }

    private async void Load()
    {
        if (Source == 2 && Categorie == 4 && Text?.Length < 3)
        {
            Model.Show(App.Lang("AddModPackWindow.Error6"));
            return;
        }

        Model.Progress(App.Lang("AddModPackWindow.Info2"));
        var res = await WebBinding.GetModPackList((SourceType)Source,
            GameVersion, Text, Page ?? 0, Source == 2 ? Categorie : SortType,
            Source == 2 ? "" : Categorie < 0 ? "" : _categories[Categorie]);

        MaxPage = res.Count / 20;
        var page = 0;
        if (Source == 1)
        {
            page = Page ?? 0;
        }

        EnableNextPage = (MaxPage - Page) > 0;

        var data = res.List;

        if (data == null)
        {
            Model.Show(App.Lang("AddModPackWindow.Error2"));
            Model.ProgressClose();
            return;
        }

        DisplayList.Clear();

        int b = 0;
        for (int a = page * 50; a < data.Count; a++, b++)
        {
            if (b >= 50)
            {
                break;
            }
            var item = data[a];
            item.Add = this;
            DisplayList.Add(item);
        }

        OnPropertyChanged(nameof(DisplayList));

        _last = null;

        EmptyDisplay = DisplayList.Count == 0;

        Model.ProgressClose();
        Model.Notify(App.Lang("AddWindow.Info16"));
    }

    private async void Load1()
    {
        if (Display == false)
            return;

        FileList.Clear();
        Model.Progress(App.Lang("AddModPackWindow.Info3"));
        List<FileVersionItemModel>? list = null;
        var page = 0;
        if (Source == 0)
        {
            var res = await WebBinding.GetFileList((SourceType)Source,
                (_last!.Data as CurseForgeObjList.DataObj)!.Id.ToString(), Page1 ?? 0,
                GameVersion1, Loaders.Normal);
            list = res.List;
            MaxPage1 = res.Count / 50;
        }
        else if (Source == 1)
        {
            var res = await WebBinding.GetFileList((SourceType)Source,
                (_last!.Data as ModrinthSearchObj.HitObj)!.ProjectId, Page1 ?? 0,
                GameVersion1, Loaders.Normal);
            list = res.List;
            MaxPage1 = res.Count / 50;
            page = Page1 ?? 0;
        }

        EnableNextPage1 = (MaxPage1 - Page1) > 0;

        if (list == null)
        {
            Model.Show(App.Lang("AddModPackWindow.Error3"));
            Model.ProgressClose();
            return;
        }

        int b = 0;
        for (int a = page * 50; a < list.Count; a++, b++)
        {
            if (b >= 50)
            {
                break;
            }
            var item = list[a];
            item.Add = this;
            var games = GameBinding.GetGames();
            if (games.Any(item1 => item1.ModPack && item1.ModPackType == (SourceType)Source
            && item1.PID == item.ID && item1.FID == item.ID1))
            {
                item.IsDownload = true;
            }
            FileList.Add(item);
        }

        EmptyVersionDisplay = FileList.Count == 0;

        Model.ProgressClose();
        Model.Notify(App.Lang("AddWindow.Info16"));
    }

    public void Install(FileItemModel item)
    {
        SetSelect(item);
        Install();
    }

    private void ClearList()
    {
        foreach (var item in DisplayList)
        {
            item.Close();
        }
        DisplayList.Clear();
    }

    public void Back()
    {
        if (_load || Page <= 0)
        {
            return;
        }

        Page -= 1;
    }

    public void Next()
    {
        if (_load)
        {
            return;
        }

        Page += 1;
    }

    public void Reload1()
    {
        if (Display)
        {
            Load1();
        }
        else
        {
            Load();
        }
    }

    public override void Close()
    {
        if (Display)
        {
            Model.PopBack();
        }

        _load = true;
        Model.RemoveChoiseData(_useName);
        FileList.Clear();
        foreach (var item in DisplayList)
        {
            item.Close();
        }
        DisplayList.Clear();
        _last = null;
    }

    public void SetSelect(FileVersionItemModel item)
    {
        if (Item != null)
        {
            Item.IsSelect = false;
        }
        Item = item;
        item.IsSelect = true;
    }

    public void BackVersion()
    {
        if (_load || Page1 <= 0)
        {
            return;
        }

        Page1 -= 1;
    }

    public void NextVersion()
    {
        if (_load)
        {
            return;
        }

        Page1 += 1;
    }
}
