using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnNotifierUserInterface : PrimaryBattleUserInterface
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
                CameraControl.GoToWaypoint(Battle.main.attacker.flagCameraPoint, MiscellaneousVariables.it.playerCameraTransitionTime * 0.2f);
                break;
        }
    }

    protected override void ProcessInput()
    {
        base.ProcessInput();
        if (tap)
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
