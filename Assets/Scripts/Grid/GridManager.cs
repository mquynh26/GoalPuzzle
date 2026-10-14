using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Grid))]
public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    public Grid grid;
    public Vector2Int size;   // x = số cột, y = số hàng của màn đang chơi
    
    public Vector2 gridOffset = Vector2.zero;
    
    public float outerMargin = 1f;

    // Ô bị chiếm bởi vật thể có thể chặn đường: Player, Ball, NPC, Fence, Cone, cột gôn
    readonly Dictionary<Vector2Int, GridObject> occupied = new Dictionary<Vector2Int, GridObject>();

    // Ô khung thành 0 nằm trong occupied để người, bóng và cọc đi vào được
    readonly HashSet<Vector2Int> goals = new HashSet<Vector2Int>();

    Vector3 Offset => new Vector3(gridOffset.x, gridOffset.y, 0f);

    void Awake()
    {
        Instance = this;
        if (grid == null) grid = GetComponent<Grid>();
    }

    /// Gọi mỗi khi dựng màn mới đặt kích thước và xóa sạch dữ liệu cũ
    public void Init(Vector2Int newSize)
    {
        size = newSize;
        occupied.Clear();
        goals.Clear();
    }

    /// Ô (x, y) -> vị trí tâm ô trong world
    public Vector3 CellToWorld(Vector2Int cell)
    {
        return grid.GetCellCenterWorld(new Vector3Int(cell.x, cell.y, 0)) + Offset;
    }

    /// Vị trí world -> ô (x, y)
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

    // Điểm bóng biến mất ngay sau mép màn hình
    public Vector3 OuterLimit(Vector3 from, Direction dir)
    {
        float pad = outerMargin * grid.cellSize.x;
        Camera cam = Camera.main;

        float left, right, bottom, top;
        if (cam != null && cam.orthographic)
        {
            float halfH = cam.orthographicSize;
            float halfW = halfH * cam.aspect;
            Vector3 c = cam.transform.position;
            left = c.x - halfW; right = c.x + halfW;
            bottom = c.y - halfH; top = c.y + halfH;
        }
        else
        {
            // Không có camera thì lấy mép lưới
            Vector3 cell = grid.cellSize;
            Vector3 min = CellToWorld(Vector2Int.zero) - cell / 2f;
            Vector3 max = CellToWorld(size - Vector2Int.one) + cell / 2f;
            left = min.x; right = max.x; bottom = min.y; top = max.y;
        }

        switch (dir)
        {
            case Direction.Up:    from.y = top + pad; break;
            case Direction.Down:  from.y = bottom - pad; break;
            case Direction.Right: from.x = right + pad; break;
            default:              from.x = left - pad; break;
        }
        return from;
    }
    

    // Vật thể đang đứng ở ô này, không có thì trả về null
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

    // Gỡ vật thể khỏi ô nó đang chiếm
    public void Remove(GridObject obj)
    {
        if (occupied.TryGetValue(obj.posIndex, out GridObject current) && current == obj)
        {
            occupied.Remove(obj.posIndex);
        }
    }

    // Chuyển vật thể sang ô mới cập nhật cả posIndex lẫn bảng occupied
    public void Move(GridObject obj, Vector2Int to)
    {
        Remove(obj);
        obj.posIndex = to;
        Place(obj);
    }
    

    public void AddGoal(Vector2Int cell) => goals.Add(cell);

    public bool IsGoal(Vector2Int cell) => goals.Contains(cell);

    // DEBUG: vẽ khung lưới + đánh số ô

    [Header("Debug")]
    public bool showGridGizmos = true;     // vẽ đường kẻ lưới
    public bool showCellLabels = true;     // hiện số ô "x,y"
    public Vector2Int previewSize = new Vector2Int(13, 7);   // kích thước xem trước khi chưa chạy game
    public Color gridColor = new Color(1f, 1f, 1f, 0.6f);

    void OnDrawGizmos()
    {
        if (!showGridGizmos) return;

        Grid g = grid != null ? grid : GetComponent<Grid>();
        if (g == null) return;

        // Đang chơi thì dùng size thật, chưa chơi thì dùng previewSize
        Vector2Int s = (size.x > 0 && size.y > 0) ? size : previewSize;

        // Đường kẻ lưới (đã cộng offset)
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

        if (!Application.isPlaying)
        {
            DrawCellLabels(g, s);
            return;
        }

        // ô màu ô bị chiếm đỏ và ô khung thành xanh
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
                        string n = obj.GetType().Name;          
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