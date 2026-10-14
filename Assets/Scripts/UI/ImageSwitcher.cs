using UnityEngine;
using UnityEngine.UI;

public class ImageSwitcher : MonoBehaviour
{
    public Image targetImage;
    public Sprite[] images;
    public float switchInterval = 2f;

    int currentIndex;

    // Dùng OnEnable (không phải Start) vì panel bị SetActive(false) rồi bật lại nhiều lần
    void OnEnable()
    {
        if (targetImage == null || images == null || images.Length < 2) return;

        currentIndex = 0;
        targetImage.sprite = images[0];
        InvokeRepeating(nameof(SwitchImage), switchInterval, switchInterval);
    }

    void OnDisable()
    {
        CancelInvoke(nameof(SwitchImage));
    }

    void SwitchImage()
    {
        currentIndex = (currentIndex + 1) % images.Length;
        targetImage.sprite = images[currentIndex];
    }
}