using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    public GamePhase startPhase = GamePhase.Menu;
    public float nextLevelDelay = 1.5f;
    
    public GamePhase State { get; private set; }
    
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
    
    public void SetPhase(GamePhase phase)
    {
        if (State == phase) return;

        State = phase;
        PhaseChanged?.Invoke(phase);
    }
    
    public void Win()
    {
        if (State != GamePhase.Playing) return;   // tránh gọi hai lần

        SetPhase(GamePhase.Win);                  // khóa phím trong lúc chờ
        AudioManager.Play(Sfx.Win);

        LevelManager lm = LevelManager.Instance;
        if (lm == null) return;

        SaveSystem.CompleteLevel(lm.CurrentLevelID, lm.database.Count);
        StartCoroutine(AutoNextLevel(lm.CurrentIndex));
    }
    
    IEnumerator AutoNextLevel(int wonIndex)
    {
        yield return new WaitForSecondsRealtime(nextLevelDelay);
        
        if (State != GamePhase.Win || LevelManager.Instance.CurrentIndex != wonIndex) yield break;

        if (!LevelManager.Instance.NextLevel())
        {
            // Đã hết màn
            if (UIManager.Instance != null) UIManager.Instance.GoToLevelSelect();
            else SetPhase(GamePhase.Menu);
        }
    }
}
