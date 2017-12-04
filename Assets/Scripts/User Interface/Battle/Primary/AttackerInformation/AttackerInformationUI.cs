using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackerInformationUI : InputEnabledUI
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
                CameraControl.GoToWaypoint(Battle.main.attacker.flagCameraPoint, MiscellaneousVariables.it.playerCameraTransitionTime);
                break;
        }
    }
}
