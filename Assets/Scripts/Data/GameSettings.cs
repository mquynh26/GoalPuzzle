using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameSettings
{
    public static bool IsMusicEnabled { get; private set; } = true;   // nhạc nền
    public static bool IsSfxEnabled { get; private set; } = true;     // âm thanh hiệu ứng
    public static bool IsFullScreen { get; private set; } = true;

    // Còn bất kỳ âm thanh nào đang bật
    public static bool IsSoundEnabled => IsMusicEnabled || IsSfxEnabled;

    public static event Action Changed;

    // Chạy một lần khi game khởi động, đảm bảo về mặc định kể cả khi Editor tắt Domain Reload
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        IsMusicEnabled = true;
        IsSfxEnabled = true;
        IsFullScreen = Screen.fullScreen;   // lấy theo trạng thái cửa sổ thật lúc mở game
        Changed = null;
    }

    public static void SetMusicEnabled(bool on)
    {
        IsMusicEnabled = on;
        Changed?.Invoke();
    }

    public static void SetSfxEnabled(bool on)
    {
        IsSfxEnabled = on;
        Changed?.Invoke();
    }

    // Bật / tắt cả nhạc lẫn hiệu ứng
    public static void SetSoundEnabled(bool on)
    {
        IsMusicEnabled = on;
        IsSfxEnabled = on;
        Changed?.Invoke();
    }

    // Phím M: bật hết -> tắt nhạc (còn hiệu ứng) -> tắt hết -> bật hết
    public static void CycleSound()
    {
        if (IsMusicEnabled && IsSfxEnabled) SetMusicEnabled(false);
        else if (!IsMusicEnabled && !IsSfxEnabled) SetSoundEnabled(true);
        else SetSoundEnabled(false);
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