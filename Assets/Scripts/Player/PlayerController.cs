using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public PlayerCharacter player;
    public Ball ball;

    bool busy;   // true khi bóng đang lăn: chặn mọi thao tác

    public bool IsBusy => busy;

    void Update()
    {
        if (!IsPlaying() || player == null) return;

        if (TryGetDirection(out Direction d)) HandleMove(d);                    // đi: luôn được
        else if (Input.GetKeyDown(KeyCode.Space) && !busy) HandleShoot();       // sút: chỉ khi bóng đã dừng
    }

    // ---------- Đọc phím ----------

    bool TryGetDirection(out Direction d)
    {
        d = Direction.Up;
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))    { d = Direction.Up;    return true; }
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) { d = Direction.Right; return true; }
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))  { d = Direction.Down;  return true; }
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))  { d = Direction.Left;  return true; }
        return false;
    }

    // ---------- Đi ----------

    void HandleMove(Direction d)
    {
        player.Face(d);   // luôn quay mặt, kể cả khi bị chặn

        Vector2Int target = player.posIndex + d.ToVector();
        if (!RuleChecker.Instance.CanWalk(target)) return;

        if (!busy) RecordUndo(); 
        player.Move(target);
        PlaySound(Sfx.Move);
    }

    // ---------- Sút ----------

    void HandleShoot()
    {
        Direction dir = player.facingDir;
        Vector2Int front = player.FrontCell;
        GridObject target = GridManager.Instance.GetAt(front);

        if (target is Ball b)
        {
            ShootBall(b, dir);
        }
        else if (target is NPC npc)
        {
            RotateNPC(npc);
        }
        else if (target is TrainingCone cone)
        {
            PushCone(cone, dir);
        }
        // Ô trước mặt trống hoặc là hàng rào: không có gì xảy ra
    }

    void ShootBall(Ball b, Direction dir)
    {
        RollResult result = RuleChecker.Instance.SimulateRoll(b, dir);
        if (result.IsEmpty) return;   // bóng bị chặn ngay, không tính là một lượt

        RecordUndo();
        PlaySound(Sfx.Kick);
        StartCoroutine(RunRoll(result));
    }

    void RotateNPC(NPC npc)
    {
        RecordUndo();
        npc.Rotate();
        PlaySound(Sfx.Rotate);

        // Sau khi xoay, nếu bóng đang nằm yên ngay trước mặt một NPC thì NPC đá bóng đi
        if (ball != null)
        {
            RollResult kick = RuleChecker.Instance.ResolveRestingKick(ball);
            if (kick != null) StartCoroutine(RunRoll(kick));
        }
    }

    void PushCone(TrainingCone cone, Direction dir)
    {
        Vector2Int to = cone.posIndex + dir.ToVector();
        if (!RuleChecker.Instance.CanMoveConeTo(to)) return;

        RecordUndo();
        cone.MoveCone(to);
    }
    
    public void ResetState()
    {
        StopAllCoroutines();
        busy = false;
    }

    // ---------- Chạy một lần bóng lăn ----------

    IEnumerator RunRoll(RollResult result)
    {
        busy = true;

        // Cú đá khi bóng đang nằm yên (pathIndex = -1) thì NPC đá ngay lập tức
        foreach (KickEvent k in result.kicks)
        {
            if (k.pathIndex == -1) k.npc.Kick();
        }

        // Bóng lăn qua từng ô. Tới ô nào có NPC đá thì chạy animation NPC đó
        yield return ball.Roll(result.path, i =>
        {
            foreach (KickEvent k in result.kicks)
            {
                if (k.pathIndex == i) k.npc.Kick();
            }
        });

        // Bóng đập cọc: cọc biến mất
        if (ball.RollCompleted && result.hitCone != null)
        {
            result.hitCone.Disappear();
            PlaySound(Sfx.ConeBreak);
        }

        if (RuleChecker.Instance.CheckWin(ball)) OnWin();

        busy = false;
    }

    // ================= Các chỗ sẽ nối với file sau =================

    bool IsPlaying()
    {
        return GameManager.Instance != null && GameManager.Instance.State == GamePhase.Playing;
    }

    void RecordUndo()
    {
        if (UndoManager.Instance != null) UndoManager.Instance.Record();
    }

    void PlaySound(Sfx sfx)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.Play(sfx);
    }

    void OnWin()
    {
        GameManager.Instance.Win();
    }
}
