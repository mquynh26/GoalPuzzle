using System;
using UnityEngine;

public class TrainingCone : GridObject
{
    /// Báo cho LevelManager biết cọc vừa biến mất để gỡ khỏi danh sách quản lý.
    public static event Action<TrainingCone> Disappeared;

    /// Cọc biến mất khỏi lưới
    public void Disappear()
    {
        GridManager.Instance.Remove(this);     // trả ô lại cho bóng/người đi qua
        Disappeared?.Invoke(this);             // LevelManager lắng nghe
        Destroy(gameObject);
    }
}