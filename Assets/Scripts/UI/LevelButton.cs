using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LevelState
{
    Locked,      // chưa mở khóa
    Open,        // đã mở, chưa hoàn thành
    Completed    // đã hoàn thành
}

public class LevelButton : GridObject
{
    protected override bool SortByY => false;
    public float digitGap = 0f;
    public int sortingOrder = 5;
    
    public Color lockedColor = new Color(0.55f, 0.55f, 0.55f, 1f);
    public Color openColor = Color.white;
    public Color completedColor = Color.cyan;

    // Vị trí của màn trong LevelDatabase
    public int Index { get; private set; }

    public LevelState State { get; private set; } = LevelState.Locked;

    // Chỉ ô đã mở mới được khung chọn dừng vào.
    public bool IsSelectable => State != LevelState.Locked;

    SpriteRenderer tens;   // chữ số hàng chục
    SpriteRenderer ones;   // chữ số hàng đơn vị

    // Gán số màn. digits là mảng 10 sprite theo thứ tự 0, 1, ... 9
    public void Setup(int index, Sprite[] digits, Vector2Int cell)
    {
        EnsureRenderers();
        Index = index;
        posIndex = cell;

        int number = Mathf.Clamp(index + 1, 0, 99);
        Sprite tensSprite = digits[number / 10];
        Sprite onesSprite = digits[number % 10];
        tens.sprite = tensSprite;
        ones.sprite = onesSprite;

        // Xếp hai chữ số cạnh nhau, cả cặp nằm giữa ô
        // Trừ bounds.center để bù pivot của từng sprite, nên pivot đặt ở đâu cũng được
        float wT = tensSprite != null ? tensSprite.bounds.size.x : 0f;
        float wO = onesSprite != null ? onesSprite.bounds.size.x : 0f;
        float total = wT + digitGap + wO;

        Vector3 centerT = tensSprite != null ? tensSprite.bounds.center : Vector3.zero;
        Vector3 centerO = onesSprite != null ? onesSprite.bounds.center : Vector3.zero;

        tens.transform.localPosition = new Vector3(-total / 2f + wT / 2f, 0f, 0f) - centerT;
        ones.transform.localPosition = new Vector3(total / 2f - wO / 2f, 0f, 0f) - centerO;

        Show();   // đưa gốc của ô về tâm ô grid
    }

    public void SetState(LevelState state)
    {
        State = state;
        EnsureRenderers();

        Color c;
        switch (state)
        {
            case LevelState.Locked:    c = lockedColor; break;
            case LevelState.Completed: c = completedColor; break;
            default:                   c = openColor; break;
        }
        tens.color = c;
        ones.color = c;
    }
    
    void EnsureRenderers()
    {
        if (tens == null) tens = MakeDigit("Tens");
        if (ones == null) ones = MakeDigit("Ones");
    }

    SpriteRenderer MakeDigit(string objName)
    {
        var go = new GameObject(objName);
        go.transform.SetParent(transform, false);
        var r = go.AddComponent<SpriteRenderer>();
        r.sortingOrder = sortingOrder;
        return r;
    }
}