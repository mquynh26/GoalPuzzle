using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Level_01", menuName = "Game/Level Data")]
public class LevelData : ScriptableObject
{
    public int levelID = 1;
    public Vector2Int gridSize = new Vector2Int(15, 8);   // x = số cột, y = số hàng
    public List<int> gridElements = new List<int>();     // mã ô theo CellType

    /// Số ô của lưới.
    public int CellCount => gridSize.x * gridSize.y;

    /// Lấy loại ô tại (x, y). Ngoài biên hoặc dữ liệu thiếu thì trả về Empty.
    public CellType Get(int x, int y)
    {
        if (x < 0 || y < 0 || x >= gridSize.x || y >= gridSize.y) return CellType.Empty;

        int index = y * gridSize.x + x;
        if (index >= gridElements.Count) return CellType.Empty;

        return (CellType)gridElements[index];
    }

    /// Gán loại ô tại (x, y). Dùng cho Level Editor.
    public void Set(int x, int y, CellType type)
    {
        if (x < 0 || y < 0 || x >= gridSize.x || y >= gridSize.y) return;

        EnsureSize();
        gridElements[y * gridSize.x + x] = (int)type;
    }

    /// Đảm bảo danh sách có đúng gridSize.x * gridSize.y phần tử (thiếu thì thêm 0, thừa thì cắt).
    public void EnsureSize()
    {
        int target = Mathf.Max(0, CellCount);

        while (gridElements.Count < target) gridElements.Add((int)CellType.Empty);
        if (gridElements.Count > target) gridElements.RemoveRange(target, gridElements.Count - target);
    }

    /// Tự chỉnh khi bạn sửa gridSize trong Inspector.
    void OnValidate()
    {
        gridSize.x = Mathf.Max(1, gridSize.x);
        gridSize.y = Mathf.Max(1, gridSize.y);
        EnsureSize();
    }
}
