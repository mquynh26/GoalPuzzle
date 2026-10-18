using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Bảng hiển thị số màn đang chơi, dùng sprite số 0-9
public class Board : GridObject
{
    [Header("Digit renderers (vị trí đặt sẵn trong prefab/scene)")]
    public SpriteRenderer tensDigit;   // chữ số hàng chục (màn < 10 thì hiện số 0)
    public SpriteRenderer onesDigit;   // chữ số hàng đơn vị

    [Header("Sprite số, thứ tự 0..9")]
    public Sprite[] digitSprites = new Sprite[10];

    void OnEnable()
    {
        LevelSelectUI.LevelSelected += SetLevel;
        LevelManager.LevelIndex += SetLevel;

        // Board bật sau khi màn đã nạp thì lấy số hiện tại
        if (LevelManager.Instance != null && LevelManager.Instance.CurrentData != null)
            SetLevel(LevelManager.Instance.CurrentLevelID);
    }

    void OnDisable()
    {
        LevelSelectUI.LevelSelected -= SetLevel;
        LevelManager.LevelIndex -= SetLevel;
    }

    // Luôn hiện 2 chữ số, màn dưới 10 thì có số 0 đứng trước (01, 02, ...)
    public void SetLevel(int level)
    {
        level = Mathf.Clamp(level, 0, 99);

        if (tensDigit != null) tensDigit.sprite = GetDigit(level / 10);
        if (onesDigit != null) onesDigit.sprite = GetDigit(level % 10);
    }

    Sprite GetDigit(int d)
    {
        return (digitSprites != null && d >= 0 && d < digitSprites.Length) ? digitSprites[d] : null;
    }
}