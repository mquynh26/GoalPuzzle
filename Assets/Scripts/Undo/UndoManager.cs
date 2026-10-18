using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UndoManager : MonoBehaviour
{
    public static UndoManager Instance { get; private set; }
    
    public PlayerController playerController;
    
    public Func<GameState> capture;
    public Action<GameState> apply;

    readonly Stack<GameState> stack = new Stack<GameState>();

    public bool CanUndo => stack.Count > 0;
    public int Count => stack.Count;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z)) Undo();
    }

    // Xóa toàn bộ lịch sử
    public void Clear()
    {
        stack.Clear();
    }

    // Chụp trạng thái hiện tại và đẩy lên chồng
    public void Record()
    {
        if (capture == null) return;

        stack.Push(capture());
    }

    // Quay lại trạng thái trước hành động gần nhất
    public void Undo()
    {
        if (!CanUndo || apply == null) return;
        if (playerController != null && playerController.IsBusy) return;   // bóng đang lăn không Undo
        if (GameManager.Instance != null && GameManager.Instance.State != GamePhase.Playing) return;
        AudioManager.Play(Sfx.Undo);
        apply(stack.Pop());
    }
}
