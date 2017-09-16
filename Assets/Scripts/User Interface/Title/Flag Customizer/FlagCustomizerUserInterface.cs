using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FlagCustomizerUserInterface : SlidingUserInterface
{
    public Text firstPlayerText;
    public FlagCustomizationWidget[] flagCustomizationWidgets;

    protected override void ChangeState(UIState state)
    {
        base.ChangeState(state);
        if (state == UIState.ENABLING)
        {
            if (GameModeSelectorUserInterface.selectedMode == 0)
            {
                width = 2;
                flagCustomizationWidgets[1].gameObject.SetActive(true);
                firstPlayerText.text = "FIRST PLAYER\nDRAW YOUR FLAG";
            }
            else
            {
                width = 1;
                flagCustomizationWidgets[1].gameObject.SetActive(false);
                firstPlayerText.text = "DRAW YOUR FLAG";
            }
        }
    }
}
