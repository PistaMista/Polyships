using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnNotifierUI : InputEnabledUI
{
    protected override void SetState(UIState state)
    {
        base.SetState(state);
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
            State = UIState.DISABLING;
            if (Battle.main.attacker.board.ships == null)
            {
                BattleUIMaster.EnablePrimaryBUI(BattleUIType.FLEET_PLACEMENT);
            }
            else
            {
                BattleUIMaster.EnablePrimaryBUI(BattleUIType.BATTLE_OVERVIEW);
            }
        }
    }

    protected override void Update()
    {
        base.Update();
    }

}
