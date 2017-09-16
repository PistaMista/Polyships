using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameModeSelectorUserInterface : SlidingUserInterface
{
    public static int selectedMode;
    public Button[] modeSelectionButtons;
    public RectTransform battleResumerParent;
    public RectTransform battleCreatorParent;

    protected override void ChangeState(UIState state)
    {
        base.ChangeState(state);
        switch (state)
        {
            case UIState.ENABLING:
                if (selectedMode < 0)
                {
                    SlidingUserInterface_Master.lockedDirections[1] = true;
                }

                for (int i = 0; i < modeSelectionButtons.Length; i++)
                {
                    ColorBlock block = modeSelectionButtons[i].colors;
                    if (i == selectedMode)
                    {
                        block.normalColor = GameLoaderUserInterface.buttonColors.pressedColor;
                        block.highlightedColor = GameLoaderUserInterface.buttonColors.pressedColor;
                        block.disabledColor = GameLoaderUserInterface.buttonColors.pressedColor;
                    }
                    else
                    {
                        block = GameLoaderUserInterface.buttonColors;
                    }
                    modeSelectionButtons[i].colors = block;
                }

                if (GameLoaderUserInterface.saveSlotData.stage == BattleStage.NOT_INITIALIZED)
                {
                    battleCreatorParent.anchoredPosition = Vector2.down * 120;
                    battleResumerParent.gameObject.SetActive(false);
                }
                else
                {
                    battleCreatorParent.anchoredPosition = Vector2.zero;
                    battleResumerParent.gameObject.SetActive(true);
                }

                break;
            case UIState.DISABLING:
                SlidingUserInterface_Master.lockedDirections = new bool[2];
                break;
        }

    }

    public override void OnMasterEnable()
    {
        base.OnMasterEnable();
        selectedMode = -1;
    }

    public override void OnMasterDisable()
    {
        base.OnMasterDisable();
        for (int i = 0; i < modeSelectionButtons.Length; i++)
        {
            modeSelectionButtons[i].colors = GameLoaderUserInterface.buttonColors;
        }
    }

    public void SelectMode(int mode)
    {
        selectedMode = mode;
        for (int i = 0; i < modeSelectionButtons.Length; i++)
        {
            ColorBlock block = modeSelectionButtons[i].colors;
            if (i == mode)
            {
                block.normalColor = GameLoaderUserInterface.buttonColors.pressedColor;
                block.highlightedColor = GameLoaderUserInterface.buttonColors.pressedColor;
                block.disabledColor = GameLoaderUserInterface.buttonColors.pressedColor;
            }
            else
            {
                block = GameLoaderUserInterface.buttonColors;
            }
            modeSelectionButtons[i].colors = block;
        }
        SlidingUserInterface_Master.lockedDirections = new bool[2];

        Battle.BattleData mod = GameLoaderUserInterface.newBattleData;
        if (mode != 2)
        {
            mod.attacked.computerControlled = mode == 1;
        }
        GameLoaderUserInterface.newBattleData = mod;
    }

}
