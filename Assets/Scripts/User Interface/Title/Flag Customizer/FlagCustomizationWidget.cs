using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlagCustomizationWidget : MonoBehaviour
{

    public int position;
    public RectTransform rect;

    void Start()
    {
        rect.anchoredPosition = Vector2.right * position * Screen.width;
    }
}
