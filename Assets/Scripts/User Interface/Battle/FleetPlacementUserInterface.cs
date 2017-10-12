using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FleetPlacementUserInterface : BoardViewUserInterface
{
    public Waypoint cameraWaypoint;
    protected override void ChangeState(UIState state)
    {
        base.ChangeState(state);
        switch (state)
        {
            case UIState.DISABLING:
                SetInteractable(false);
                break;
            case UIState.ENABLING:
                break;
        }
    }

    protected override void DeployWorldElements()
    {
        base.DeployWorldElements();
        cameraWaypoint.transform.position = Battle.main.attacker.boardCameraPoint.transform.position;
        cameraWaypoint.transform.rotation = Battle.main.attacker.boardCameraPoint.transform.rotation;
        CameraControl.GoToWaypoint(cameraWaypoint, MiscellaneousVariables.it.playerCameraTransitionTime);
    }
}
