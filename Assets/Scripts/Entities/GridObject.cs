using System.Collections;
using UnityEngine;

public abstract class GridObject : MonoBehaviour
{
    public Vector2Int posIndex;
    public GameObject anim;

    // Tốc độ trượt khi đổi ô (dùng cho Player và Cone)
    public float moveSpeed = 8f;

    /// true trong lúc hình đang trượt sang ô mới
    public bool IsMoving { get; private set; }

    SpriteRenderer sr;
    Coroutine slide;

    public Vector3 WorldPosFor(Vector2Int cell)
    {
        Vector3 center = GridManager.Instance.CellToWorld(cell);

        if (sr == null) sr = GetComponent<SpriteRenderer>();
        if (sr == null || sr.sprite == null) return center;

        // Tâm hình của sprite tính từ pivot, đã nhân scale
        Vector3 localCenter = Vector3.Scale(sr.sprite.bounds.center, transform.lossyScale);
        if (sr.flipX) localCenter.x = -localCenter.x;

        return center - transform.rotation * localCenter;
    }

    /// Đổi sang ô mới: ô logic đổi ngay, hình trượt dần sang ô đó
    public void SlideTo(Vector2Int to)
    {
        GridManager.Instance.Move(this, to);

        if (slide != null) StopCoroutine(slide);
        slide = StartCoroutine(SlideRoutine());
    }

    IEnumerator SlideRoutine()
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
    public virtual void Show()
    {
        if (slide != null) { StopCoroutine(slide); slide = null; }
        IsMoving = false;
        transform.position = WorldPosFor(posIndex);
    }

    public void Effect()
    {
        if (anim == null) return;
        Instantiate(anim, WorldPosFor(posIndex), Quaternion.identity, transform);
    }
}