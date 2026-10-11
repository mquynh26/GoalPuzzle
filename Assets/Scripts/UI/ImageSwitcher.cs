using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageSwitcher : MonoBehaviour
{
    public Image targetImage;
    public Sprite[] images;
    public float switchInterval = 2f;

    private int currentIndex = 0;

    void Start()
    {
        if (targetImage == null || images == null || images.Length < 2)
            return;

        targetImage.sprite = images[currentIndex];

        InvokeRepeating(nameof(SwitchImage), switchInterval, switchInterval);
    }

    void SwitchImage()
    {
        currentIndex = (currentIndex + 1) % 2;
        targetImage.sprite = images[currentIndex];
    }

    void OnDisable()
    {
        CancelInvoke(nameof(SwitchImage));
    }
}

