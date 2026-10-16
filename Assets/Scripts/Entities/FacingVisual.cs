using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class FacingVisual : MonoBehaviour
{
    public enum Mode { SwapSprite, Rotate, Animator }

    public Mode mode = Mode.SwapSprite;

    [Tooltip("Thứ tự: 0 = Up, 1 = Right, 2 = Down, 3 = Left")]
    public Sprite[] sprites = new Sprite[4];

    public Animator animator;   // dùng cho Mode.Animator

    static readonly int FaceX = Animator.StringToHash("FaceX");
    static readonly int FaceY = Animator.StringToHash("FaceY");

    SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (animator == null) animator = GetComponent<Animator>();
    }

    public void Apply(Direction d)
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        if (animator == null) animator = GetComponent<Animator>();

        switch (mode)
        {
            case Mode.SwapSprite:
                int i = (int)d;
                if (sprites != null && i < sprites.Length && sprites[i] != null)
                    sr.sprite = sprites[i];
                break;

            case Mode.Rotate:
                transform.rotation = Quaternion.Euler(0f, 0f, -90f * (int)d);
                break;

            case Mode.Animator:
                if (animator == null) break;
                Vector2Int v = d.ToVector();
                animator.SetFloat(FaceX, v.x);
                animator.SetFloat(FaceY, v.y);
                break;
        }
    }
}