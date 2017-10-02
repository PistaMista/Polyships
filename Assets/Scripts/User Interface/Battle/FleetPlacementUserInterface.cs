using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FleetPlacementUserInterface : BoardViewUserInterface
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
                CameraControl.GoToWaypoint(Battle.main.attacker.cameraPoint, MiscellaneousVariables.it.playerCameraTransitionTime);
                break;
        }
    }
}
