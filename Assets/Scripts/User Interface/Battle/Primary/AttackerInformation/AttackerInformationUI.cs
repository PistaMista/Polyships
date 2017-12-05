using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackerInformationUI : InputEnabledUI
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
                CameraControl.GoToWaypoint(Battle.main.attacker.flagCameraPoint, MiscellaneousVariables.it.playerCameraTransitionTime);
                break;
        }
    }
}
