﻿using Avalonia.Platform.Storage;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using System.IO;
using System.Reflection;

namespace ColorMC.Gui.Utils;

public static class PathUtils
{
    /// <summary>
    /// 目录转字符串
    /// </summary>
    /// <param name="file">路径</param>
    /// <returns>路径字符串</returns>
    public static string? GetPath(this IStorageFolder file)
    {
        var path = file.Path.LocalPath;
        if (SystemInfo.Os == OsType.Android)
        {
            path = path.Replace("/document/primary:", "/storage/emulated/0/");
        }
        return path;
    }
    /// <summary>
    /// 文件转字符串
    /// </summary>
    /// <param name="file">文件</param>
    /// <returns>路径字符串</returns>
    public static string? GetPath(this IStorageFile file)
    {
        var path = file.Path.LocalPath;
        if (SystemInfo.Os == OsType.Android)
        {
            path = path.Replace("/document/primary:", "/storage/emulated/0/");
        }
        return path;
    }
    /// <summary>
    /// 文件转字符串
    /// </summary>
    /// <param name="file">文件</param>
    /// <returns>路径字符串</returns>
    public static string? GetPath(this IStorageItem file)
    {
        var path = file.Path.LocalPath;
        if (SystemInfo.Os == OsType.Android)
        {
            path = path.Replace("/document/primary:", "/storage/emulated/0/");
        }
        return path;
    }

    /// <summary>
    /// 从资源文件获取文件二进制
    /// </summary>
    /// <param name="name">文件名</param>
    /// <returns>数据</returns>
    public static byte[] GetFile(string name)
    {
        var assm = Assembly.GetExecutingAssembly();
        var item = assm.GetManifestResourceStream(name);
        using MemoryStream stream = new();
        item!.CopyTo(stream);
        return stream.ToArray();
    }
}
