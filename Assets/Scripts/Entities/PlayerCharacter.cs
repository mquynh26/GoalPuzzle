using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacter : GridObject
{
    public Direction facingDir = Direction.Down;
    public FacingVisual visual;

    void Awake()
    {
        if (visual == null) visual = GetComponent<FacingVisual>();
    }

    /// Ô ngay phía trước mặt cầu thủ (dùng khi sút).
    public Vector2Int FrontCell => posIndex + facingDir.ToVector();

    /// Quay mặt về hướng d (không di chuyển). Cầu thủ vẫn quay mặt khi bị chặn,
    /// để có thể quay về phía bóng đang đứng sát cạnh rồi sút.
    public void Face(Direction d)
    {
        facingDir = d;
        if (visual != null) visual.Apply(d);
    }

    /// Bước sang ô 'to'. Giả định ô đó đã được RuleChecker.CanWalk xác nhận hợp lệ.
    public void Move(Vector2Int to)
    {
        GridManager.Instance.Move(this, to);   // cập nhật posIndex + bảng occupied
        Show();                                // đưa hình ảnh về tâm ô mới
    }
}
