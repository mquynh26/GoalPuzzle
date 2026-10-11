using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LevelState
{
    Locked,      // chưa mở khóa (hoặc chưa có màn này): màu xám, khung chọn không dừng vào được
    Open,        // đã mở, chưa hoàn thành
    Completed    // đã hoàn thành
}

/// <summary>
/// Một ô số trong bảng chọn màn: số màn (luôn 2 chữ số, ví dụ 01, 12, 30) ghép từ hai sprite
/// chữ số đơn 0-9, nằm giữa 1 ô grid của sân.
/// Chỉ lo phần hiển thị. Việc di chuyển khung chọn và vào màn do LevelSelectUI xử lý.
/// Lưu ý: ô chọn màn chỉ là hình, KHÔNG được Place vào bảng ô bị chiếm của GridManager.
/// </summary>
public class LevelButton : GridObject
{
    [Header("Hiển thị")]
    [Tooltip("Khoảng cách giữa hai chữ số, tính bằng đơn vị world (1 pixel của sprite 32px = 0.03125)")]
    public float digitGap = 0f;
    public int sortingOrder = 5;

    [Header("Màu theo trạng thái (tô lên sprite chữ số màu trắng)")]
    public Color lockedColor = new Color(0.55f, 0.55f, 0.55f, 1f);
    public Color openColor = Color.white;
    [Tooltip("Màu màn đã hoàn thành. Mặc định trùng màu màn đã mở, đổi nếu muốn phân biệt.")]
    public Color completedColor = Color.white;

    /// Vị trí của màn trong LevelDatabase (0-based).
    public int Index { get; private set; }

    public LevelState State { get; private set; } = LevelState.Locked;

    /// Chỉ ô đã mở mới được khung chọn dừng vào.
    public bool IsSelectable => State != LevelState.Locked;

    SpriteRenderer tens;   // chữ số hàng chục
    SpriteRenderer ones;   // chữ số hàng đơn vị

    /// Gán số màn. digits là mảng 10 sprite theo thứ tự 0, 1, ... 9.
    /// Màn index 0 hiện "01", index 11 hiện "12".
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

        // Xếp hai chữ số cạnh nhau, cả cặp nằm giữa ô.
        // Trừ bounds.center để bù pivot của từng sprite, nên pivot đặt ở đâu cũng được.
        float wT = tensSprite != null ? tensSprite.bounds.size.x : 0f;
        float wO = onesSprite != null ? onesSprite.bounds.size.x : 0f;
        float total = wT + digitGap + wO;

        Vector3 centerT = tensSprite != null ? tensSprite.bounds.center : Vector3.zero;
        Vector3 centerO = onesSprite != null ? onesSprite.bounds.center : Vector3.zero;

        tens.transform.localPosition = new Vector3(-total / 2f + wT / 2f, 0f, 0f) - centerT;
        ones.transform.localPosition = new Vector3(total / 2f - wO / 2f, 0f, 0f) - centerO;

        Show();   // đưa gốc của ô về tâm ô grid (gốc không có sprite nên đặt thẳng vào tâm)
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

    // Tạo sẵn hai object con mang sprite chữ số, nên prefab không cần cấu hình gì thêm
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