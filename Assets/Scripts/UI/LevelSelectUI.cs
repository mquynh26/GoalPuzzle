using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSelectUI : MonoBehaviour
{
    [Header("Prefab và hình")]
    public LevelButton buttonPrefab;
    public LevelCursor cursor;
    [Tooltip("10 sprite chữ số theo đúng thứ tự 0, 1, 2, ... 9")]
    public Sprite[] digits = new Sprite[10];

    [Header("Bố cục trên lưới")]
    [Tooltip("Ô grid của màn 01 (góc trên trái của bảng). Xem số ô bằng khung debug của GridManager.")]
    public Vector2Int firstCell = new Vector2Int(2, 4);
    public int columns = 10;

    readonly List<LevelButton> buttons = new List<LevelButton>();
    int selected;
    int enabledFrame = -1;

    // ================= Mở / làm mới =================

    void OnEnable()
    {
        enabledFrame = Time.frameCount;   // bỏ qua phím của chính khung hình vừa mở bảng

        if (!Prepare()) return;
        RefreshStates();
        selected = FindStartIndex();
        MoveCursor();
    }

    /// Dựng các ô số theo đúng số màn trong database. Trả về false nếu chưa sẵn sàng.
    bool Prepare()
    {
        // Lúc vừa nạp scene, các hệ thống khác chưa kịp chạy Awake. Lần mở thật sau đó sẽ dựng bình thường
        if (GridManager.Instance == null || LevelManager.Instance == null) return false;

        LevelDatabase db = LevelManager.Instance.database;
        if (db == null || db.Count == 0)
        {
            Debug.LogWarning("LevelSelectUI: LevelDatabase trống hoặc chưa gán, không có màn nào để hiện.");
            ClearButtons();
            return false;
        }

        if (buttonPrefab == null || cursor == null || digits == null || digits.Length < 10)
        {
            Debug.LogError("LevelSelectUI: chưa gán Button Prefab, Cursor hoặc đủ 10 sprite chữ số.");
            return false;
        }

        // Đã dựng đủ rồi thì thôi. Số màn đổi (thêm màn vào database) thì dựng lại
        if (buttons.Count == db.Count) return true;

        ClearButtons();
        for (int i = 0; i < db.Count; i++)
        {
            LevelButton b = Instantiate(buttonPrefab, transform);
            b.name = "Level_" + (i + 1).ToString("00");
            b.Setup(i, digits, CellOf(i));
            buttons.Add(b);
        }
        return true;
    }

    void ClearButtons()
    {
        foreach (LevelButton b in buttons)
        {
            if (b != null) Destroy(b.gameObject);
        }
        buttons.Clear();
    }

    /// Cập nhật trạng thái từng ô: khóa / mở / đã xong.
    void RefreshStates()
    {
        LevelDatabase db = LevelManager.Instance.database;

        for (int i = 0; i < buttons.Count; i++)
        {
            LevelData data = db.GetData(i);

            LevelState state;
            if (data == null || !SaveSystem.IsUnlocked(data.levelID)) state = LevelState.Locked;
            else state = SaveSystem.IsCompleted(data.levelID) ? LevelState.Completed : LevelState.Open;

            buttons[i].SetState(state);
        }
    }

    /// Khung chọn bắt đầu ở màn mở cao nhất (màn mới nhất người chơi tới được).
    int FindStartIndex()
    {
        for (int i = buttons.Count - 1; i >= 0; i--)
        {
            if (buttons[i].IsSelectable) return i;
        }
        return 0;
    }

    // ================= Điều khiển =================

    void Update()
    {
        if (Time.frameCount == enabledFrame || buttons.Count == 0) return;
        if (GameManager.Instance != null && GameManager.Instance.State != GamePhase.Menu) return;

        if (Pressed(KeyCode.W, KeyCode.UpArrow)) TryMove(-columns);
        else if (Pressed(KeyCode.S, KeyCode.DownArrow)) TryMove(columns);
        else if (Pressed(KeyCode.A, KeyCode.LeftArrow)) TryMove(-1);
        else if (Pressed(KeyCode.D, KeyCode.RightArrow)) TryMove(1);
        else if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)) EnterSelected();
    }

    static bool Pressed(KeyCode a, KeyCode b)
    {
        return Input.GetKeyDown(a) || Input.GetKeyDown(b);
    }

    /// Dịch khung chọn theo thứ tự đọc: trái/phải ±1 (hết hàng thì xuống hàng kế), lên/xuống ±số cột.
    /// Chỉ di chuyển nếu ô đích tồn tại và đã mở.
    void TryMove(int delta)
    {
        int target = selected + delta;
        if (target < 0 || target >= buttons.Count) return;
        if (!buttons[target].IsSelectable) return;

        selected = target;
        MoveCursor();
        if (AudioManager.Instance != null) AudioManager.Instance.Play(Sfx.Click);
    }

    void EnterSelected()
    {
        if (!buttons[selected].IsSelectable) return;
        UIManager.Instance.StartLevel(buttons[selected].Index);
    }

    // ================= Tiện ích =================

    void MoveCursor()
    {
        if (cursor != null) cursor.MoveTo(CellOf(selected));
    }

    /// Ô grid của màn thứ index: đi sang phải theo cột, hết hàng thì xuống hàng dưới (y giảm).
    Vector2Int CellOf(int index)
    {
        return new Vector2Int(firstCell.x + index % columns, firstCell.y - index / columns);
    }
}
