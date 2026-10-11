using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GridObject : MonoBehaviour
{
    public Vector2Int posIndex;

    /// <summary>
    /// Đặt hình ảnh của vật thể về đúng tâm ô posIndex.
    /// </summary>
    public virtual void Show()
    {
        // TODO (File 6): sau khi có GridManager, thay dòng này bằng:
        transform.position = GridManager.Instance.CellToWorld(posIndex);
    }
}
