using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainingCone : GridObject
{
    /// Báo cho LevelManager biết cọc vừa biến mất để gỡ khỏi danh sách quản lý.
    public static event Action<TrainingCone> Disappeared;

    /// Dịch cọc sang ô 'to'. Giả định ô đó đã được kiểm tra hợp lệ.
    public void MoveCone(Vector2Int to)
    {
        GridManager.Instance.Move(this, to);   // cập nhật posIndex + bảng occupied
        Show();                                // đưa hình ảnh về tâm ô mới
    }

    /// Cọc biến mất khỏi lưới (khi bóng đập vào).
    public void Disappear()
    {
        GridManager.Instance.Remove(this);     // trả ô lại cho bóng/người đi qua
        Disappeared?.Invoke(this);             // LevelManager lắng nghe (làm ở file 19)
        Destroy(gameObject);
    }
}
