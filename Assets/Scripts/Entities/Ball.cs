using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : GridObject
{
    public float rollSpeed = 12f;

    public bool isRolling { get; private set; }

    // true nếu lần lăn gần nhất đi hết đường đã tính; false nếu bị chặn giữa chừng
    public bool RollCompleted { get; private set; }

    Renderer[] renderers;

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
    }
    
    public IEnumerator Roll(List<Vector2Int> path, System.Action<int> onCellReached = null)
    {
        isRolling = true;
        RollCompleted = true;

        for (int i = 0; i < path.Count; i++)
        {
            Vector2Int cell = path[i];

            // Đường được tính từ trước, nhưng người chơi có thể đã bước vào ô này trong lúc bóng lăn
            GridObject other = GridManager.Instance.GetAt(cell);
            if (other != null && other != this)
            {
                RollCompleted = false;   // bị chặn
                break;
            }

            // Chiếm ô ngay khi bắt đầu đi tới đó, để người chơi không chen vào giữa chừng
            GridManager.Instance.Move(this, cell);

            Vector3 target = WorldPosFor(cell);
            while ((transform.position - target).sqrMagnitude > 0.0001f)
            {
                transform.position = Vector3.MoveTowards(transform.position, target, rollSpeed * Time.deltaTime);
                yield return null;
            }

            onCellReached?.Invoke(i);
        }

        Stop();
    }
    
    public IEnumerator RollOffScreen(Direction dir)
    {
        isRolling = true;
        Vector3 target = GridManager.Instance.OuterLimit(transform.position, dir);

        while ((transform.position - target).sqrMagnitude > 0.0001f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, rollSpeed * Time.deltaTime);
            yield return null;
        }

        SetVisible(false);   // đã ra khỏi màn hình: biến mất
    }

    /// Bóng xuất hiện lại ở ô cell
    public void Respawn(Vector2Int cell)
    {
        GridManager.Instance.Move(this, cell);   // ô logic về ô cũ
        Show();                                  // đặt hình vào giữa ô
        SetVisible(true);
        isRolling = false;
    }
    
    public void Stop()
    {
        isRolling = false;
        Show();
    }

    void SetVisible(bool visible)
    {
        if (renderers == null) return;
        foreach (Renderer r in renderers)
        {
            if (r != null) r.enabled = visible;
        }
    }
}