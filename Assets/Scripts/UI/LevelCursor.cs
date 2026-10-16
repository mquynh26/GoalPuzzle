using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class LevelCursor : GridObject
{
    protected override bool SortByY => false;
    public void MoveTo(Vector2Int cell)
    {
        posIndex = cell;
        Show();
    }
}
