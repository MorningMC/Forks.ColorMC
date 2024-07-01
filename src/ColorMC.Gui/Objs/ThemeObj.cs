﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Media;

namespace ColorMC.Gui.Objs;

public record ThemeObj
{
    /// <summary>
    /// 主要颜色
    /// </summary>
    public IBrush MainColor;
    /// <summary>
    /// 字体颜色
    /// </summary>
    public IBrush FontColor;
    public IBrush WindowBG;
    /// <summary>
    /// 用于开启模糊模式时窗口的颜色
    /// </summary>
    public IBrush WindowTranColor;
    public IBrush ProgressBarBG;
    public IBrush MainGroupBG;
    public IBrush MainGroupBorder;
    public IBrush ItemBG;
    public IBrush GameItemBG;
    public IBrush TopViewBG;
    public IBrush AllBorder;
    /// <summary>
    /// 按钮边框背景色
    /// </summary>
    public IBrush ButtonBorder;
    /// <summary>
    /// 浮动弹出框背景色
    /// </summary>
    public IBrush TopBGColor;
    /// <summary>
    /// 拖拽文件显示框背景色
    /// </summary>
    public IBrush TopGridColor;
    /// <summary>
    /// 覆盖层背景色
    /// </summary>
    public IBrush OverBGColor;
    /// <summary>
    /// 覆盖层边框颜色
    /// </summary>
    public IBrush OverBrushColor;
}
