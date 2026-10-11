using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UndoManager : MonoBehaviour
{
    public static UndoManager Instance { get; private set; }

    [Tooltip("Dùng để chặn Undo khi bóng đang lăn")]
    public PlayerController playerController;

    /// LevelManager (hoặc file test) đăng ký hai hàm này khi nạp màn.
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

    /// Xóa toàn bộ lịch sử (khi nạp màn mới hoặc chơi lại).
    public void Clear()
    {
        stack.Clear();
    }

    /// Chụp trạng thái hiện tại và đẩy lên chồng. Gọi TRƯỚC khi thay đổi gì.
    public void Record()
    {
        if (capture == null) return;

        stack.Push(capture());
    }

    /// Quay lại trạng thái trước hành động gần nhất.
    public void Undo()
    {
        if (!CanUndo || apply == null) return;
        if (playerController != null && playerController.IsBusy) return;   // bóng đang lăn: không Undo
        if (GameManager.Instance != null && GameManager.Instance.State != GamePhase.Playing) return;

        apply(stack.Pop());
        if (AudioManager.Instance != null) AudioManager.Instance.Play(Sfx.Undo);
    }
}
