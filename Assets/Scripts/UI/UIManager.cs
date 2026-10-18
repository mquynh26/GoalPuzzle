using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    
    public GameObject mainMenuPanel;
    public GameObject levelSelectPanel;
    public GameObject hubBotton;
    public GameObject guidePanel;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        GameManager.Instance.PhaseChanged += ApplyPhase;

        Show(guidePanel, false);

        ApplyPhase(GameManager.Instance.State);
        if (GameManager.Instance.State == GamePhase.Menu) ShowMainMenu();
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null) GameManager.Instance.PhaseChanged -= ApplyPhase;
    }

    void Update()
    {
        GamePhase p = GameManager.Instance.State;
        
        if (Input.GetKeyDown(KeyCode.M)) GameSettings.CycleSound();

        if (Input.GetKeyDown(KeyCode.F)) SetFullScreen(!GameSettings.IsFullScreen);

        if (Input.GetKeyDown(KeyCode.G)) OpenGuide();
        
        
        // Esc: đóng bảng hướng dẫn đang mở
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Q))
        {
            if (IsOpen(guidePanel))
            {
                CloseGuide();
                return;
            }

            // Đang ở màn chọn màn thi quay về menu chính
            if (p == GamePhase.Menu && IsOpen(levelSelectPanel))
            {
                ShowMainMenu();
                return;
            }

            // Đang chơi thì quay về màn chọn màn
            if (p == GamePhase.Playing)
            {
                GoToLevelSelect();
                return;
            }
        }
        
        // Menu chính thì nhấn Space để vào chọn màn
        if (p == GamePhase.Menu && IsOpen(mainMenuPanel))
        {
            if (Input.GetKeyDown(KeyCode.Space))
                OpenLevelSelect();

            return;
        }
    }

    void ApplyPhase(GamePhase p)
    {
        bool inMenu = p == GamePhase.Menu;

        Show(hubBotton, inMenu);

        // Vào chơi thì ẩn menu và chọn màn
        if (!inMenu)
        {
            Show(mainMenuPanel, false);
            Show(levelSelectPanel, false);
        }
    }

    public void ShowMainMenu()
    {
        HideOverlays();
        LevelManager.Instance.UnloadLevel();
        GameManager.Instance.SetPhase(GamePhase.Menu);
        Show(mainMenuPanel, true);
        Show(levelSelectPanel, false);
    }
    
    public void OpenLevelSelect()
    {
        Show(mainMenuPanel, false);
        Show(levelSelectPanel, true);   // LevelSelectUI tự dựng lại danh sách nút khi panel bật
    }
    
    public void GoToLevelSelect()
    {
        HideOverlays();
        LevelManager.Instance.UnloadLevel();
        GameManager.Instance.SetPhase(GamePhase.Menu);
        Show(mainMenuPanel, false);
        Show(levelSelectPanel, true);
    }

    // LevelButton gọi hàm này index là vị trí màn trong database
    public void StartLevel(int index)
    {
        LevelData data = LevelManager.Instance.database.GetData(index);
        if (data == null) return;

        if (!SaveSystem.IsUnlocked(data.levelID))
        {
            return;
        }
        LevelManager.Instance.LoadLevel(index);   // nạp xong tự chuyển sang Playing
    }
    

    public void OpenGuide()
    {
        Show(guidePanel, true);
    }

    public void CloseGuide()
    {
        Show(guidePanel, false);
    }
    
    public void SetSound(bool on)
    {
        GameSettings.SetSoundEnabled(on);
    }
    
    public void SetFullScreen(bool on)
    {
        GameSettings.SetFullScreen(on);
    }

    void HideOverlays()
    {
        Show(guidePanel, false);
    }

    static void Show(GameObject go, bool on)
    {
        if (go != null) go.SetActive(on);
    }

    static bool IsOpen(GameObject go)
    {
        return go != null && go.activeSelf;
    }
    
}
