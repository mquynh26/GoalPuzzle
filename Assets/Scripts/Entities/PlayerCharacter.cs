using System.Collections;
using UnityEngine;

public class PlayerCharacter : GridObject
{
    public Direction facingDir = Direction.Down;
    public FacingVisual visual;
    public Animator animator;
    static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
    static readonly int KickHash = Animator.StringToHash("Kick");

    public float kickLockTime = 0.3f;   // thời gian khóa trong lúc đá
    float kickUntil;

    public bool IsKicking => Time.time < kickUntil;

    void Awake()
    {
        if (visual == null) visual = GetComponent<FacingVisual>();
        animator = GetComponent<Animator>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (animator != null) animator.SetBool(IsMovingHash, IsMoving);
    }
    // Ô ngay phía trước mặt cầu thủ
    public Vector2Int FrontCell => posIndex + facingDir.ToVector();

    // Quay mặt về hướng d, cầu thủ vẫn quay mặt khi bị chặn
    public void Face(Direction d)
    {
        facingDir = d;
        if (visual != null) visual.Apply(d);
        Show();   // căn lại giữa ô vì sprite mới có thể có tâm khác
    }
    
    public void Kick()
    {
        if (animator == null) return;
        if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Idle")) return;
        if (animator.IsInTransition(0)) return;
        animator.SetTrigger(KickHash);
        kickUntil = Time.time + kickLockTime;
    }

    // Gọi khi reset màn hoặc undo để mở khóa ngay
    public void CancelKick()
    {
        kickUntil = 0f;
        if (animator != null && animator.isActiveAndEnabled && animator.runtimeAnimatorController != null)
            animator.ResetTrigger(KickHash);
    }
}