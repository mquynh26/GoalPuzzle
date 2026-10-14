using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct KickEvent
{
    public int pathIndex;  
    public NPC npc;
}

public class RollResult
{
    public List<Vector2Int> path = new List<Vector2Int>();   // các ô bóng đi qua, theo thứ tự
    public TrainingCone hitCone;                             // cọc bị bóng đập 
    public List<KickEvent> kicks = new List<KickEvent>();    // các cú đá của NPC
    public bool outOfBounds;                                 // bóng đã lăn ra khỏi lưới
    public Direction exitDir;                                // hướng bóng đang lăn lúc ra khỏi lưới
    
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

    // Di chuyển
    // Cầu thủ có bước vào ô này được không
    public bool CanWalk(Vector2Int cell)
    {
        GridManager g = GridManager.Instance;
        return g.InBounds(cell) && g.GetAt(cell) == null;
    }

    // Cọc có đẩy sang ô này được không
    public bool CanMoveConeTo(Vector2Int cell)
    {
        return CanWalk(cell);
    }

    //Bóng lăn
    // NPC nào đang nhìn thẳng vào ô
    NPC FindKicker(Vector2Int cell)
    {
        GridManager g = GridManager.Instance;

        for (int i = 0; i < 4; i++)
        {
            var d = (Direction)i;
            if (g.GetAt(cell + d.ToVector()) is NPC npc && npc.facing == d.Opposite())
            {
                return npc;
            }
        }
        return null;
    }
    
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
            
            GridObject obj = g.GetAt(next);
            
            bool passableNow = obj is PlayerCharacter && result.path.Count > 0;

            if (obj != null && obj != ball && !passableNow)
            {
                if (obj is TrainingCone cone) result.hitCone = cone;   // đập cọc
                break;                                                  // hàng rào, người, NPC
            }

            result.path.Add(next);
            cur = next;

            if (g.IsGoal(cur)) break;                                   // vào khung thành

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

    // Bóng đang nằm trong ô khung thành
    public bool CheckWin(Ball ball)
    {
        return GridManager.Instance.IsGoal(ball.posIndex);
    }
}