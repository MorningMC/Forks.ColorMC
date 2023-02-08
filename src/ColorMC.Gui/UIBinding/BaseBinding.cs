﻿using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using ColorMC.Core;
using ColorMC.Core.Game;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Net;
using ColorMC.Core.Net.Downloader;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;
using ColorMC.Core.Utils;
using ColorMC.Gui.Utils.LaunchSetting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Colors = ColorMC.Gui.Utils.LaunchSetting.Colors;

namespace ColorMC.Gui.UIBinding;

public static class BaseBinding
{
    public readonly static Dictionary<Process, GameSettingObj> Games = new();
    public static bool ISNewStart { get; private set; } = false;

    public static void Init()
    {
        CoreMain.OnError = ShowError;
        CoreMain.NewStart = ShowNew;
        CoreMain.DownloaderUpdate = DownloaderUpdate;
        CoreMain.ProcessLog = PLog;
        CoreMain.GameLog = PLog;
        CoreMain.LanguageReload = Change;

        GuiConfigUtils.Init(AppContext.BaseDirectory);
        FontSel.Load();
    }

    public static Task<IReadOnlyList<IStorageFile>> OpFile(Window window, string title, string ext, string name) 
    {
        return window.StorageProvider.OpenFilePickerAsync(new()
        {
            Title = title,
            AllowMultiple = false,
            FileTypeFilter = new List<FilePickerFileType>()
            {
                new(name)
                {
                     Patterns = new List<string>()
                     {
                        ext
                     }
                }
            }
        });
    }

    public static Task<string?> OpSave(Window window, string title, string ext, string name)
    {
        SaveFileDialog save = new()
        {
            Title = title,
            DefaultExtension = ext,
            InitialFileName = name
        };

        return save.ShowAsync(window);
    }

    public static void Exit()
    {
        Colors.Instance.Stop();
        Logs.Stop();
        DownloadManager.Stop();
    }

    public static async Task<bool> Launch(GameSettingObj obj, LoginObj obj1)
    {
        if (Games.ContainsValue(obj))
        {
            return false;
        }
        if (GuiConfigUtils.Config.ServerCustom.JoinServer &&
            !string.IsNullOrEmpty(GuiConfigUtils.Config.ServerCustom.IP))
        {
            var server = await ServerMotd.GetServerInfo(GuiConfigUtils.Config.ServerCustom.IP, GuiConfigUtils.Config.ServerCustom.Port);

            obj = obj.CopyObj();
            obj.StartServer.IP = server.ServerAddress;
            obj.StartServer.Port = server.ServerPort;
        }
        if (App.GameEditWindows.TryGetValue(obj, out var win))
        {
            win.ClearLog();
        }

        CoreMain.DownloaderUpdate = DownloaderUpdateOnThread;

        var res = await Task.Run(() =>
        {
            try
            {
                UserBinding.AddLockUser(obj1);
                return obj.StartGame(obj1).Result;
            }
            catch (Exception e)
            {
                UserBinding.RemoveLockUser(obj1);
                string temp = Localizer.Instance["Error6"];
                if (e is LaunchException launch && launch.Ex != null)
                {
                    Logs.Error(temp, launch.Ex);
                    CoreMain.OnError?.Invoke(temp, launch.Ex, false);
                }
                else
                {
                    Logs.Error(temp, e);
                    CoreMain.OnError?.Invoke(temp, e, false);
                }
                return null;
            }
        });
        Funtcions.RunGC();
        if (res != null)
        {
            res.Exited += (a, b) =>
            {
                UserBinding.RemoveLockUser(obj1);
                if (Games.Remove(res, out var obj2))
                {
                    App.MainWindow?.GameClose(obj2);
                }
                if (a is Process)
                {
                    var p = a as Process;
                    if (p?.ExitCode == 0)
                    {
                        return;
                    }
                    string file = obj.GetLogLatestFile();
                    if (!File.Exists(file))
                    {
                        return;
                    }

                    Dispatcher.UIThread.Post(() =>
                    {
                        App.ShowError(Localizer.Instance["UserBinding.Error2"], File.ReadAllText(file));
                    });
                }
                res.Dispose();
            };
            Games.Add(res, obj);
        }

        CoreMain.DownloaderUpdate = DownloaderUpdate;

        return res != null;
    }

    public static void DownloaderUpdateOnThread(CoreRunState state)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            App.DownloaderUpdate(state);
        }).Wait();
    }

    public static void DownloaderUpdate(CoreRunState state)
    {
        App.DownloaderUpdate(state);
    }

    public static void PLog(Process? p, string? d)
    {
        if (p == null)
            return;
        if (Games.TryGetValue(p, out var obj)
            && App.GameEditWindows.TryGetValue(obj, out var win))
        {
            win.Log(d);
        }
    }

    public static void PLog(GameSettingObj obj, string? d)
    {
        if (App.GameEditWindows.TryGetValue(obj, out var win))
        {
            win.Log(d);
        }
    }

    private static void ShowError(string data, Exception e, bool close)
    {
        App.ShowError(data, e, close);
    }

    private static void ShowNew()
    {
        ISNewStart = true;
    }

    private static void Change(LanguageType type)
    {
        App.LoadLanguage(type);
        Localizer.Instance.Reload();
    }

    public static (int, int) GetDownloadSize()
    {
        return (DownloadManager.AllSize, DownloadManager.DoneSize);
    }

    public static CoreRunState GetDownloadState()
    {
        return DownloadManager.State;
    }

    public static List<string> GetDownloadSources()
    {
        var list = new List<string>();
        Array values = Enum.GetValues(typeof(SourceLocal));
        foreach (SourceLocal value in values)
        {
            list.Add(value.GetName());
        }

        return list;
    }

    public static List<string> GetWindowTranTypes()
    {
        return new()
        {
            Localizer.Instance["OtherBinding.TranTypes.Item1"],
            Localizer.Instance["OtherBinding.TranTypes.Item2"],
            Localizer.Instance["OtherBinding.TranTypes.Item3"],
            Localizer.Instance["OtherBinding.TranTypes.Item4"],
            Localizer.Instance["OtherBinding.TranTypes.Item5"]
        };
    }

    public static List<string> GetLanguages()
    {
        var list = new List<string>();
        Array values = Enum.GetValues(typeof(LanguageType));
        foreach (LanguageType value in values)
        {
            list.Add(value.GetName());
        }

        return list;
    }

    public static void OpFile(string item, bool file)
    {
        switch (SystemInfo.Os)
        {
            case OsType.Windows:
                Process.Start("explorer",
                    $@"/select,{item}");
                break;
            case OsType.Linux:
                Process.Start("nautilus",
                    '"' + item + '"');
                break;
            case OsType.MacOS:
                if (file)
                {
                    var file1 = new FileInfo(item);
                    Process.Start("open", '"' + file1.Directory?.FullName + '"');
                }
                else
                {
                    Process.Start("open",
                        '"' + item + '"');
                }
                break;
        }
    }

    public static byte[] GetUIJson()
    {
        return App.GetFile("ColorMC.Gui.Resource.UI.CustomUI.json");
    }

    public static void OpUrl(string url)
    {
        //url = UrlEncoder.Default.Encode(url);
        url = url.Replace(" ", "%20");
        switch (SystemInfo.Os)
        {
            case OsType.Windows:
                var ps = Process.Start(new ProcessStartInfo()
                {
                    FileName = "cmd",
                    CreateNoWindow = true,
                    RedirectStandardInput = true,
                });
                ps.StandardInput.WriteLine($"start {url}");
                ps.Close();
                break;
            case OsType.Linux:
                Process.Start("xdg-open",
                    '"' + url + '"');
                break;
            case OsType.MacOS:
                Process.Start("open -a Safari",
                    '"' + url + '"');
                break;
        }
    }

    public static string GetPath(this IStorageFile file)
    {
        file.TryGetUri(out var uri);
        return uri!.LocalPath;
    }

    public static List<string> GetFontList()
    {
        return FontManager.Current.GetInstalledFontFamilyNames().ToList();
    }
}
