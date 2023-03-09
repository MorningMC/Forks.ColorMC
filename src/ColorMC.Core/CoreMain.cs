using ColorMC.Core.Game;
using ColorMC.Core.Game.Auth;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Net.Downloader;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;
using ColorMC.Core.Utils;
using System.Diagnostics;

namespace ColorMC.Core;

public enum CoreRunState
{
    Read, Init, GetInfo, Start, End,
    Download,
    Error,
}

public static class ColorMCCore
{
    public const string Version = "A15.230306";

    /// <summary>
    /// 错误显示回调
    /// 标题 错误 关闭程序
    /// </summary>
    public static Action<string, Exception, bool>? OnError { internal get; set; }
    /// <summary>
    /// 下载线程相应回调
    /// </summary>
    public static Action<CoreRunState>? DownloaderUpdate { internal get; set; }
    /// <summary>
    /// 下载项目更新回调
    /// </summary>
    public static Action<int, DownloadItem>? DownloadItemStateUpdate { internal get; set; }
    /// <summary>
    /// 下载项目错误回调
    /// </summary>
    public static Action<int, DownloadItem, Exception>? DownloadItemError { internal get; set; }

    /// <summary>
    /// 游戏实例覆盖回调
    /// </summary>
    public static Func<GameSettingObj, Task<bool>>? GameOverwirte { internal get; set; }
    /// <summary>
    /// 是否下载游戏回调
    /// </summary>
    public static Func<LaunchState, GameSettingObj, Task<bool>>? GameDownload { internal get; set; }
    /// <summary>
    /// 游戏启动回调
    /// </summary>
    public static Action<GameSettingObj, LaunchState>? GameLaunch { internal get; set; }

    /// <summary>
    /// 压缩包处理回调
    /// </summary>
    public static Action<CoreRunState>? PackState { internal get; set; }
    /// <summary>
    /// 压缩包更新回调
    /// </summary>
    public static Action<int, int>? PackUpdate { internal get; set; }

    /// <summary>
    /// 游戏进程日志回调
    /// </summary>
    public static Action<Process?, string?>? ProcessLog { internal get; set; }
    /// <summary>
    /// 游戏日志回调
    /// </summary>
    public static Action<GameSettingObj, string?>? GameLog { internal get; set; }

    /// <summary>
    /// 登录状态回调
    /// </summary>
    public static Action<AuthState>? AuthStateUpdate { internal get; set; }
    /// <summary>
    /// 登录码回调
    /// </summary>
    public static Action<string, string>? LoginOAuthCode { internal get; set; }

    /// <summary>
    /// 语言更新回调
    /// </summary>
    public static Action<LanguageType>? LanguageReload { internal get; set; }
    /// <summary>
    /// 登录失败是否以离线方式启动
    /// </summary>
    public static Func<LoginObj, Task<bool>>? LoginFailLaunch { internal get; set; }
    /// <summary>
    /// 解压Java时
    /// </summary>
    public static Action? JavaUnzip { internal get; set; }
    /// <summary>
    /// 没有Java时
    /// </summary>
    public static Action? NoJava { internal get; set; }

    /// <summary>
    /// 新运行
    /// </summary>
    public static bool NewStart { get; internal set; }

    /// <summary>
    /// 停止事件
    /// </summary>
    internal static event Action? Stop;

    public static string BaseDir { get; private set; }

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="dir">运行的路径</param>
    public static void Init(string dir)
    {
        BaseDir = dir;
        Directory.CreateDirectory(dir);
        LanguageHelper.Load(LanguageType.zh_cn);
        Logs.Init(dir);
        ConfigSave.Init();
        ConfigUtils.Init(dir);
        LocalMaven.Init(dir);
        DownloadManager.Init(dir);
        JvmPath.Init(dir);
        AuthDatabase.Init(dir);
        MCPath.Init(dir);

        Logs.Info(LanguageHelper.GetName("Core.Info1"));
    }

    /// <summary>
    /// 执行关闭操作
    /// </summary>
    public static void Close()
    {
        Stop?.Invoke();
    }
}