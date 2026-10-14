using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public PlayerCharacter player;
    public Ball ball;
    
    public float respawnDelay = 0.4f;

    const int MaxConeKicks = 30;   // chặn NPC đá cọc qua lại mãi

    bool busy;          // true khi bóng đang lăn
    bool coneKicking;   // true khi cọc đang bị NPC đá

    public bool IsBusy => busy || coneKicking;

    // Dừng mọi thứ đang chạy và mở khóa input
    public void ResetState()
    {
        StopAllCoroutines();
        busy = false;
        coneKicking = false;
    }

    void Update()
    {
        if (!IsPlaying() || player == null) return;
        if (player.IsMoving) return;   // đang bước

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!IsBusy) HandleShoot();  // bóng đang lăn thì không sút
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

        if (!IsBusy) RecordUndo();    // bước đi lúc bóng đang lăn gộp chung vào lượt sút, không tách riêng
        player.SlideTo(target);
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

        // Sau khi xoay, nếu ngay trước mặt NPC có cọc thì NPC đá cọc đi
        if (GridManager.Instance.GetAt(npc.FrontCell) is TrainingCone frontCone) StartCoroutine(KickConeRoutine(frontCone));

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
        cone.SlideTo(to);
        StartCoroutine(KickConeRoutine(cone));   // cọc trượt vào ô xong, nếu ô đó nằm trước mặt NPC thì bị đá tiếp
    }

    // Cọc phải trượt vào ô xong rồi mới bị đá. Sau mỗi cú đá lại chờ cọc trượt xong rồi xét tiếp.
    IEnumerator KickConeRoutine(TrainingCone cone)
    {
        coneKicking = true;

        for (int i = 0; i < MaxConeKicks; i++)
        {
            while (cone != null && cone.IsMoving) yield return null;
            if (cone == null) break;   // cọc đã biến mất (bóng đập, Undo...)

            NPC kicker = RuleChecker.Instance.FindConeKicker(cone);
            if (kicker == null) break;

            kicker.Kick();
            cone.SlideTo(cone.posIndex + kicker.facing.ToVector());
        }

        coneKicking = false;
    }

    IEnumerator RunRoll(RollResult result)
    {
        busy = true;

        // Nhớ vị trí trước cú sút, nếu bóng bay ra ngoài thì cả người lẫn bóng quay về đây
        Vector2Int ballStart = ball.posIndex;
        Vector2Int playerStart = player.posIndex;
        Direction playerFacing = player.facingDir;

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
            ResetPositions(playerStart, playerFacing, ballStart);
        }
        else if (ball.RollCompleted && result.hitCone != null)
        {
            // Bóng đập cọc
            result.hitCone.Disappear();
        }

        if (RuleChecker.Instance.CheckWin(ball)) OnWin();

        busy = false;
    }

    // Đặt lại cả người và bóng về vị trí cũ, mỗi vật thể phát effect của GridObject
    void ResetPositions(Vector2Int playerCell, Direction playerFacing, Vector2Int ballCell)
    {
        GridManager g = GridManager.Instance;

        // Gỡ cả hai khỏi bảng ô trước rồi mới đặt lại, tránh ghi đè nhau
        g.Remove(player);
        g.Remove(ball);

        player.posIndex = playerCell;
        g.Place(player);
        player.Face(playerFacing);   // quay mặt và căn lại giữa ô (hủy cú trượt đang chạy)
        player.Effect();

        ball.Respawn(ballCell);
        ball.Effect();
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