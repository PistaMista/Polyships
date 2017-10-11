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
                SetInteractable(true);
                cameraWaypoint.transform.position = Battle.main.attacker.boardCameraPoint.transform.position + Vector3.left * Battle.main.attacker.board.tiles.GetLength(0) / 2.0f * 0.85f;
                cameraWaypoint.transform.rotation = Battle.main.attacker.boardCameraPoint.transform.rotation;
                CameraControl.GoToWaypoint(cameraWaypoint, MiscellaneousVariables.it.playerCameraTransitionTime);
                break;
        }
    }
}
