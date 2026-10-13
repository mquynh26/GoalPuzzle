using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameSettings
{
    public static bool IsSoundEnabled { get; private set; } = true;
    public static bool IsFullScreen { get; private set; } = true;
    
    public static event Action Changed;

    // Chạy một lần khi game khởi động, đảm bảo về mặc định kể cả khi Editor tắt Domain Reload
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        IsSoundEnabled = true;
        IsFullScreen = Screen.fullScreen;   // lấy theo trạng thái cửa sổ thật lúc mở game
        Changed = null;
    }

    public static void SetSoundEnabled(bool on)
    {
        IsSoundEnabled = on;
        Changed?.Invoke();
    }

    public static void SetFullScreen(bool on)
    {
        IsFullScreen = on;
        Screen.fullScreen = on;
        Changed?.Invoke();
    }

    public static void ToggleSound() => SetSoundEnabled(!IsSoundEnabled);

    public static void ToggleFullScreen() => SetFullScreen(!IsFullScreen);
}
