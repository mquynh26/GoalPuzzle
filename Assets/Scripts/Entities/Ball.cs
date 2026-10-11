using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : GridObject
{
    [Tooltip("Tốc độ lăn, tính bằng đơn vị world mỗi giây (ô 1x1 thì bằng số ô mỗi giây)")]
    public float rollSpeed = 12f;

    public bool isRolling { get; private set; }

    /// true nếu lần lăn gần nhất đi hết đường đã tính; false nếu bị chặn giữa chừng.
    public bool RollCompleted { get; private set; }

    /// Lăn lần lượt qua từng ô trong path. onCellReached(i) được gọi khi bóng tới ô thứ i.
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
                RollCompleted = false;   // bị chặn: bóng dừng ngay tại ô hiện tại
                break;
            }

            // Chiếm ô ngay khi bắt đầu đi tới đó, để người chơi không chen vào giữa chừng
            GridManager.Instance.Move(this, cell);

            Vector3 target = GridManager.Instance.CellToWorld(cell);
            while ((transform.position - target).sqrMagnitude > 0.0001f)
            {
                transform.position = Vector3.MoveTowards(transform.position, target, rollSpeed * Time.deltaTime);
                yield return null;
            }

            onCellReached?.Invoke(i);
        }

        Stop();
    }

    /// Kết thúc lăn: tắt cờ isRolling và căn bóng về đúng tâm ô cuối cùng.
    public void Stop()
    {
        isRolling = false;
        Show();
    }
}
