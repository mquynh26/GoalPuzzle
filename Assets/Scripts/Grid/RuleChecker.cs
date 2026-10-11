using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct KickEvent
{
    public int pathIndex;   // chỉ số ô trong path nơi bóng bị đá (-1 = bóng đang nằm yên, bị đá ngay từ đầu)
    public NPC npc;
}

/// <summary>Kết quả mô phỏng một lần bóng lăn.</summary>
public class RollResult
{
    public List<Vector2Int> path = new List<Vector2Int>();   // các ô bóng đi qua, theo thứ tự
    public TrainingCone hitCone;                             // cọc bị bóng đập (nếu có)
    public List<KickEvent> kicks = new List<KickEvent>();    // các cú đá của NPC

    /// Không có gì xảy ra: bóng không nhúc nhích và không phá cọc nào.
    public bool IsEmpty => path.Count == 0 && hitCone == null;
}

/// <summary>
/// Chứa toàn bộ luật chơi. Chỉ TÍNH TOÁN, không di chuyển gì cả:
/// PlayerController nhận kết quả rồi mới ra lệnh cho các đối tượng.
/// </summary>
public class RuleChecker : MonoBehaviour
{
    public static RuleChecker Instance { get; private set; }

    const int MaxSteps = 200;   // chặn vòng lặp vô tận
    const int MaxKicks = 30;    // chặn NPC đá bóng qua lại mãi

    void Awake()
    {
        Instance = this;
    }

    // ---------- Di chuyển ----------

    /// Cầu thủ có bước vào ô này được không: trong biên và không bị vật nào chiếm.
    public bool CanWalk(Vector2Int cell)
    {
        GridManager g = GridManager.Instance;
        return g.InBounds(cell) && g.GetAt(cell) == null;
    }

    /// Cọc có đẩy sang ô này được không: như CanWalk, và không được đẩy lên ô khung thành
    /// (nếu không cọc sẽ chắn mất đường bóng vào gôn).
    public bool CanMoveConeTo(Vector2Int cell)
    {
        return CanWalk(cell) && !GridManager.Instance.IsGoal(cell);
    }

    // ---------- Bóng lăn ----------

    /// NPC nào đang nhìn thẳng vào ô 'cell' (ô này nằm ngay trước mặt NPC)? Không có thì null.
    NPC FindKicker(Vector2Int cell)
    {
        GridManager g = GridManager.Instance;

        for (int i = 0; i < 4; i++)
        {
            var d = (Direction)i;
            // NPC đứng ở ô kề theo hướng d, và mặt nó quay ngược lại về phía 'cell'
            if (g.GetAt(cell + d.ToVector()) is NPC npc && npc.facing == d.Opposite())
            {
                return npc;
            }
        }
        return null;
    }

    /// Mô phỏng bóng bị sút theo hướng dir từ vị trí hiện tại của nó.
    /// Dừng khi gặp: biên, hàng rào, cọc, người, NPC (từ bên hông/sau lưng), hoặc ô khung thành.
    /// Bóng đi vào ô trước mặt một NPC thì bị đá, đổi sang hướng nhìn của NPC.
    public RollResult SimulateRoll(Ball ball, Direction dir)
    {
        GridManager g = GridManager.Instance;
        var result = new RollResult();
        Vector2Int cur = ball.posIndex;
        int kickCount = 0;

        for (int step = 0; step < MaxSteps; step++)
        {
            Vector2Int next = cur + dir.ToVector();
            if (!g.InBounds(next)) break;

            // Ô bóng xuất phát vẫn đang ghi tên chính bóng nên coi như trống
            GridObject obj = g.GetAt(next);
            if (obj != null && obj != ball)
            {
                if (obj is TrainingCone cone) result.hitCone = cone;   // đập cọc: cọc biến mất, bóng dừng
                break;                                                  // hàng rào, người, NPC...: bóng dừng
            }

            result.path.Add(next);
            cur = next;

            if (g.IsGoal(cur)) break;                                   // vào khung thành: dừng, thắng

            NPC kicker = FindKicker(cur);
            if (kicker != null)
            {
                if (++kickCount > MaxKicks) break;
                result.kicks.Add(new KickEvent { pathIndex = result.path.Count - 1, npc = kicker });
                dir = kicker.facing;                                    // bị đá đi theo hướng NPC nhìn
            }
        }

        return result;
    }

    /// Dùng khi bóng đang NẰM YÊN mà có NPC vừa xoay mặt vào nó.
    /// Nếu bóng đang ở ô trước mặt một NPC thì trả về kết quả cú đá, ngược lại (hoặc bóng bị chặn ngay) trả về null.
    public RollResult ResolveRestingKick(Ball ball)
    {
        NPC kicker = FindKicker(ball.posIndex);
        if (kicker == null) return null;

        RollResult result = SimulateRoll(ball, kicker.facing);
        if (result.IsEmpty) return null;

        result.kicks.Insert(0, new KickEvent { pathIndex = -1, npc = kicker });
        return result;
    }

    // ---------- Thắng / bế tắc ----------

    /// Bóng đang nằm trong ô khung thành.
    public bool CheckWin(Ball ball)
    {
        return GridManager.Instance.IsGoal(ball.posIndex);
    }

    /// Bế tắc: bóng kẹt trong góc, bị biên hoặc hàng rào chặn ở hai hướng vuông góc
    /// và không nằm trong khung thành. Chỉ tính hàng rào và biên vì đó là vật chặn vĩnh viễn.
    public bool CheckDeadlock(Ball ball)
    {
        if (CheckWin(ball)) return false;

        bool up    = IsPermanentBlock(ball.posIndex + Vector2Int.up);
        bool down  = IsPermanentBlock(ball.posIndex + Vector2Int.down);
        bool left  = IsPermanentBlock(ball.posIndex + Vector2Int.left);
        bool right = IsPermanentBlock(ball.posIndex + Vector2Int.right);

        return (up || down) && (left || right);
    }

    bool IsPermanentBlock(Vector2Int cell)
    {
        GridManager g = GridManager.Instance;
        return !g.InBounds(cell) || g.GetAt(cell) is Fence;
    }
}
