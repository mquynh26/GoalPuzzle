using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Grid))]
public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    public Grid grid;
    public Vector2Int size;   // x = số cột, y = số hàng của màn đang chơi

    [Header("Căn lưới")]
    [Tooltip("Dời lưới logic theo đơn vị world. Ô (0,0) sẽ nằm ở góc dưới trái + offset.")]
    public Vector2 gridOffset = Vector2.zero;

    // Ô bị chiếm bởi vật thể có thể chặn đường Player, Ball, NPC, Fence, Cone
    readonly Dictionary<Vector2Int, GridObject> occupied = new Dictionary<Vector2Int, GridObject>();

    // Ô khung thành không nằm trong occupied để người và bóng đi vào được
    readonly HashSet<Vector2Int> goals = new HashSet<Vector2Int>();

    Vector3 Offset => new Vector3(gridOffset.x, gridOffset.y, 0f);

    void Awake()
    {
        Instance = this;
        if (grid == null) grid = GetComponent<Grid>();
    }

    // Gọi mỗi khi dựng màn mới đặt kích thước và xóa sạch dữ liệu cũ
    public void Init(Vector2Int newSize)
    {
        size = newSize;
        occupied.Clear();
        goals.Clear();
    }

    // Đổi tọa độ
    // Ô (x, y) sang vị trí tâm ô trong world
    public Vector3 CellToWorld(Vector2Int cell)
    {
        return grid.GetCellCenterWorld(new Vector3Int(cell.x, cell.y, 0)) + Offset;
    }

    // Vị trí world sang ô (x, y)
    public Vector2Int WorldToCell(Vector3 world)
    {
        Vector3Int c = grid.WorldToCell(world - Offset);
        return new Vector2Int(c.x, c.y);
    }

    // Biên
    public bool InBounds(Vector2Int cell)
    {
        return cell.x >= 0 && cell.y >= 0 && cell.x < size.x && cell.y < size.y;
    }

    //Ô bị chiếm
    // Lấy vật thể đang đứng ở ô này, không có thì trả về null
    public GridObject GetAt(Vector2Int cell)
    {
        occupied.TryGetValue(cell, out GridObject obj);
        return obj;
    }

    // Đăng ký vật thể vào ô posIndex của nó
    public void Place(GridObject obj)
    {
        occupied[obj.posIndex] = obj;
    }

    /// Gỡ vật thể khỏi ô nó đang chiếm (chỉ gỡ nếu đúng là nó).
    public void Remove(GridObject obj)
    {
        if (occupied.TryGetValue(obj.posIndex, out GridObject current) && current == obj)
        {
            occupied.Remove(obj.posIndex);
        }
    }

    /// Chuyển vật thể sang ô mới: cập nhật cả posIndex lẫn bảng occupied.
    /// (Chưa cập nhật hình ảnh, hãy gọi obj.Show() sau đó.)
    public void Move(GridObject obj, Vector2Int to)
    {
        Remove(obj);
        obj.posIndex = to;
        Place(obj);
    }

    // ---------- Khung thành ----------

    public void AddGoal(Vector2Int cell) => goals.Add(cell);

    public bool IsGoal(Vector2Int cell) => goals.Contains(cell);

    // ================== DEBUG: vẽ khung lưới + đánh số ô ==================

    [Header("Debug")]
    public bool showGridGizmos = true;     // vẽ đường kẻ lưới
    public bool showCellLabels = true;     // hiện số ô "x,y" (chỉ thấy ở Scene view)
    public Vector2Int previewSize = new Vector2Int(6, 5);   // kích thước xem trước khi chưa chạy game
    public Color gridColor = new Color(1f, 1f, 1f, 0.6f);

    void OnDrawGizmos()
    {
        if (!showGridGizmos) return;

        Grid g = grid != null ? grid : GetComponent<Grid>();
        if (g == null) return;

        // Đang chơi thì dùng size thật, chưa chơi thì dùng previewSize
        Vector2Int s = (size.x > 0 && size.y > 0) ? size : previewSize;

        // 1) Đường kẻ lưới (đã cộng offset)
        Gizmos.color = gridColor;
        for (int x = 0; x <= s.x; x++)
        {
            Gizmos.DrawLine(g.CellToWorld(new Vector3Int(x, 0, 0)) + Offset,
                            g.CellToWorld(new Vector3Int(x, s.y, 0)) + Offset);
        }
        for (int y = 0; y <= s.y; y++)
        {
            Gizmos.DrawLine(g.CellToWorld(new Vector3Int(0, y, 0)) + Offset,
                            g.CellToWorld(new Vector3Int(s.x, y, 0)) + Offset);
        }

        // Chấm đánh dấu gốc ô (0,0)
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(g.CellToWorld(Vector3Int.zero) + Offset, 0.12f);

        if (!Application.isPlaying)
        {
            DrawCellLabels(g, s);
            return;
        }

        // 2) Khi đang chạy: tô màu ô bị chiếm (đỏ) và ô khung thành (xanh)
        Vector3 cube = new Vector3(g.cellSize.x * 0.9f, g.cellSize.y * 0.9f, 0.01f);
        for (int y = 0; y < s.y; y++)
        {
            for (int x = 0; x < s.x; x++)
            {
                var cell = new Vector2Int(x, y);
                Vector3 center = g.GetCellCenterWorld(new Vector3Int(x, y, 0)) + Offset;

                if (IsGoal(cell))
                {
                    Gizmos.color = new Color(0f, 1f, 0f, 0.25f);
                    Gizmos.DrawCube(center, cube);
                }
                if (GetAt(cell) != null)
                {
                    Gizmos.color = new Color(1f, 0f, 0f, 0.25f);
                    Gizmos.DrawCube(center, cube);
                }
            }
        }

        DrawCellLabels(g, s);
    }

    void DrawCellLabels(Grid g, Vector2Int s)
    {
#if UNITY_EDITOR
        if (!showCellLabels) return;

        var style = new GUIStyle();
        style.normal.textColor = Color.yellow;
        style.alignment = TextAnchor.MiddleCenter;
        style.fontSize = 10;

        for (int y = 0; y < s.y; y++)
        {
            for (int x = 0; x < s.x; x++)
            {
                var cell = new Vector2Int(x, y);
                string text = x + "," + y;

                if (Application.isPlaying)
                {
                    GridObject obj = GetAt(cell);
                    if (obj != null)
                    {
                        string n = obj.GetType().Name;           // vd: PlayerCharacter, Ball, NPC
                        text += "\n" + n.Substring(0, Mathf.Min(4, n.Length));
                    }
                    else if (IsGoal(cell))
                    {
                        text += "\nGOAL";
                    }
                }

                UnityEditor.Handles.Label(g.GetCellCenterWorld(new Vector3Int(x, y, 0)) + Offset, text, style);
            }
        }
#endif
    }
}