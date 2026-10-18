using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : GridObject
{
    public Direction facing = Direction.Up;
    public FacingVisual visual;
    public Animator animator;

    void Awake()
    {
        if (visual == null) visual = GetComponent<FacingVisual>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    void Start()
    {
        // Áp hướng ban đầu đặt trong Inspector
        SetFacing(facing);
    }

    // Ô ngay phía trước mặt NPC, bóng nằm ở ô này thì bị đá đi
    public Vector2Int FrontCell => posIndex + facing.ToVector();

    public bool IsFacing(Vector2Int cell)
    {
        return cell == FrontCell;
    }

    // Ô này NPC có thể quay mặt về hướng đó không (chỉ chặn bởi Fence/cột gôn và NPC khác)
    bool CanFace(Direction d)
    {
        GridObject obj = GridManager.Instance.GetAt(posIndex + d.ToVector());
        return !(obj is Fence) && !(obj is NPC);
    }

    // Tìm hướng tiếp theo theo chiều kim đồng hồ mà không bị Fence/NPC chặn.
    // Trả về false nếu cả 3 hướng còn lại đều bị chặn (không quay được)
    public bool TryGetNextFacing(out Direction next)
    {
        Direction d = facing;
        for (int i = 0; i < 3; i++)
        {
            d = d.RotateCW();
            if (CanFace(d))
            {
                next = d;
                return true;
            }
        }
        next = facing;
        return false;
    }

    // Xoay sang hướng hợp lệ kế tiếp theo chiều kim đồng hồ. Trả về true nếu có quay
    public bool Rotate()
    {
        if (!TryGetNextFacing(out Direction next)) return false;
        SetFacing(next);
        return true;
    }

    // Đặt hướng trực tiếp
    public void SetFacing(Direction d)
    {
        facing = d;
        if (visual != null) visual.Apply(d);

        if (animator != null)
        {
            Vector2Int v = d.ToVector();
            animator.SetFloat("FaceX", v.x);
            animator.SetFloat("FaceY", v.y);
        }
    }

    // Hiệu ứng sút bóng
    public void Kick()
    {
        if (animator == null) return;
        StartCoroutine(KickRoutine());
    }

    IEnumerator KickRoutine()
    {
        animator.SetTrigger("Kick");
        yield return null;   // đợi 1 frame cho Animator xử lý
        animator.ResetTrigger("Kick");
    }
}