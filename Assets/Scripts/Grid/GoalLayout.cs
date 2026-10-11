using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GoalLayout
{
    /// Mỗi bên ô giữa có 2 ô: tổng cộng 5 ô.
    public const int HalfWidth = 2;

    /// dx là độ lệch ngang so với ô giữa. Hai ô ngoài cùng (|dx| = 2) là cột gôn.
    public static bool IsPostOffset(int dx)
    {
        return Mathf.Abs(dx) == HalfWidth;
    }
}
