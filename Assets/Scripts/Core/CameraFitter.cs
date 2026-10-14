using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFitter : MonoBehaviour
{
    public Renderer[] mapRenderers;
    public float trimTop = 0.5f;

    Camera cam;
    int lastW, lastH;
    Renderer floorRenderer;   // cache, tránh GameObject.Find mỗi lần

    void Awake()
    {
        cam = GetComponent<Camera>();
        cam.orthographic = true;
    }

    void Start()
    {
        Fit();
    }

    void Update()
    {
        // Đổi cỡ cửa sổ hoặc tỉ lệ thì canh lại
        if (Screen.width != lastW || Screen.height != lastH) Fit();
    }
    
    public void Fit()
    {
        // Cửa sổ bị thu nhỏ: kích thước = 0, bỏ qua để tránh chia cho 0
        if (Screen.width <= 0 || Screen.height <= 0) return;

        // Ghi nhớ kích thước ngay, để Update không gọi lại Fit mỗi frame khi không tìm thấy bounds
        lastW = Screen.width;
        lastH = Screen.height;

        if (!TryGetBounds(out Bounds b)) return;

        float aspect = (float)Screen.width / Screen.height;

        // Chiều cao 1 ô (lấy từ lưới, không có thì coi là 1)
        float cellH = (GridManager.Instance != null && GridManager.Instance.grid != null)
            ? GridManager.Instance.grid.cellSize.y
            : 1f;

        // Bỏ bớt mép trên
        Vector3 min = b.min;
        Vector3 max = b.max;
        max.y -= trimTop * cellH;

        Vector3 center = (min + max) / 2f;
        float halfW = (max.x - min.x) / 2f;
        float halfH = (max.y - min.y) / 2f;

        // Zoom vừa đủ để cả chiều ngang lẫn chiều dọc đều nằm trong màn hình
        cam.orthographicSize = Mathf.Max(halfH, halfW / aspect);
        cam.transform.position = new Vector3(center.x, center.y, cam.transform.position.z);
    }

    bool TryGetBounds(out Bounds bounds)
    {
        bounds = default;
        bool has = false;

        if (mapRenderers != null)
        {
            foreach (Renderer r in mapRenderers)
            {
                if (r == null) continue;
                if (!has) { bounds = r.bounds; has = true; }
                else bounds.Encapsulate(r.bounds);
            }
        }

        if (!has)
        {
            if (floorRenderer == null)
            {
                GameObject floor = GameObject.Find("Floor");
                if (floor != null) floor.TryGetComponent(out floorRenderer);
            }

            if (floorRenderer != null)
            {
                bounds = floorRenderer.bounds;
                has = true;
            }
        }
        return has;
    }
}