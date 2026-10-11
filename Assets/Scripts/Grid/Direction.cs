using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Direction
{
    Up = 0,
    Right = 1,
    Down = 2,
    Left = 3
}

public static class DirectionExt
{
    /// Đổi hướng thành vector bước trên lưới (x = cột, y = hàng, y tăng là đi lên).
    public static Vector2Int ToVector(this Direction d)
    {
        switch (d)
        {
            case Direction.Up:    return Vector2Int.up;
            case Direction.Right: return Vector2Int.right;
            case Direction.Down:  return Vector2Int.down;
            default:              return Vector2Int.left;
        }
    }

    /// Xoay 90 độ theo chiều kim đồng hồ (dùng cho NPC.Rotate).
    public static Direction RotateCW(this Direction d)
    {
        return (Direction)(((int)d + 1) % 4);
    }

    /// Hướng ngược lại (dùng cho NPC.CanReceive).
    public static Direction Opposite(this Direction d)
    {
        return (Direction)(((int)d + 2) % 4);
    }
}
