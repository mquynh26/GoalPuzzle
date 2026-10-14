using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct KickEvent
{
    public int pathIndex;   // chỉ số ô trong path nơi bóng bị đá (-1 = bóng đang nằm yên, bị đá ngay từ đầu)
    public NPC npc;
}

public class RollResult
{
    public List<Vector2Int> path = new List<Vector2Int>();   // các ô bóng đi qua, theo thứ tự
    public TrainingCone hitCone;                             // cọc bị bóng đập (nếu có)
    public List<KickEvent> kicks = new List<KickEvent>();    // các cú đá của NPC
    public bool outOfBounds;                                 // bóng đã lăn ra khỏi lưới
    public Direction exitDir;                                // hướng bóng đang lăn lúc ra khỏi lưới

    /// Không có gì xảy ra: bóng không nhúc nhích, không phá cọc, không ra khỏi lưới.
    public bool IsEmpty => path.Count == 0 && hitCone == null && !outOfBounds;
}

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
    /// Ô ghi bàn không bị chiếm nên đi vào được.
    public bool CanWalk(Vector2Int cell)
    {
        GridManager g = GridManager.Instance;
        return g.InBounds(cell) && g.GetAt(cell) == null;
    }

    /// Cọc có đẩy sang ô này được không: như CanWalk. Cọc được phép vào cả ô khung thành.
    public bool CanMoveConeTo(Vector2Int cell)
    {
        return CanWalk(cell);
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
    /// Dừng khi gặp: hàng rào, cột gôn, cọc, NPC (từ bên hông/sau lưng), hoặc ô khung thành.
    /// Người chơi chỉ chặn nếu đứng sát bóng; xa hơn thì Ball.Roll kiểm tra lại khi bóng thật sự tới.
    /// Ra khỏi lưới thì đánh dấu outOfBounds (bóng sẽ lăn khỏi màn hình rồi xuất hiện lại ở ô cũ).
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

            if (!g.InBounds(next))
            {
                result.outOfBounds = true;
                result.exitDir = dir;
                break;
            }

            // Ô bóng xuất phát vẫn đang ghi tên chính bóng nên coi như trống
            GridObject obj = g.GetAt(next);

            // Người chơi có thể bước đi chỗ khác trước khi bóng lăn tới ô này, nên từ ô thứ hai trở đi
            // không coi người là vật cản lúc tính trước. Nếu lúc bóng tới mà người vẫn đứng đó
            // thì Ball.Roll tự dừng bóng. Ô ngay sát bóng thì người không kịp tránh nên vẫn chặn.
            bool passableNow = obj is PlayerCharacter && result.path.Count > 0;

            if (obj != null && obj != ball && !passableNow)
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
    
    public NPC FindConeKicker(TrainingCone cone)
    {
        NPC kicker = FindKicker(cone.posIndex);
        if (kicker == null) return null;

        return CanMoveConeTo(cone.posIndex + kicker.facing.ToVector()) ? kicker : null;
    }

    // ---------- Thắng ----------

    /// Bóng đang nằm trong ô khung thành.
    public bool CheckWin(Ball ball)
    {
        return GridManager.Instance.IsGoal(ball.posIndex);
    }
}