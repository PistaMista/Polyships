using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ExclusiveTogglingButtonGroup : MonoBehaviour
{
    public GameObject[] callbackObjects;
    public string[] callbackMethodNames;
    public Button[] buttons;
    ColorBlock[] defaultButtonColors;

    void Awake()
    {
        defaultButtonColors = new ColorBlock[buttons.Length];
        for (int i = 0; i < buttons.Length; i++)
        {
            defaultButtonColors[i] = buttons[i].colors;
        }
    }

    public void OnPressButton(int buttonID)
    {
        ResetColors(buttonID);

        for (int i = 0; i < callbackObjects.Length; i++)
        {
            callbackObjects[i].SendMessage(callbackMethodNames[i], buttonID);
        }
    }

    public void ResetColors()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].colors = defaultButtonColors[i];
        }
    }

    public void ResetColors(int exclude)
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            if (i == exclude)
            {
                ColorBlock block = defaultButtonColors[i];
                block.disabledColor = defaultButtonColors[i].pressedColor;
                block.highlightedColor = defaultButtonColors[i].pressedColor;
                block.normalColor = defaultButtonColors[i].pressedColor;

                buttons[i].colors = block;
            }
            else
            {
                buttons[i].colors = defaultButtonColors[i];
            }
        }
    }
}
