using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct NPCState
{
    public Vector2Int pos;
    public Direction facing;
}

/// <summary>
/// Ảnh chụp toàn bộ phần thay đổi được của màn chơi tại một thời điểm (Bảng 3.18).
/// UndoManager lưu một chồng các ảnh chụp này; Undo là lấy ảnh trên cùng ra và khôi phục lại.
/// Những thứ không bao giờ đổi trong lúc chơi (hàng rào, khung thành, kích thước lưới) thì không cần lưu.
/// </summary>
public class GameState
{
    public Vector2Int playerPos;
    public Direction playerFacing;

    public Vector2Int ballPos;

    // Thứ tự phần tử phải khớp thứ tự NPC trong danh sách của LevelManager (cố định suốt màn chơi)
    public List<NPCState> npcs = new List<NPCState>();

    // Chỉ lưu các cọc còn lại. Cọc đã bị phá sẽ không có trong danh sách,
    // khi Undo thì tạo lại cọc từ vị trí lưu ở đây.
    public List<Vector2Int> cones = new List<Vector2Int>();
}
