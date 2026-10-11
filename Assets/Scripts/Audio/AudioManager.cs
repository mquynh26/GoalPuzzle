using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Sfx
{
    Move,        // cầu thủ bước
    Kick,        // sút bóng
    Rotate,      // xoay NPC
    ConeBreak,   // bóng đập cọc
    Win,         // thắng màn
    Undo,        // hoàn tác
    Click        // bấm nút UI
}

/// <summary>
/// Phát hiệu ứng âm thanh. Tôn trọng GameSettings.IsSoundEnabled (tắt âm thì không phát gì).
/// Gắn lên GameSystems/AudioManager (script tự thêm AudioSource).
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Clip (để trống = không phát)")]
    public AudioClip move;
    public AudioClip kick;
    public AudioClip rotate;
    public AudioClip coneBreak;
    public AudioClip win;
    public AudioClip undo;
    public AudioClip click;

    [Header("Âm lượng")]
    [Range(0f, 1f)] public float volume = 1f;
    [Tooltip("Dao động nhẹ cao độ cho đỡ nhàm tai (0 = tắt)")]
    [Range(0f, 0.3f)] public float pitchVariation = 0.05f;

    AudioSource source;

    void Awake()
    {
        Instance = this;
        source = GetComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = false;
    }

    void OnEnable()  { GameSettings.Changed += OnSettingsChanged; }
    void OnDisable() { GameSettings.Changed -= OnSettingsChanged; }

    // Tắt âm thì cắt luôn tiếng đang phát
    void OnSettingsChanged()
    {
        if (!GameSettings.IsSoundEnabled) source.Stop();
    }

    public void Play(Sfx sfx)
    {
        if (!GameSettings.IsSoundEnabled) return;

        AudioClip clip = GetClip(sfx);
        if (clip == null) return;

        source.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
        source.PlayOneShot(clip, volume);
    }

    AudioClip GetClip(Sfx sfx)
    {
        switch (sfx)
        {
            case Sfx.Move:      return move;
            case Sfx.Kick:      return kick;
            case Sfx.Rotate:    return rotate;
            case Sfx.ConeBreak: return coneBreak;
            case Sfx.Win:       return win;
            case Sfx.Undo:      return undo;
            case Sfx.Click:     return click;
            default:            return null;
        }
    }
}
