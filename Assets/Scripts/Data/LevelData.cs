using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Level", menuName = "Game/Level Data")]
public class LevelData : ScriptableObject
{
    public int levelID = 1;
    public Vector2Int gridSize = new Vector2Int(15, 8);   // x = số cột, y = số hàng
    public List<int> gridElements = new List<int>();     // mã ô theo CellType

    // Hướng nhìn của NPC, cùng chỉ số với gridElements (0 = Up, 1 = Right, 2 = Down, 3 = Left).
    // Chỉ có ý nghĩa ở những ô là NPC. Màn cũ chưa có dữ liệu này thì mặc định NPC nhìn lên.
    public List<int> npcFacings = new List<int>();

    // Số ô của lưới.
    public int CellCount => gridSize.x * gridSize.y;

    // Lấy loại ô tại (x, y), ngoài biên hoặc dữ liệu thiếu thì trả về Empty
    public CellType Get(int x, int y)
    {
        if (x < 0 || y < 0 || x >= gridSize.x || y >= gridSize.y) return CellType.Empty;

        int index = y * gridSize.x + x;
        if (index >= gridElements.Count) return CellType.Empty;

        return (CellType)gridElements[index];
    }

    // Hướng nhìn của NPC tại (x, y). Thiếu dữ liệu thì trả về Up
    public Direction GetNpcFacing(int x, int y)
    {
        if (x < 0 || y < 0 || x >= gridSize.x || y >= gridSize.y) return Direction.Up;

        int index = y * gridSize.x + x;
        if (index >= npcFacings.Count) return Direction.Up;

        return (Direction)npcFacings[index];
    }

    // Gán hướng nhìn của NPC tại (x, y) dùng cho Level Editor
    public void SetNpcFacing(int x, int y, Direction d)
    {
        if (x < 0 || y < 0 || x >= gridSize.x || y >= gridSize.y) return;

        EnsureSize();
        npcFacings[y * gridSize.x + x] = (int)d;
    }

    // Gán loại ô tại (x, y) dùng cho Level Editor
    public void Set(int x, int y, CellType type)
    {
        if (x < 0 || y < 0 || x >= gridSize.x || y >= gridSize.y) return;

        EnsureSize();
        gridElements[y * gridSize.x + x] = (int)type;
    }

    // Đảm bảo danh sách có đúng gridSize.x * gridSize.y phần tử
    public void EnsureSize()
    {
        int target = Mathf.Max(0, CellCount);

        while (gridElements.Count < target) gridElements.Add((int)CellType.Empty);
        if (gridElements.Count > target) gridElements.RemoveRange(target, gridElements.Count - target);

        while (npcFacings.Count < target) npcFacings.Add((int)Direction.Up);
        if (npcFacings.Count > target) npcFacings.RemoveRange(target, npcFacings.Count - target);
    }

    // Tự chỉnh khi bạn sửa gridSize trong Inspector.
    void OnValidate()
    {
        gridSize.x = Mathf.Max(1, gridSize.x);
        gridSize.y = Mathf.Max(1, gridSize.y);
        EnsureSize();
    }
}