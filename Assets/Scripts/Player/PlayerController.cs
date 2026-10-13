using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public PlayerCharacter player;
    public Ball ball;
    
    public float respawnDelay = 0.4f;

    bool busy;                  // true khi bóng đang lăn
    Vector2Int? reservedCell;   // ô cũ của bóng khi bóng đang bay ra ngoài

    public bool IsBusy => busy;

    // Dừng mọi thứ đang chạy và mở khóa input
    public void ResetState()
    {
        StopAllCoroutines();
        busy = false;
        reservedCell = null;
    }

    void Update()
    {
        if (!IsPlaying() || player == null) return;
        if (player.IsMoving) return;   // đang bước

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!busy) HandleShoot();   // bóng đang lăn thì không sút
            return;
        }

        if (TryGetDirection(out Direction d)) HandleMove(d);
    }

    bool TryGetDirection(out Direction d)
    {
        d = Direction.Up;
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))    { d = Direction.Up;    return true; }
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) { d = Direction.Right; return true; }
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))  { d = Direction.Down;  return true; }
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))  { d = Direction.Left;  return true; }
        return false;
    }

    // Đi
    void HandleMove(Direction d)
    {
        player.Face(d);   // luôn quay mặt, kể cả khi bị chặn

        Vector2Int target = player.posIndex + d.ToVector();
        if (!RuleChecker.Instance.CanWalk(target)) return;
        if (reservedCell.HasValue && reservedCell.Value == target) return;   // ô này dành cho bóng quay về

        if (!busy) RecordUndo();   // bước đi lúc bóng đang lăn gộp chung vào lượt sút, không tách riêng
        player.Move(target);
    }

    // ---------- Sút ----------

    void HandleShoot()
    {
        Direction dir = player.facingDir;
        GridObject target = GridManager.Instance.GetAt(player.FrontCell);

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
    }

    void ShootBall(Ball b, Direction dir)
    {
        RollResult result = RuleChecker.Instance.SimulateRoll(b, dir);
        if (result.IsEmpty) return;

        RecordUndo();
        StartCoroutine(RunRoll(result));
    }

    void RotateNPC(NPC npc)
    {
        RecordUndo();
        npc.Rotate();

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

    IEnumerator RunRoll(RollResult result)
    {
        busy = true;
        Vector2Int origin = ball.posIndex;   // ô bóng đứng trước khi bị sút

        // Bóng sắp bay ra ngoài và xuất hiện lại ở ô này nên không cho người chơi bước vào
        reservedCell = result.outOfBounds ? origin : (Vector2Int?)null;

        // Cú đá khi bóng đang nằm yên (pathIndex = -1) thì NPC đá ngay lập tức
        foreach (KickEvent k in result.kicks)
        {
            if (k.pathIndex == -1) k.npc.Kick();
        }

        // Bóng lăn qua từng ô
        yield return ball.Roll(result.path, i =>
        {
            foreach (KickEvent k in result.kicks)
            {
                if (k.pathIndex == i) k.npc.Kick();
            }
        });

        if (result.outOfBounds && ball.RollCompleted)
        {
            // Bóng ra khỏi lưới
            yield return ball.RollOffScreen(result.exitDir);
            yield return new WaitForSeconds(respawnDelay);
            ball.Respawn(origin);
        }
        else if (ball.RollCompleted && result.hitCone != null)
        {
            // Bóng đập cọc
            result.hitCone.Disappear();
        }

        reservedCell = null;
        if (RuleChecker.Instance.CheckWin(ball)) OnWin();

        busy = false;
    }

    bool IsPlaying()
    {
        return GameManager.Instance != null && GameManager.Instance.State == GamePhase.Playing;
    }

    void RecordUndo()
    {
        if (UndoManager.Instance != null) UndoManager.Instance.Record();
    }

    void OnWin()
    {
        GameManager.Instance.Win();
    }
}