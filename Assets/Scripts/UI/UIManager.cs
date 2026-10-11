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

        // Các phím tắt cài đặt và hướng dẫn có thể sử dụng ở bất cứ đâu
        if (Input.GetKeyDown(KeyCode.M))
            SetSound(!GameSettings.IsSoundEnabled);

        if (Input.GetKeyDown(KeyCode.F))
            SetFullScreen(!GameSettings.IsFullScreen);

        if (Input.GetKeyDown(KeyCode.G))
            OpenGuide();
        
        
        // Esc: đóng bảng hướng dẫn đang mở
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Q))
        {
            if (IsOpen(guidePanel))
            {
                CloseGuide();
                return;
            }

            // Đang ở màn chọn màn: quay về menu chính
            if (p == GamePhase.Menu && IsOpen(levelSelectPanel))
            {
                PlayClick();
                ShowMainMenu();
                return;
            }

            // Đang chơi: quay về màn chọn màn
            if (p == GamePhase.Playing)
            {
                GoToLevelSelect();
                return;
            }
        }
        
        // Menu chính: nhấn Space để vào chọn màn
        if (p == GamePhase.Menu && IsOpen(mainMenuPanel))
        {
            if (Input.GetKeyDown(KeyCode.Space))
                OpenLevelSelect();

            return;
        }
    }

    // ================= Theo trạng thái game =================

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

    // ================= Menu chính / chọn màn =================

    public void ShowMainMenu()
    {
        HideOverlays();
        LevelManager.Instance.UnloadLevel();
        GameManager.Instance.SetPhase(GamePhase.Menu);
        Show(mainMenuPanel, true);
        Show(levelSelectPanel, false);
    }

    /// Nút "Chơi" ở menu chính.
    public void OpenLevelSelect()
    {
        PlayClick();
        Show(mainMenuPanel, false);
        Show(levelSelectPanel, true);   // LevelSelectUI tự dựng lại danh sách nút khi panel bật
    }

    /// Nút "Thoát màn" ở bảng tạm dừng, nút Exit trên HUD, và khi hết màn.
    public void GoToLevelSelect()
    {
        HideOverlays();
        LevelManager.Instance.UnloadLevel();
        GameManager.Instance.SetPhase(GamePhase.Menu);
        Show(mainMenuPanel, false);
        Show(levelSelectPanel, true);
    }

    /// LevelButton gọi hàm này. index là vị trí màn trong database (0-based).
    public void StartLevel(int index)
    {
        LevelData data = LevelManager.Instance.database.GetData(index);
        if (data == null) return;

        if (!SaveSystem.IsUnlocked(data.levelID))
        {
            return;
        }

        PlayClick();
        LevelManager.Instance.LoadLevel(index);   // nạp xong tự chuyển sang Playing
    }

    // ================= Hướng dẫn / cài đặt =================

    public void OpenGuide()
    {
        PlayClick(); // đang xem thì không chơi được
        Show(guidePanel, true);
    }

    public void CloseGuide()
    {
        PlayClick();
        Show(guidePanel, false);
    }

    /// Gán vào Toggle Âm thanh: OnValueChanged -> Dynamic bool.
    public void SetSound(bool on)
    {
        GameSettings.SetSoundEnabled(on);
        PlayClick();
    }

    /// Gán vào Toggle Toàn màn hình: OnValueChanged -> Dynamic bool.
    public void SetFullScreen(bool on)
    {
        GameSettings.SetFullScreen(on);
        PlayClick();
    }
    
    // ================= Tiện ích =================

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

    static void PlayClick()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.Play(Sfx.Click);
    }
}
