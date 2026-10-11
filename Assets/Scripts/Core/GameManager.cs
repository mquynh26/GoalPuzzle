using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Tooltip("Trạng thái lúc mới vào game. Để Playing khi đang test, đổi sang Menu khi đã có giao diện menu.")]
    public GamePhase startPhase = GamePhase.Menu;
    [Tooltip("Thời gian chờ (giây) sau khi thắng rồi tự sang màn mới.")]
    public float nextLevelDelay = 1.5f;
    
    public GamePhase State { get; private set; }

    /// Các script UI lắng nghe sự kiện này để bật/tắt panel đúng lúc (làm ở file 23).
    public event Action<GamePhase> PhaseChanged;

    void Awake()
    {
        Instance = this;
        State = startPhase;
    }

    void OnDestroy()
    {
        Time.timeScale = 1f; 
    }

    /// Đổi trạng thái và báo cho những ai quan tâm.
    public void SetPhase(GamePhase phase)
    {
        if (State == phase) return;

        State = phase;
        PhaseChanged?.Invoke(phase);
    }

    /// Gọi khi bóng vào khung thành.
    public void Win()
    {
        if (State != GamePhase.Playing) return;   // tránh gọi hai lần

        SetPhase(GamePhase.Win);                  // khóa phím trong lúc chờ
        if (AudioManager.Instance != null) AudioManager.Instance.Play(Sfx.Win);

        LevelManager lm = LevelManager.Instance;
        if (lm == null) return;

        SaveSystem.CompleteLevel(lm.CurrentLevelID, lm.database.Count);
        StartCoroutine(AutoNextLevel(lm.CurrentIndex));
    }
    
    IEnumerator AutoNextLevel(int wonIndex)
    {
        yield return new WaitForSecondsRealtime(nextLevelDelay);

        // Người chơi đã bấm chơi lại / thoát trong lúc chờ thì thôi, không tự nạp nữa
        if (State != GamePhase.Win || LevelManager.Instance.CurrentIndex != wonIndex) yield break;

        if (!LevelManager.Instance.NextLevel())
        {
            // Đã hết màn
            if (UIManager.Instance != null) UIManager.Instance.GoToLevelSelect();
            else SetPhase(GamePhase.Menu);
        }
    }
}
