using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnNotifierUserInterface : BattleUserInterface
{
    protected override void ChangeState(UIState state)
    {
        base.ChangeState(state);
        switch (state)
        {
            case UIState.DISABLING:
                SetInteractable(false);
                break;
            case UIState.ENABLING:
                SetInteractable(true);
                break;
        }
    }

    protected override void ProcessInput()
    {
        base.ProcessInput();
        if (beginPress)
        {
            if (Battle.main.attacker.ships == null)
            {
                SwitchToInterface(BattleUIType.FLEET_PLACEMENT);
            }
            else
            {
                SwitchToInterface(BattleUIType.BATTLE_OVERVIEW);
            }
        }
    }

    protected override void Update()
    {
        base.Update();
    }

}
