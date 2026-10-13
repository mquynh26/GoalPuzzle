using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacter : GridObject
{
    public Direction facingDir = Direction.Down;
    public FacingVisual visual;
    
    public float moveSpeed = 8f;

    // true trong lúc hình đang trượt sang ô mới.
    public bool IsMoving { get; private set; }

    Coroutine slide;

    void Awake()
    {
        if (visual == null) visual = GetComponent<FacingVisual>();
    }

    /// Ô ngay phía trước mặt cầu thủ
    public Vector2Int FrontCell => posIndex + facingDir.ToVector();

    // Quay mặt về hướng d, cầu thủ vẫn quay mặt khi bị chặn
    public void Face(Direction d)
    {
        facingDir = d;
        if (visual != null) visual.Apply(d);
        Show();   // căn lại giữa ô vì sprite mới có thể có tâm khác
    }

    /// Bước sang ô , ô đó đã được RuleChecker.CanWalk xác nhận hợp lệ
    public void Move(Vector2Int to)
    {
        GridManager.Instance.Move(this, to);   // ô logic đổi ngay

        if (slide != null) StopCoroutine(slide);
        slide = StartCoroutine(Slide());       // hình trượt dần sang ô mới
    }

    IEnumerator Slide()
    {
        IsMoving = true;
        Vector3 target = WorldPosFor(posIndex);

        while ((transform.position - target).sqrMagnitude > 0.00001f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = target;
        IsMoving = false;
        slide = null;
    }

    /// Đặt thẳng hình về giữa ô posIndex, hủy cú trượt đang chạy
    public override void Show()
    {
        if (slide != null) { StopCoroutine(slide); slide = null; }
        IsMoving = false;
        base.Show();
    }
}
