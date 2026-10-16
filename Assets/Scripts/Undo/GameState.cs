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

public class GameState
{
    public Vector2Int playerPos;
    public Direction playerFacing;

    // Thứ tự phần tử phải khớp thứ tự bóng trong danh sách của LevelManager
    public List<Vector2Int> ballPositions = new List<Vector2Int>();

    // Thứ tự phần tử phải khớp thứ tự NPC trong danh sách của LevelManager
    public List<NPCState> npcs = new List<NPCState>();

    // Chỉ lưu các cọc còn lại. Cọc đã bị phá sẽ không có trong danh sách,
    // khi Undo thì tạo lại cọc từ vị trí lưu ở đây.
    public List<Vector2Int> cones = new List<Vector2Int>();
}
