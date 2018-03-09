using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FlagCustomizerUI : TitleSlaveUI
{
    public FlagEditingAreaUIAgent editingArea;
    public Toggle fillModeToggle;
    public Color selectedColor;
    public Image[] colorPalette;
    public int successiveFills;

    protected override void SetState(UIState state)
    {
        base.SetState(state);
        SetInteractable((int)state >= 2);
    }
    protected override void ProcessInput()
    {
        base.ProcessInput();
        if (fillModeToggle.isOn)
        {
            if (endPress)
            {
                if (editingArea.TryFill(selectedColor, currentInputPosition.screen) && successiveFills < 1)
                {
                    fillModeToggle.isOn = false;
                    successiveFills++;
                };
            }
        }
        else
        {
            if (pressed)
            {
                if (editingArea.TryPaint(selectedColor, currentInputPosition.screen))
                {
                    successiveFills = 0;
                }
            }
        }
    }

    void SaveFlag()
    {
        float[,,] flag = new float[editingArea.flagPixels.GetLength(0), editingArea.flagPixels.GetLength(1), 3];
        for (int x = 0; x < flag.GetLength(0); x++)
        {
            for (int y = 0; y < flag.GetLength(1); y++)
            {
                Color color = editingArea.flagPixels[x, y].color;
                flag[x, y, 0] = color.r;
                flag[x, y, 1] = color.g;
                flag[x, y, 2] = color.b;
            }
        }

        BattleTweakerUI.flags[editingArea.currentPlayer] = flag;
    }

    public override void Next()
    {
        SaveFlag();
        if (editingArea.currentPlayer == 1 || BattleTweakerUI.aiOpponent) base.Next();
        else
        {
            editingArea.currentPlayer++;
            editingArea.ReconstructFlag();
        }
    }

    public override void Previous()
    {
        SaveFlag();
        if (editingArea.currentPlayer == 0) base.Previous();
        else
        {
            editingArea.currentPlayer--;
            editingArea.ReconstructFlag();
        }
    }

    public void SelectColor(int color)
    {
        selectedColor = colorPalette[color].color;
    }
}
