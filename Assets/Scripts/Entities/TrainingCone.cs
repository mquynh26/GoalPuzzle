using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainingCone : GridObject
{
    /// Báo cho LevelManager biết cọc vừa biến mất để gỡ khỏi danh sách quản lý.
    public static event Action<TrainingCone> Disappeared;
    
    public float moveSpeed = 8f;

    /// true trong lúc hình đang trượt sang ô mới
    public bool IsMoving { get; private set; }

    Coroutine slide;

    /// Dịch cọc sang ô 
    public void MoveCone(Vector2Int to)
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

    /// Cọc biến mất khỏi lưới
    public void Disappear()
    {
        GridManager.Instance.Remove(this);     // trả ô lại cho bóng/người đi qua
        Disappeared?.Invoke(this);             // LevelManager lắng nghe
        Destroy(gameObject);
    }
}
