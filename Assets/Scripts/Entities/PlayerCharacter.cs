using UnityEngine;

public class PlayerCharacter : GridObject
{
    public Direction facingDir = Direction.Down;
    public FacingVisual visual;

    void Awake()
    {
        if (visual == null) visual = GetComponent<FacingVisual>();
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
}