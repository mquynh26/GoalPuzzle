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
        if (animator == null) animator = GetComponent<Animator>();
    }

    // Ô ngay phía trước mặt NPC, bóng nằm ở ô này thì bị đá đi
    public Vector2Int FrontCell => posIndex + facing.ToVector();

    // Ô có phải ô ngay trước mặt NPC không.
    public bool IsFacing(Vector2Int cell)
    {
        return cell == FrontCell;
    }

    /// Xoay 90° theo chiều kim đồng hồ
    public void Rotate()
    {
        SetFacing(facing.RotateCW());
    }

    /// Đặt hướng trực tiếp
    public void SetFacing(Direction d)
    {
        facing = d;
        if (visual != null) visual.Apply(d);
    }

    /// Hiệu ứng sút bóng
    public void Kick()
    {
        if (animator != null) animator.SetTrigger("Kick");
    }
}
