using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class LevelCursor : GridObject
{
    public void MoveTo(Vector2Int cell)
    {
        posIndex = cell;
        Show();
    }
}
