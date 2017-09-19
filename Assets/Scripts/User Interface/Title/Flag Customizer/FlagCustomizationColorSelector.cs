using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FlagCustomizationColorSelector : MonoBehaviour
{
    Color color;
    public Image indicator;
    public Button button;
    public Color Color
    {
        get { return color; }
        set
        {
            color = value;
            indicator.color = value;
        }
    }

    void Start()
    {
        color = indicator.color;
    }
    public void SetHighlight(bool enabled)
    {
        if (enabled)
        {
            ColorBlock block = button.colors;
            block.normalColor = GameLoaderUserInterface.buttonColors.pressedColor;
            block.highlightedColor = GameLoaderUserInterface.buttonColors.pressedColor;
            block.disabledColor = GameLoaderUserInterface.buttonColors.pressedColor;

            button.colors = block;
        }
        else
        {
            button.colors = GameLoaderUserInterface.buttonColors;
        }
    }
}
