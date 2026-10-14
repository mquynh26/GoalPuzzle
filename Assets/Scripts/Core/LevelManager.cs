using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }
    
    public LevelDatabase database;
    public int startLevelIndex = 0;
    public bool autoLoadOnStart = true;
    
    public PlayerCharacter playerPrefab;
    public Ball ballPrefab;
    public Goal goalPrefab;
    public Fence fencePrefab;
    public TrainingCone conePrefab;
    public NPC npcPrefab;
    
    public Transform objectsParent;               // Grid/Objects
    public PlayerController playerController;     // GameSystems/PlayerController

    public int CurrentIndex { get; private set; } = -1;
    public LevelData CurrentData { get; private set; }
    public int CurrentLevelID => CurrentData != null ? CurrentData.levelID : 0;
    public bool HasNext => database != null && database.HasLevel(CurrentIndex + 1);

    PlayerCharacter player;
    Ball ball;
    readonly List<NPC> npcs = new List<NPC>();
    readonly List<TrainingCone> cones = new List<TrainingCone>();
    readonly List<Vector2Int> goalCenters = new List<Vector2Int>();
    readonly Dictionary<string, Transform> groups = new Dictionary<string, Transform>();

    void Awake()
    {
        Instance = this;
    }

    void OnEnable()  { TrainingCone.Disappeared += OnConeGone; }
    void OnDisable() { TrainingCone.Disappeared -= OnConeGone; }

    void OnConeGone(TrainingCone c) { cones.Remove(c); }

    void Start()
    {
        if (autoLoadOnStart) LoadLevel(startLevelIndex);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) RestartLevel();
        if (Input.GetKeyDown(KeyCode.N)) NextLevel();
        if (Input.GetKeyDown(KeyCode.F12)) { SaveSystem.ResetProgress(); Debug.Log("Đã xóa tiến trình"); }
        if (Input.GetKeyDown(KeyCode.M)) { GameSettings.ToggleSound(); Debug.Log("Âm thanh: " + GameSettings.IsSoundEnabled); }
        if (Input.GetKeyDown(KeyCode.F11)) { GameSettings.ToggleFullScreen(); Debug.Log("Toàn màn hình: " + GameSettings.IsFullScreen); }
    }

    // Nạp màn theo index trong database (0-based). Trả về false nếu thất bại.
    public bool LoadLevel(int index)
    {
        if (!ValidateReferences()) return false;

        LevelData data = database.GetData(index);
        if (data == null)
        {
            return false;
        }
        return Build(index, data);
    }

    public void RestartLevel()
    {
        if (CurrentIndex >= 0) LoadLevel(CurrentIndex);
    }

    // Sang màn kế tiếp, trả về false nếu đã hết màn.
    public bool NextLevel()
    {
        return HasNext && LoadLevel(CurrentIndex + 1);
    }
    
    public void UnloadLevel()
    {
        playerController.ResetState();
        ClearObjects();
        CurrentIndex = -1;
        CurrentData = null;
    }

    bool Build(int index, LevelData data)
    {
        playerController.ResetState();   // dừng bóng đang lăn, mở khóa input
        ClearObjects();

        CurrentIndex = index;
        CurrentData = data;

        GridManager g = GridManager.Instance;
        g.Init(data.gridSize);

        for (int y = 0; y < data.gridSize.y; y++)
        {
            for (int x = 0; x < data.gridSize.x; x++)
            {
                var cell = new Vector2Int(x, y);
                string suffix = "_" + x + "_" + y;

                switch (data.Get(x, y))
                {
                    case CellType.Player:
                        player = Spawn(playerPrefab, cell, objectsParent, "Player");
                        player.Face(Direction.Down);
                        player.Effect();
                        break;

                    case CellType.Ball:
                        ball = Spawn(ballPrefab, cell, objectsParent, "Ball");
                        ball.Effect();
                        break;

                    case CellType.NPC:
                    {
                        NPC npc = Spawn(npcPrefab, cell, Group("NPCs"), "NPC" + suffix);
                        npc.SetFacing(Direction.Up);   // hướng ban đầu mặc định
                        npcs.Add(npc);
                        npc.Effect();
                        break;
                    }
                    case CellType.Fence:
                        Fence fence = Spawn(fencePrefab, cell, Group("Fences"), "Fence" + suffix);
                        fence.Effect();
                        break;

                    case CellType.Cone:
                        TrainingCone cone = Spawn(conePrefab, cell, Group("Cones"), "Cone" + suffix);
                        cones.Add(cone);
                        cone.Effect();
                        break;

                    case CellType.Goal:
                        goalCenters.Add(cell);
                        break;
                }
            }
        }

        BuildGoals();

        if (player == null || ball == null)
        {
            Debug.LogError("LevelManager: màn " + data.name + " thiếu Player hoặc Ball!");
            return false;
        }

        playerController.player = player;
        playerController.ball = ball;
        
        //Undo
        UndoManager.Instance.capture = Capture;
        UndoManager.Instance.apply = ApplyState;
        UndoManager.Instance.Clear();

        if (GameManager.Instance != null) GameManager.Instance.SetPhase(GamePhase.Playing);
        return true;
    }
    
    void BuildGoals()
    {
        GridManager g = GridManager.Instance;

        foreach (Vector2Int center in goalCenters)
        {
            for (int dx = -GoalLayout.HalfWidth; dx <= GoalLayout.HalfWidth; dx++)
            {
                var cell = new Vector2Int(center.x + dx, center.y);
                string suffix = "_" + cell.x + "_" + cell.y;

                if (!g.InBounds(cell))
                {
                    Debug.LogWarning("Gôn ở " + center + " tràn ra ngoài lưới, bỏ qua ô " + cell);
                    continue;
                }

                if (dx != 0 && g.GetAt(cell) != null)
                {
                    Debug.LogWarning("Ô " + cell + " nằm trong vùng 5 ô của gôn nhưng đang có vật thể khác.");
                    continue;
                }

                if (GoalLayout.IsPostOffset(dx))
                {
                    CreateInvisible<Fence>(cell, "GoalPost" + suffix, true);
                }
                else if (dx == 0)
                {
                    Spawn(goalPrefab, cell, Group("Goals"), "Goal" + suffix, false);
                    g.AddGoal(cell);
                }
                else
                {
                    CreateInvisible<Goal>(cell, "Goal" + suffix, false);
                    g.AddGoal(cell);
                }
            }
        }
    }

    // Tạo vật thể trên lưới không có hình
    T CreateInvisible<T>(Vector2Int cell, string objName, bool block) where T : GridObject
    {
        var go = new GameObject(objName);
        go.transform.SetParent(Group("Goals"), false);

        T obj = go.AddComponent<T>();
        obj.posIndex = cell;
        obj.Show();                                   
        if (block) GridManager.Instance.Place(obj);
        return obj;
    }

    void ClearObjects()
    {
        for (int i = objectsParent.childCount - 1; i >= 0; i--)
        {
            Destroy(objectsParent.GetChild(i).gameObject);
        }
        groups.Clear();
        npcs.Clear();
        cones.Clear();
        goalCenters.Clear();
        player = null;
        ball = null;
    }

    // Nhóm con của Objects
    Transform Group(string name)
    {
        if (!groups.TryGetValue(name, out Transform t) || t == null)
        {
            var go = new GameObject(name);
            t = go.transform;
            t.SetParent(objectsParent, false);
            groups[name] = t;
        }
        return t;
    }

    T Spawn<T>(T prefab, Vector2Int cell, Transform parent, string objName, bool block = true) where T : GridObject
    {
        T obj = Instantiate(prefab, parent);
        obj.name = objName;
        obj.posIndex = cell;
        obj.Show();
        if (block) GridManager.Instance.Place(obj);
        return obj;
    }

    //Undo
    public GameState Capture()
    {
        var s = new GameState();
        s.playerPos = player.posIndex;
        s.playerFacing = player.facingDir;
        s.ballPos = ball.posIndex;
        foreach (NPC n in npcs) s.npcs.Add(new NPCState { pos = n.posIndex, facing = n.facing });
        foreach (TrainingCone c in cones) s.cones.Add(c.posIndex);
        return s;
    }

    // Khôi phục về trạng thái đã cap
    public void ApplyState(GameState s)
    {
        GridManager g = GridManager.Instance;

        // gỡ mọi vật thể thay đổi được khỏi bảng ô bị chiếm
        g.Remove(player);
        g.Remove(ball);
        foreach (NPC n in npcs) g.Remove(n);
        foreach (TrainingCone c in cones) { g.Remove(c); Destroy(c.gameObject); }
        cones.Clear();

        // Đặt lại vị trí và hướng
        player.posIndex = s.playerPos;
        player.Face(s.playerFacing);
        g.Place(player);
        player.Show();

        ball.posIndex = s.ballPos;
        g.Place(ball);
        ball.Show();

        for (int i = 0; i < npcs.Count; i++)
        {
            npcs[i].posIndex = s.npcs[i].pos;
            npcs[i].SetFacing(s.npcs[i].facing);
            g.Place(npcs[i]);
            npcs[i].Show();
        }

        //Tạo lại các cọc còn lại
        foreach (Vector2Int pos in s.cones)
        {
            cones.Add(Spawn(conePrefab, pos, Group("Cones"), "Cone_" + pos.x + "_" + pos.y));
        }
    }

    //Kiểm tra tham chiếu
    bool ValidateReferences()
    {
        bool ok = true;
        void Need(UnityEngine.Object o, string name)
        {
            if (o == null) { Debug.LogError("LevelManager: chưa gán / chưa có " + name); ok = false; }
        }

        Need(database, "Database (LevelDatabase)");
        Need(objectsParent, "Objects Parent (Grid/Objects)");
        Need(playerController, "Player Controller (GameSystems/PlayerController)");
        Need(playerPrefab, "Player Prefab");
        Need(ballPrefab, "Ball Prefab");
        Need(goalPrefab, "Goal Prefab");
        Need(fencePrefab, "Fence Prefab");
        Need(conePrefab, "Cone Prefab");
        Need(npcPrefab, "NPC Prefab");
        Need(GridManager.Instance, "GridManager (gắn lên object Grid)");
        Need(RuleChecker.Instance, "RuleChecker (gắn lên GameSystems/RuleChecker)");
        Need(UndoManager.Instance, "UndoManager (gắn lên GameSystems/UndoManager)");
        return ok;
    }
}
