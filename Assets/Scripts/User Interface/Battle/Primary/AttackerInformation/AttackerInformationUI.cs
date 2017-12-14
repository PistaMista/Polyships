using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackerInformationUI : InputEnabledUI
{
    protected override void SetState(UIState state)
    {
        base.SetState(state);
        SetInteractable((int)state >= 2);
        switch (state)
        {
            case UIState.ENABLING:
                CameraControl.GoToWaypoint(Battle.main.attacker.flagCameraPoint, MiscellaneousVariables.it.playerCameraTransitionTime);
                break;
        }
    }

    protected override void ProcessInput()
    {
        base.ProcessInput();
        if (tap)
        {
            State = UIState.DISABLING;
            BattleUIMaster.EnablePrimaryBUI(BattleUIType.BATTLE_OVERVIEW);
        }
    }
}
