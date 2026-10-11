using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class FacingVisual : MonoBehaviour
{
    public enum Mode { SwapSprite, Rotate }

    public Mode mode = Mode.SwapSprite;

    [Tooltip("Thứ tự: 0 = Up, 1 = Right, 2 = Down, 3 = Left")]
    public Sprite[] sprites = new Sprite[4];

    SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    /// Cập nhật hình ảnh theo hướng d
    public void Apply(Direction d)
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();

        if (mode == Mode.SwapSprite)
        {
            int i = (int)d;
            if (sprites != null && i < sprites.Length && sprites[i] != null)
            {
                sr.sprite = sprites[i];
            }
        }
        else
        {
            // Up = 0°, Right = -90°, Down = -180°, Left = -270°
            transform.rotation = Quaternion.Euler(0f, 0f, -90f * (int)d);
        }
    }
}
