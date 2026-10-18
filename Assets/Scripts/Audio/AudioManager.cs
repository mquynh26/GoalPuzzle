using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Các loại âm thanh hiệu ứng trong game
public enum Sfx
{
    Move,
    NPC,        // NPC bị đá xoay hướng
    Hit,        // bóng va vào bóng khác
    Cone,       // bóng đập cọc
    Fence,       // fence
    Out,        // bóng bay ra ngoài
    Win,        // thắng màn
    MenuMove,   // di chuyển khung chọn màn
    Undo
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Nguồn phát")]
    public AudioSource musicSource;   // loop, dùng cho nhạc nền
    public AudioSource sfxSource;     // phát hiệu ứng bằng PlayOneShot

    [Header("Nhạc nền")]
    public AudioClip music;
    [Range(0f, 1f)] public float musicVolume = 0.5f;

    [Header("Hiệu ứng")]
    [Range(0f, 1f)] public float sfxVolume = 1f;
    public AudioClip move;
    public AudioClip npc;
    public AudioClip hit;
    public AudioClip cone;
    public AudioClip fence;
    public AudioClip outOfBounds;
    public AudioClip win;
    public AudioClip menuMove;
    public AudioClip undo;

    void Awake()
    {
        Instance = this;
    }

    void OnEnable()
    {
        GameSettings.Changed += Apply;
    }

    void OnDisable()
    {
        GameSettings.Changed -= Apply;
    }

    void Start()
    {
        if (musicSource != null && music != null)
        {
            musicSource.clip = music;
            musicSource.loop = true;
            musicSource.volume = musicVolume;
            musicSource.playOnAwake = false;
            musicSource.Play();
        }
        Apply();
    }

    // Tắt nhạc bằng mute để nhạc giữ nguyên vị trí, bật lại thì tiếp tục chứ không phát lại từ đầu
    void Apply()
    {
        if (musicSource != null) musicSource.mute = !GameSettings.IsMusicEnabled;
    }

    public static void Play(Sfx id)
    {
        if (Instance != null) Instance.PlaySfx(id);
    }

    void PlaySfx(Sfx id)
    {
        if (!GameSettings.IsSfxEnabled || sfxSource == null) return;

        AudioClip clip = GetClip(id);
        if (clip != null) sfxSource.PlayOneShot(clip, sfxVolume);
    }

    AudioClip GetClip(Sfx id)
    {
        switch (id)
        {
            case Sfx.Move:      return move;
            case Sfx.NPC:     return npc;
            case Sfx.Hit:        return hit;
            case Sfx.Fence :      return fence;
            case Sfx.Cone:       return cone;
            case Sfx.Out:        return outOfBounds;
            case Sfx.Win:        return win;
            case Sfx.MenuMove:   return menuMove;
            case Sfx.Undo:       return undo;
        }
        return null;
    }
}