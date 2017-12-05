using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FlagCustomizerUserInterface : SlidingUserInterface
{
    public Text firstPlayerText;
    public FlagCustomizationModuleUserInterface[] flagCustomizationModules;
    public Vector2 flagResolution;

    protected override void SetState(UIState state)
    {
        base.SetState(state);
        switch (state)
        {
            case UIState.ENABLING:
                if (GameLoaderUserInterface.newBattleData.attacker.flag == null)
                {
                    GameLoaderUserInterface.newBattleData.attacker.flag = new float[(int)flagResolution.x, (int)flagResolution.y, 3];
                    GameLoaderUserInterface.newBattleData.defender.flag = new float[(int)flagResolution.x, (int)flagResolution.y, 3];
                }

                flagCustomizationModules[0].State = UIState.ENABLED;
                if (GameModeSelectorUserInterface.selectedMode == 0)
                {
                    width = 2;
                    flagCustomizationModules[1].State = UIState.ENABLED;
                    firstPlayerText.text = "FIRST PLAYER\nDRAW YOUR FLAG";
                }
                else
                {
                    width = 1;
                    flagCustomizationModules[1].State = UIState.DISABLED;
                    firstPlayerText.text = "DRAW YOUR FLAG";
                }

                break;
        }
    }

    public override void OnMasterEnable()
    {
        base.OnMasterEnable();
    }
}
