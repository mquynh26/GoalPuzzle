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

    // Xoay 90° theo chiều kim đồng hồ
    public void Rotate()
    {
        SetFacing(facing.RotateCW());
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
