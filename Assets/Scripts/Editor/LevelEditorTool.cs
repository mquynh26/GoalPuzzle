using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class LevelEditorTool : EditorWindow
{
    // Kích thước sân. Đổi ở đây nếu nền sân đổi (phải trùng Grid Size và Map Size ở CameraFitter).
    static readonly Vector2Int FixedSize = new Vector2Int(15, 8);

    const float CellPx = 44f;

    [SerializeField] LevelData level;
    [SerializeField] LevelDatabase database;
    CellType brush = CellType.Fence;
    Direction npcDir = Direction.Up;   // hướng NPC sẽ được vẽ ra
    Vector2 scroll;
    GUIStyle cellStyle;

    static readonly string[] DirNames = { "↑ Lên", "→ Phải", "↓ Xuống", "← Trái" };   // khớp thứ tự enum Direction
    static readonly string[] DirArrows = { "↑", "→", "↓", "←" };

    static readonly string[] BrushNames = { "Trống", "Người", "NPC", "Bóng", "Rào/Cột", "Cọc", "Gôn" };
    // Thứ tự khớp giá trị enum CellType: Empty=0, Player=1, NPC=2, Ball=3, Fence=4, Cone=5, Goal=6

    [MenuItem("Tools/Level Editor")]
    static void Open()
    {
        GetWindow<LevelEditorTool>("Level Editor");
    }

    void OnGUI()
    {
        if (cellStyle == null)
        {
            cellStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 16
            };
        }

        DrawTop();

        if (level == null)
        {
            EditorGUILayout.HelpBox("Chọn một LevelData ở trên (EditLevel) hoặc bấm 'Tạo màn mới' (CreateLevel).", MessageType.Info);
            return;
        }

        EditorGUILayout.Space();
        DrawLevelInfo();
        EditorGUILayout.Space();

        brush = (CellType)GUILayout.Toolbar((int)brush, BrushNames, GUILayout.Height(28));

        // Chọn NPC thì hiện thêm thanh chọn hướng. Vẽ đè lên NPC có sẵn sẽ đổi hướng của nó
        if (brush == CellType.NPC)
        {
            npcDir = (Direction)GUILayout.Toolbar((int)npcDir, DirNames, GUILayout.Height(24));
        }
        EditorGUILayout.LabelField("Chuột trái: vẽ  |  Chuột phải: xóa  |  Hàng trên cùng của bảng là y lớn nhất", EditorStyles.miniLabel);

        scroll = EditorGUILayout.BeginScrollView(scroll);
        DrawGrid();
        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();
        DrawValidation();
        DrawBottom();
    }

    // ---------- Phần đầu: chọn / tạo màn ----------

    void DrawTop()
    {
        EditorGUILayout.Space(4);

        level = (LevelData)EditorGUILayout.ObjectField("Màn đang sửa", level, typeof(LevelData), false);
        database = (LevelDatabase)EditorGUILayout.ObjectField("Database", database, typeof(LevelDatabase), false);

        if (GUILayout.Button("Tạo màn mới")) CreateLevel();
    }

    void CreateLevel()
    {
        int nextId = database != null ? database.Count + 1 : 1;
        string path = EditorUtility.SaveFilePanelInProject(
            "Tạo màn mới", "Level_" + nextId.ToString("00"), "asset", "Chọn nơi lưu màn chơi", "Assets/Levels");
        if (string.IsNullOrEmpty(path)) return;

        var data = CreateInstance<LevelData>();
        data.levelID = nextId;
        data.gridSize = FixedSize;
        data.gridElements = new List<int>();
        data.EnsureSize();

        AssetDatabase.CreateAsset(data, path);
        AssetDatabase.SaveAssets();

        level = data;
        AddToDatabase();
        Selection.activeObject = data;
    }

    // ---------- Thông tin màn ----------

    void DrawLevelInfo()
    {
        EditorGUI.BeginChangeCheck();
        int id = EditorGUILayout.IntField("Level ID", level.levelID);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(level, "Đổi Level ID");
            level.levelID = Mathf.Max(1, id);
            EditorUtility.SetDirty(level);
        }

        if (level.gridSize != FixedSize)
        {
            EditorGUILayout.HelpBox(
                "Màn này có kích thước " + level.gridSize.x + "x" + level.gridSize.y +
                ", khác kích thước sân cố định " + FixedSize.x + "x" + FixedSize.y + ".",
                MessageType.Warning);

            if (GUILayout.Button("Đổi về kích thước sân")) ResizeToFixed();
        }
    }

    void ResizeToFixed()
    {
        Undo.RecordObject(level, "Đổi kích thước");

        Vector2Int oldSize = level.gridSize;
        var old = new CellType[oldSize.x, oldSize.y];
        for (int y = 0; y < oldSize.y; y++)
            for (int x = 0; x < oldSize.x; x++)
                old[x, y] = level.Get(x, y);

        var oldFacing = new Direction[oldSize.x, oldSize.y];
        for (int y = 0; y < oldSize.y; y++)
            for (int x = 0; x < oldSize.x; x++)
                oldFacing[x, y] = level.GetNpcFacing(x, y);

        level.gridSize = FixedSize;
        level.gridElements = new List<int>();
        level.npcFacings = new List<int>();
        level.EnsureSize();

        for (int y = 0; y < Mathf.Min(oldSize.y, FixedSize.y); y++)
        {
            for (int x = 0; x < Mathf.Min(oldSize.x, FixedSize.x); x++)
            {
                level.Set(x, y, old[x, y]);
                level.SetNpcFacing(x, y, oldFacing[x, y]);
            }
        }

        EditorUtility.SetDirty(level);
    }

    // ---------- Lưới vẽ ----------

    void DrawGrid()
    {
        Vector2Int size = level.gridSize;
        level.EnsureSize();

        // Bản hiển thị: ô Gôn vẽ 1 ô sẽ được LevelManager mở rộng thành 5 ô ngang
        var view = new CellType[size.x, size.y];
        var ghost = new bool[size.x, size.y];
        for (int y = 0; y < size.y; y++)
            for (int x = 0; x < size.x; x++)
                view[x, y] = level.Get(x, y);

        for (int y = 0; y < size.y; y++)
        {
            for (int x = 0; x < size.x; x++)
            {
                if (level.Get(x, y) != CellType.Goal) continue;

                for (int dx = -GoalLayout.HalfWidth; dx <= GoalLayout.HalfWidth; dx++)
                {
                    int nx = x + dx;
                    if (dx == 0 || nx < 0 || nx >= size.x) continue;
                    if (level.Get(nx, y) != CellType.Empty) continue;

                    view[nx, y] = GoalLayout.IsPostOffset(dx) ? CellType.Fence : CellType.Goal;
                    ghost[nx, y] = true;
                }
            }
        }

        Rect area = GUILayoutUtility.GetRect(size.x * CellPx, size.y * CellPx);

        for (int y = 0; y < size.y; y++)
        {
            for (int x = 0; x < size.x; x++)
            {
                // Hàng y lớn nhất vẽ ở trên cùng
                var r = new Rect(area.x + x * CellPx, area.y + (size.y - 1 - y) * CellPx, CellPx - 2, CellPx - 2);
                CellType t = view[x, y];

                Color col = ColorOf(t);
                if (ghost[x, y]) col.a = 0.45f;
                EditorGUI.DrawRect(r, col);

                string label = LabelOf(t);
                if (t == CellType.NPC && !ghost[x, y]) label = DirArrows[(int)level.GetNpcFacing(x, y)];   // NPC hiện mũi tên hướng nhìn
                if (ghost[x, y]) label = label.ToLower();
                GUI.Label(r, label, cellStyle);
            }
        }

        // Chuột
        Event e = Event.current;
        if ((e.type == EventType.MouseDown || e.type == EventType.MouseDrag) &&
            (e.button == 0 || e.button == 1) && area.Contains(e.mousePosition))
        {
            int x = (int)((e.mousePosition.x - area.x) / CellPx);
            int row = (int)((e.mousePosition.y - area.y) / CellPx);
            int y = size.y - 1 - row;

            if (x >= 0 && x < size.x && y >= 0 && y < size.y)
            {
                Paint(x, y, e.button == 1 ? CellType.Empty : brush);
                e.Use();
                Repaint();
            }
        }
    }

    void Paint(int x, int y, CellType type)
    {
        bool sameType = level.Get(x, y) == type;
        bool sameFacing = type != CellType.NPC || level.GetNpcFacing(x, y) == npcDir;
        if (sameType && sameFacing) return;

        Undo.RecordObject(level, "Vẽ ô");

        // Người và Bóng mỗi màn chỉ có một: vẽ ô mới thì xóa ô cũ
        if (type == CellType.Player || type == CellType.Ball) ClearAll(type);

        level.Set(x, y, type);
        if (type == CellType.NPC) level.SetNpcFacing(x, y, npcDir);
        EditorUtility.SetDirty(level);
    }

    void ClearAll(CellType type)
    {
        for (int y = 0; y < level.gridSize.y; y++)
            for (int x = 0; x < level.gridSize.x; x++)
                if (level.Get(x, y) == type) level.Set(x, y, CellType.Empty);
    }

    static Color ColorOf(CellType t)
    {
        switch (t)
        {
            case CellType.Player: return new Color(0.25f, 0.55f, 1f);
            case CellType.NPC:    return new Color(0.9f, 0.25f, 0.25f);
            case CellType.Ball:   return new Color(0.95f, 0.95f, 0.95f);
            case CellType.Fence:  return new Color(0.45f, 0.45f, 0.45f);
            case CellType.Cone:   return new Color(1f, 0.6f, 0.1f);
            case CellType.Goal:   return new Color(0.2f, 0.75f, 0.3f);
            default:              return new Color(0.18f, 0.28f, 0.18f);
        }
    }

    static string LabelOf(CellType t)
    {
        switch (t)
        {
            case CellType.Player: return "P";
            case CellType.NPC:    return "N";
            case CellType.Ball:   return "B";
            case CellType.Fence:  return "F";
            case CellType.Cone:   return "C";
            case CellType.Goal:   return "G";
            default:              return "";
        }
    }

    // ---------- Kiểm tra màn ----------

    void DrawValidation()
    {
        int players = Count(CellType.Player);
        int balls = Count(CellType.Ball);
        int goals = Count(CellType.Goal);
        Vector2Int size = level.gridSize;

        if (players != 1) EditorGUILayout.HelpBox("Cần đúng 1 Người (đang có " + players + ").", MessageType.Error);
        if (balls != 1) EditorGUILayout.HelpBox("Cần đúng 1 Bóng (đang có " + balls + ").", MessageType.Error);
        if (goals < 1) EditorGUILayout.HelpBox("Cần ít nhất 1 ô Gôn: vẽ 1 ô ở giữa gôn, 2 ô mỗi bên sẽ tự thành cột và ô ghi bàn.", MessageType.Error);

        for (int y = 0; y < size.y; y++)
        {
            for (int x = 0; x < size.x; x++)
            {
                if (level.Get(x, y) == CellType.Goal)
                {
                    int half = GoalLayout.HalfWidth;
                    if (x - half < 0 || x + half >= size.x)
                    {
                        EditorGUILayout.HelpBox("Gôn ở (" + x + "," + y + ") tràn ra ngoài lưới: cần cách mép trái và phải ít nhất " + half + " ô.", MessageType.Error);
                    }
                    else
                    {
                        for (int dx = -half; dx <= half; dx++)
                        {
                            if (dx != 0 && level.Get(x + dx, y) != CellType.Empty)
                                EditorGUILayout.HelpBox("Ô (" + (x + dx) + "," + y + ") nằm trong vùng 5 ô của gôn (" + x + "," + y + ") nhưng không trống.", MessageType.Warning);
                        }
                    }
                }

                // Bóng nằm ngay phía trước mặt NPC thì sẽ bị đá khi người chơi xoay NPC
                if (level.Get(x, y) == CellType.NPC)
                {
                    Direction f = level.GetNpcFacing(x, y);
                    Vector2Int front = new Vector2Int(x, y) + f.ToVector();
                    if (front.x >= 0 && front.x < size.x && front.y >= 0 && front.y < size.y
                        && level.Get(front.x, front.y) == CellType.Ball)
                    {
                        EditorGUILayout.HelpBox("Bóng đang nằm ngay phía trước NPC ở (" + x + "," + y + "), NPC đang nhìn " + DirNames[(int)f] + ".", MessageType.Warning);
                    }
                }
            }
        }
    }

    int Count(CellType type)
    {
        int n = 0;
        for (int y = 0; y < level.gridSize.y; y++)
            for (int x = 0; x < level.gridSize.x; x++)
                if (level.Get(x, y) == type) n++;
        return n;
    }

    //Lưu xuất

    void DrawBottom()
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Xóa hết")) ClearBoard();
            if (GUILayout.Button("Lưu (ExportLevelData)")) ExportLevelData();
        }

        using (new EditorGUI.DisabledScope(database == null))
        {
            if (GUILayout.Button("Đặt làm màn khởi động khi bấm Play")) SetAsStartLevel();
        }
    }

    void ClearBoard()
    {
        if (!EditorUtility.DisplayDialog("Xóa hết", "Xóa toàn bộ ô của màn này?", "Xóa", "Hủy")) return;

        Undo.RecordObject(level, "Xóa hết");
        for (int i = 0; i < level.gridElements.Count; i++) level.gridElements[i] = (int)CellType.Empty;
        for (int i = 0; i < level.npcFacings.Count; i++) level.npcFacings[i] = (int)Direction.Up;
        EditorUtility.SetDirty(level);
    }

    void ExportLevelData()
    {
        level.EnsureSize();
        EditorUtility.SetDirty(level);
        AddToDatabase();
        AssetDatabase.SaveAssets();
        Debug.Log("Đã lưu màn " + level.name + " (ID " + level.levelID + ")");
    }

    void AddToDatabase()
    {
        if (database == null || level == null) return;
        if (database.levels.Contains(level)) return;

        Undo.RecordObject(database, "Thêm màn vào database");
        database.levels.Add(level);
        EditorUtility.SetDirty(database);
    }

    //bấm Play là vào đúng màn đang sửa
    void SetAsStartLevel()
    {
        AddToDatabase();

        LevelManager lm = FindObjectOfType<LevelManager>();
        if (lm == null)
        {
            Debug.LogWarning("Không thấy LevelManager trong scene đang mở.");
            return;
        }

        Undo.RecordObject(lm, "Đặt màn khởi động");
        lm.database = database;
        lm.startLevelIndex = database.levels.IndexOf(level);
        EditorUtility.SetDirty(lm);
        Debug.Log("Màn khởi động = index " + lm.startLevelIndex + " (" + level.name + ")");
    }
}