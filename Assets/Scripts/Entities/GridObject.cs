using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GridObject : MonoBehaviour
{
    public Vector2Int posIndex;
    public GameObject anim;

    SpriteRenderer sr;
    
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

    public virtual void Show()
    {
        transform.position = WorldPosFor(posIndex);
    }

    public void Effect()
    {
        if (anim == null) return;
        Instantiate(anim, WorldPosFor(posIndex), Quaternion.identity, transform);
    }
}
