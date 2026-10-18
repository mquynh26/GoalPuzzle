using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSelectUI : MonoBehaviour
{
    public static event Action<int> LevelSelected;
    public LevelButton buttonPrefab;
    public LevelCursor cursor;
    [Tooltip("10 sprite chữ số theo đúng thứ tự 0, 1, 2, ... 9")]
    public Sprite[] digits = new Sprite[10];
    
    public Vector2Int firstCell = new Vector2Int(2, 4);
    public int columns = 10;

    readonly List<LevelButton> buttons = new List<LevelButton>();
    int select = 0;
    int currentSelected = -1;
    int enabledFrame = -1;

    // Mở / làm mới
    void Awake()
    {
        LevelManager.LevelIndex += SetCurrentIndex;
    }

    void OnDestroy()
    {
        LevelManager.LevelIndex -= SetCurrentIndex;
    }
    
    void OnEnable()
    {
        enabledFrame = Time.frameCount;   // bỏ qua phím của chính khung hình vừa mở bảng
        if (!Prepare()) return;
        RefreshStates();
        if (currentSelected >= 0 && currentSelected < buttons.Count)
        {
            select = currentSelected;
        } else select = StartIndex();
        MoveCursor();
    }

    // Dựng các ô số theo đúng số màn trong database
    bool Prepare()
    {
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

    // Cập nhật trạng thái từng ô khóa / mở / đã xong
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

    // Khung chọn bắt đầu ở màn mở cao nhất
    int StartIndex()
    {
        for (int i = buttons.Count - 1; i >= 0; i--)
        {
            if (buttons[i].IsSelectable) return i;
        }
        return 0;
    }

    // Điều khiển

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
    
    void TryMove(int delta)
    {
        int target = select + delta;
        if (target < 0 || target >= buttons.Count) return;
        if (!buttons[target].IsSelectable) return;

        select = target;
        MoveCursor();
    }

    void EnterSelected()
    {
        if (!buttons[select].IsSelectable) return;
        UIManager.Instance.StartLevel(buttons[select].Index);
    }

    void MoveCursor()
    {
        if (cursor != null) cursor.MoveTo(CellOf(select));
        AudioManager.Play(Sfx.MenuMove);
        LevelSelected?.Invoke(select + 1);
    }
    
    Vector2Int CellOf(int index)
    {
        return new Vector2Int(firstCell.x + index % columns, firstCell.y - index / columns);
    }

    void SetCurrentIndex(int index)
    {
        currentSelected = index - 1;
    }
}
