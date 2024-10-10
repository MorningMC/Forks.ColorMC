using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Input;
using Avalonia.Threading;
using ColorMC.Core.Objs;
using ColorMC.Gui.Joystick;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Event = Silk.NET.SDL.Event;
using EventType = Silk.NET.SDL.EventType;
using GameControllerAxis = Silk.NET.SDL.GameControllerAxis;

namespace ColorMC.Gui.UI.Model.Setting;

public partial class SettingModel
{
    public ObservableCollection<string> Configs { get; init; } = [];
    public ObservableCollection<InputButtonModel> InputList { get; init; } = [];
    public ObservableCollection<string> InputNames { get; init; } = [];

    public ObservableCollection<InputAxisButtonModel> InputAxisList { get; init; } = [];

    public string[] AxisType { get; init; } = LanguageBinding.GetAxisTypeName();

    [ObservableProperty]
    private InputButtonModel _inputItem;

    [ObservableProperty]
    private InputAxisButtonModel _inputAxisItem;

    [ObservableProperty]
    private bool _inputInit;
    [ObservableProperty]
    private bool _inputExist;
    [ObservableProperty]
    private bool _inputEnable;
    [ObservableProperty]
    private bool _itemCycle;

    [ObservableProperty]
    private int _inputNum;
    [ObservableProperty]
    private int _inputIndex = -1;
    [ObservableProperty]
    private int _inputRotateAxis = 0;
    [ObservableProperty]
    private int _cursorDeath;
    [ObservableProperty]
    private int _rotateDeath;
    [ObservableProperty]
    private int _inputCursorAxis = 0;
    [ObservableProperty]
    private int _toBackValue;

    [ObservableProperty]
    private int _nowConfig = -1;
    [ObservableProperty]
    private int _selectConfig = -1;

    [ObservableProperty]
    private int _nowAxis1;
    [ObservableProperty]
    private int _nowAxis2;

    [ObservableProperty]
    private byte _itemCycleLeft;
    [ObservableProperty]
    private byte _itemCycleRight;

    [ObservableProperty]
    private string _cycleLeftIcon;
    [ObservableProperty]
    private string _cycleRightIcon;

    [ObservableProperty]
    private float _rotateRate;
    [ObservableProperty]
    private float _cursorRate;
    [ObservableProperty]
    private float _downRate;

    private readonly List<string> _controlUUIDs = [];
    private short _leftX, _leftY, _rightX, _rightY;

    private Action<byte>? _input;
    private Action<byte, bool>? _inputAxis;
    private Action<InputKeyObj>? _inputKey;

    private IntPtr _controlPtr;
    private int _joystickID;

    private InputControlObj? _controlObj;

    private bool _isInputConfigLoad;
    private bool _isInputLoad;

    partial void OnNowConfigChanged(int value)
    {
        if (_isInputLoad)
        {
            return;
        }
        else if (value == -1)
        {
            ConfigBinding.SaveNowInputConfig(null);
            return;
        }
        else if (_controlUUIDs.Count <= value)
        {
            NowConfig = -1;
            return;
        }

        var uuid = _controlUUIDs[value];

        ConfigBinding.SaveNowInputConfig(uuid);
    }

    partial void OnSelectConfigChanged(int value)
    {
        InputExist = value != -1;
        if (_isInputLoad || value == -1)
        {
            return;
        }
        else if (_controlUUIDs.Count <= value)
        {
            SelectConfig = -1;
            return;
        }

        string uuid = _controlUUIDs[value];

        JoystickConfig.Configs.TryGetValue(uuid, out _controlObj);
        if (_controlObj != null)
        {
            LoadInputConfig(_controlObj);
        }
    }

    partial void OnRotateDeathChanged(int value)
    {
        if (_isInputConfigLoad || _controlObj == null)
        {
            return;
        }

        ConfigBinding.SetInputDeath(_controlObj, RotateDeath, CursorDeath);
    }

    partial void OnCursorDeathChanged(int value)
    {
        if (_isInputConfigLoad || _controlObj == null)
        {
            return;
        }

        ConfigBinding.SetInputDeath(_controlObj, RotateDeath, CursorDeath);
    }

    partial void OnDownRateChanged(float value)
    {
        if (_isInputConfigLoad || _controlObj == null)
        {
            return;
        }

        ConfigBinding.SetInputRate(_controlObj, RotateRate, CursorRate, DownRate);
    }

    partial void OnCursorRateChanged(float value)
    {
        if (_isInputConfigLoad || _controlObj == null)
        {
            return;
        }

        ConfigBinding.SetInputRate(_controlObj, RotateRate, CursorRate, DownRate);
    }

    partial void OnRotateRateChanged(float value)
    {
        if (_isInputConfigLoad || _controlObj == null)
        {
            return;
        }

        ConfigBinding.SetInputRate(_controlObj, RotateRate, CursorRate, DownRate);
    }

    partial void OnItemCycleChanged(bool value)
    {
        if (_isInputConfigLoad || _controlObj == null)
        {
            return;
        }

        ConfigBinding.SaveInput(_controlObj, ItemCycle);
    }

    partial void OnItemCycleLeftChanged(byte value)
    {
        CycleLeftIcon = IconConverter.GetInputKeyIcon(ItemCycleLeft);

        if (_isInputConfigLoad || _controlObj == null)
        {
            return;
        }

        ConfigBinding.SetItemCycle(_controlObj, ItemCycleLeft, ItemCycleRight);
    }

    partial void OnItemCycleRightChanged(byte value)
    {
        CycleRightIcon = IconConverter.GetInputKeyIcon(ItemCycleRight);

        if (_isInputConfigLoad || _controlObj == null)
        {
            return;
        }

        ConfigBinding.SetItemCycle(_controlObj, ItemCycleLeft, ItemCycleRight);
    }

    partial void OnInputCursorAxisChanged(int value)
    {
        if (_isInputConfigLoad || _controlObj == null)
        {
            return;
        }

        ConfigBinding.SetInputAxis(_controlObj, InputRotateAxis, InputCursorAxis);
    }

    partial void OnInputRotateAxisChanged(int value)
    {
        if (_isInputConfigLoad || _controlObj == null)
        {
            return;
        }

        ConfigBinding.SetInputAxis(_controlObj, InputRotateAxis, InputCursorAxis);
    }

    partial void OnInputIndexChanged(int value)
    {
        InputClose();
        if (value != -1)
        {
            unsafe
            {
                _controlPtr = new(JoystickInput.Open(InputIndex));
            }
            if (_controlPtr == IntPtr.Zero)
            {
                Model.Show(App.Lang("SettingWindow.Tab8.Error1"));
            }
            else
            {
                _joystickID = JoystickInput.GetJoystickID(_controlPtr);
            }
        }
    }

    partial void OnInputEnableChanged(bool value)
    {
        if (_isInputConfigLoad)
        {
            return;
        }

        ConfigBinding.SaveInputInfo(InputEnable);
    }

    [RelayCommand]
    public async Task ExportInputConfig()
    {
        if (_controlObj == null)
        {
            return;
        }
        var top = Model.GetTopLevel();
        if (top == null)
        {
            return;
        }
        var res = await PathBinding.SaveFile(top, FileType.InputConfig, [_controlObj.Name, _controlObj]);
        if (res == null)
        {
            return;
        }
        else if (res != true)
        {
            Model.Show(App.Lang("SettingWindow.Tab8.Error4"));
            return;
        }

        Model.Notify(App.Lang("SettingWindow.Tab8.Info14"));
    }

    [RelayCommand]
    public async Task ImportInputConfig()
    {
        var top = Model.GetTopLevel();
        if (top == null)
        {
            return;
        }
        var file = await PathBinding.SelectFile(top, FileType.InputConfig);
        if (file.Item1 == null)
        {
            return;
        }

        var obj = JoystickConfig.Load(file.Item1);
        if (obj == null)
        {
            Model.Show(App.Lang("SettingWindow.Tab8.Error2"));
            return;
        }

        obj.UUID = Guid.NewGuid().ToString().ToLower();

        ConfigBinding.SaveInputConfig(obj);
        Configs.Add(obj.Name);
        _controlUUIDs.Add(obj.UUID);

        Model.Notify(App.Lang("SettingWindow.Tab8.Info15"));
    }

    [RelayCommand]
    public async Task DeleteInputConfig()
    {
        if (_controlObj == null)
        {
            return;
        }

        var res = await Model.ShowWait(string.Format(App.Lang("SettingWindow.Tab8.Info1"), _controlObj.Name));
        if (!res)
        {
            return;
        }

        ConfigBinding.RemoveInputConfig(_controlObj);

        _controlUUIDs.Remove(_controlObj.UUID);
        Configs.Remove(_controlObj.Name);

        if (Configs.Count > 0)
        {
            SelectConfig = 0;
        }
    }

    [RelayCommand]
    public async Task RenameInputConfig()
    {
        if (_controlObj == null)
        {
            return;
        }

        var (Cancel, Text1) = await Model.ShowEdit(App.Lang("SettingWindow.Tab8.Info2"), _controlObj.Name);
        if (Cancel || string.IsNullOrWhiteSpace(Text1))
        {
            return;
        }

        _controlObj.Name = Text1;
        var last = SelectConfig;
        var now = NowConfig == last;
        Configs[SelectConfig] = Text1;
        SelectConfig = last;
        if (now)
        {
            NowConfig = last;
        }

        ConfigBinding.SaveInputConfig(_controlObj);
    }

    [RelayCommand]
    public async Task NewInputConfig()
    {
        var (Cancel, Text) = await Model.ShowInputOne(App.Lang("SettingWindow.Tab8.Info2"), false);
        if (Cancel || string.IsNullOrWhiteSpace(Text))
        {
            return;
        }

        var obj = ConfigBinding.NewInput(Text);
        _controlUUIDs.Add(obj.UUID);
        Configs.Add(obj.Name);

        SelectConfig = Configs.Count - 1;
    }

    [RelayCommand]
    public async Task AddAxisInput()
    {
        if (_controlObj == null)
        {
            return;
        }
        using var cannel = new CancellationTokenSource();
        Model.ShowCancel(App.Lang("SettingWindow.Tab8.Info3"), () =>
        {
            cannel.Cancel();
        });
        var key = await WaitAxis(cannel.Token);
        Model.ShowClose();
        if (key == null)
        {
            return;
        }
        var key1 = ((byte, bool))key;
        Model.ShowCancel(App.Lang("SettingWindow.Tab8.Info4"), () =>
        {
            cannel.Cancel();
        });
        var key2 = await WaitKey(cannel.Token);
        Model.ShowClose();
        if (key2 == null)
        {
            return;
        }
        var item1 = new InputAxisButtonModel(this)
        {
            UUID = Guid.NewGuid().ToString().ToLower()[..8],
            InputKey = key1.Item1,
            Obj = key2,
            Start = key1.Item2 ? (short)2000 : (short)-2000,
            End = key1.Item2 ? short.MaxValue : short.MinValue
        };
        InputAxisList.Add(item1);
        ConfigBinding.AddAxisInput(_controlObj, item1.UUID, item1.GenObj());
        Model.Notify(App.Lang("SettingWindow.Tab8.Info5"));
    }

    [RelayCommand]
    public async Task AddInput()
    {
        if (_controlObj == null)
        {
            return;
        }

        using var cannel = new CancellationTokenSource();
        Model.ShowCancel(App.Lang("SettingWindow.Tab8.Info6"), () =>
        {
            cannel.Cancel();
        });
        var key = await WaitInput(cannel.Token);
        Model.ShowClose();
        if (key == null)
        {
            return;
        }
        var key1 = (byte)key;
        Model.ShowCancel(App.Lang("SettingWindow.Tab8.Info4"), () =>
        {
            cannel.Cancel();
        });
        var key2 = await WaitKey(cannel.Token);
        Model.ShowClose();
        if (key2 == null)
        {
            return;
        }
        foreach (var item in InputList)
        {
            if (item.InputKey == key1)
            {
                InputList.Remove(item);
                break;
            }
        }
        var item1 = new InputButtonModel(this)
        {
            InputKey = key1,
            Obj = key2
        };
        InputList.Add(item1);
        ConfigBinding.AddInput(_controlObj, item1.InputKey, item1.Obj);
        Model.Notify(App.Lang("SettingWindow.Tab8.Info7"));
    }

    [RelayCommand]
    public async Task SetItemButton(object? right)
    {
        using var cannel = new CancellationTokenSource();
        Model.ShowCancel(App.Lang("SettingWindow.Tab8.Info6"), () =>
        {
            cannel.Cancel();
        });
        var key = await WaitInput(cannel.Token);
        Model.ShowClose();
        if (key == null)
        {
            return;
        }
        var key1 = (byte)key;

        if (right is bool value && value)
        {
            ItemCycleRight = key1;
        }
        else
        {
            ItemCycleLeft = key1;
        }
    }

    public void InputSave(InputAxisButtonModel model)
    {
        if (_controlObj == null)
        {
            return;
        }

        ConfigBinding.AddAxisInput(_controlObj, model.UUID, model.GenObj());
    }

    private void StartRead()
    {
        if (SdlUtils.SdlInit)
        {
            JoystickInput.OnEvent += InputControl_OnEvent;
        }
    }

    private void UpdateType1()
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (InputCursorAxis == 0)
            {
                NowAxis1 = Math.Max(_leftX, _leftY);
            }
            else
            {
                NowAxis1 = Math.Max(_rightX, _rightY);
            }
        });
    }

    private void UpdateType2()
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (InputRotateAxis == 0)
            {
                NowAxis2 = Math.Max(_leftX, _leftY);
            }
            else
            {
                NowAxis2 = Math.Max(_rightX, _rightY);
            }
        });
    }

    public void LoadInput()
    {
        _isInputLoad = true;
        Configs.Clear();
        _controlUUIDs.Clear();
        InputEnable = GuiConfigUtils.Config.Input.Enable;

        foreach (var item in JoystickConfig.Configs)
        {
            _controlUUIDs.Add(item.Key);
            Configs.Add(item.Value.Name);
        }

        if (GuiConfigUtils.Config.Input.NowConfig != null)
        {
            NowConfig = _controlUUIDs.IndexOf(GuiConfigUtils.Config.Input.NowConfig);
        }

        _isInputLoad = false;

        if (Configs.Count > 0)
        {
            SelectConfig = 0;
        }
    }

    private void LoadInputConfig(InputControlObj config)
    {
        _isInputConfigLoad = true;
        InputList.Clear();
        InputAxisList.Clear();

        foreach (var item in config.Keys)
        {
            InputList.Add(new(this)
            {
                InputKey = item.Key,
                Obj = item.Value
            });
        }

        foreach (var item in config.AxisKeys)
        {
            InputAxisList.Add(new(this)
            {
                InputKey = item.Value.InputKey,
                UUID = item.Key,
                Obj = item.Value,
                Start = item.Value.Start,
                End = item.Value.End
            });
        }

        InputCursorAxis = config.CursorAxis;
        InputRotateAxis = config.RotateAxis;

        CursorDeath = config.CursorDeath;
        RotateDeath = config.RotateDeath;

        CursorRate = config.CursorRate;
        RotateRate = config.RotateRate;
        DownRate = config.DownRate;
        ToBackValue = config.ToBackValue;

        ItemCycle = config.ItemCycle;
        ItemCycleLeft = config.ItemCycleLeft;
        ItemCycleRight = config.ItemCycleRight;

        _isInputConfigLoad = false;
    }

    public void ReloadInput()
    {
        if (!InputInit)
        {
            return;
        }
        InputNum = JoystickInput.Count;

        InputNames.Clear();
        JoystickInput.GetNames().ForEach(InputNames.Add);
        if (InputNames.Count != 0)
        {
            InputIndex = 0;
        }
        else
        {
            InputIndex = -1;
        }
    }

    public void SetTab8Click()
    {
        Model.SetChoiseCall(_name, ReloadInput);
        Model.SetChoiseContent(_name, App.Lang("SettingWindow.Tab8.Info8"));
    }

    public async void SetKeyButton(InputButtonModel item)
    {
        if (_controlObj == null)
        {
            return;
        }
        using var cannel = new CancellationTokenSource();
        Model.ShowCancel(App.Lang("SettingWindow.Tab8.Info4"), () =>
        {
            cannel.Cancel();
        });
        var key2 = await WaitKey(cannel.Token);
        Model.ShowClose();
        if (key2 == null)
        {
            return;
        }
        item.Obj = key2;
        item.Update();

        if (item is InputAxisButtonModel model)
        {
            ConfigBinding.AddAxisInput(_controlObj, model.UUID, model.GenObj());
        }
        else
        {
            ConfigBinding.AddInput(_controlObj, item.InputKey, item.Obj);
        }
        Model.Notify(App.Lang("SettingWindow.Tab8.Info9"));
    }

    public void DeleteInput(InputButtonModel item)
    {
        if (_controlObj == null)
        {
            return;
        }
        if (item is InputAxisButtonModel model)
        {
            InputAxisList.Remove(model);
            ConfigBinding.DeleteAxisInput(_controlObj, model.UUID);
        }
        else
        {
            InputList.Remove(item);
            ConfigBinding.DeleteInput(_controlObj, item.InputKey);
        }
        Model.Notify(App.Lang("SettingWindow.Tab8.Info10"));
    }

    public void InputMouse(KeyModifiers modifiers, PointerPointProperties properties)
    {
        if (_inputKey == null)
        {
            return;
        }

        if (properties.IsMiddleButtonPressed)
        {
            _inputKey.Invoke(new()
            {
                MouseButton = MouseButton.Middle,
                KeyModifiers = modifiers
            });
        }
        else if (properties.IsRightButtonPressed)
        {
            _inputKey.Invoke(new()
            {
                MouseButton = MouseButton.Right,
                KeyModifiers = modifiers
            });
        }
        else if (properties.IsLeftButtonPressed)
        {
            _inputKey.Invoke(new()
            {
                MouseButton = MouseButton.Left,
                KeyModifiers = modifiers
            });
        }
        else if (properties.IsXButton1Pressed)
        {
            _inputKey.Invoke(new()
            {
                MouseButton = MouseButton.XButton1,
                KeyModifiers = modifiers
            });
        }
        else if (properties.IsXButton2Pressed)
        {
            _inputKey.Invoke(new()
            {
                MouseButton = MouseButton.XButton2,
                KeyModifiers = modifiers
            });
        }
    }

    public bool InputKey(KeyModifiers modifiers, Key key)
    {
        if (_inputKey == null)
        {
            return false;
        }

        if (key is Key.LeftShift or Key.RightShift && modifiers == KeyModifiers.Shift)
        {
            modifiers = KeyModifiers.None;
        }
        else if (key is Key.LeftCtrl or Key.RightCtrl && modifiers == KeyModifiers.Control)
        {
            modifiers = KeyModifiers.None;
        }
        else if (key is Key.LeftAlt or Key.RightAlt && modifiers == KeyModifiers.Alt)
        {
            modifiers = KeyModifiers.None;
        }

        _inputKey?.Invoke(new()
        {
            Key = key,
            KeyModifiers = modifiers
        });

        return true;
    }

    private Task<InputKeyObj?> WaitKey(CancellationToken token)
    {
        JoystickInput.IsEditMode = true;
        InputKeyObj? keys = null;
        bool output = false;
        _inputKey = (key) =>
        {
            _inputKey = null;
            keys = key;
            output = true;
        };
        return Task.Run(() =>
        {
            while (!output)
            {
                if (token.IsCancellationRequested)
                {
                    return null;
                }
                System.Threading.Thread.Sleep(100);

                JoystickInput.IsEditMode = false;
            }

            return keys;
        });
    }

    private Task<byte?> WaitInput(CancellationToken token)
    {
        byte? keys = null;
        bool output = false;
        _input = (key) =>
        {
            _input = null;
            keys = key;
            output = true;
        };
        return Task.Run(() =>
        {
            while (!output)
            {
                if (token.IsCancellationRequested)
                {
                    return null;
                }
                System.Threading.Thread.Sleep(100);
            }

            return keys;
        });
    }

    public Task<(byte, bool)?> WaitAxis(CancellationToken token)
    {
        byte keys = 0;
        bool output = false;
        bool positives = false;
        _inputAxis = (key, positive) =>
        {
            _inputAxis = null;
            positives = positive;
            keys = key;
            output = true;
        };
        return Task.Run<(byte, bool)?>(() =>
        {
            while (!output)
            {
                if (token.IsCancellationRequested)
                {
                    return null;
                }
                System.Threading.Thread.Sleep(100);
            }

            return (keys, positives);
        });
    }

    private void InputControl_OnEvent(Event sdlEvent)
    {
        EventType type = (EventType)sdlEvent.Type;
        if (type is EventType.Controllerdeviceadded
            or EventType.Controllerdeviceremoved)
        {
            ReloadInput();
            return;
        }

        if (sdlEvent.Cbutton.Which != _joystickID)
        {
            return;
        }

        if (type == EventType.Controlleraxismotion)
        {
            var axisEvent = sdlEvent.Caxis;
            var axisValue = axisEvent.Value;

            short axisFixValue;
            if (axisValue == short.MinValue)
            {
                axisFixValue = short.MaxValue;
            }
            else
            {
                axisFixValue = Math.Abs(axisValue);
            }

            if (axisEvent.Axis == (uint)GameControllerAxis.Leftx)
            {
                _leftX = axisFixValue;
                UpdateType1();
            }
            else if (axisEvent.Axis == (uint)GameControllerAxis.Lefty)
            {
                _leftY = axisFixValue;
                UpdateType1();
            }
            else if (axisEvent.Axis == (uint)GameControllerAxis.Rightx)
            {
                _rightX = axisFixValue;
                UpdateType2();
            }
            else if (axisEvent.Axis == (uint)GameControllerAxis.Righty)
            {
                _rightY = axisFixValue;
                UpdateType2();
            }

            if (axisFixValue > 2000)
            {
                _inputAxis?.Invoke(sdlEvent.Caxis.Axis, axisValue > 0);
            }

            Dispatcher.UIThread.Post(() =>
            {
                foreach (var item in InputAxisList)
                {
                    if (item.InputKey == axisEvent.Axis)
                    {
                        item.NowValue = axisValue;
                    }
                }
            });
        }
        else if (type == EventType.Controllerbuttondown)
        {
            _input?.Invoke(sdlEvent.Cbutton.Button);
        }
    }

    private void InputClose()
    {
        _joystickID = 0;
        if (_controlPtr != IntPtr.Zero)
        {
            JoystickInput.Close(_controlPtr);
            _controlPtr = IntPtr.Zero;
        }
    }

    private void StopRead()
    {
        if (SdlUtils.SdlInit)
        {
            JoystickInput.OnEvent -= InputControl_OnEvent;
        }
    }
}
