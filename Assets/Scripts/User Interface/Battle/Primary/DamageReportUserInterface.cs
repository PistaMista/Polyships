using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageReportUserInterface : BoardViewUserInterface
{

    protected override void ChangeState(UIState state)
    {
        base.ChangeState(state);
        switch (state)
        {
            case UIState.DISABLING:
                break;
            case UIState.ENABLING:
                CameraControl.GoToWaypoint(Battle.main.defender.boardCameraPoint, MiscellaneousVariables.it.playerCameraTransitionTime);
                break;
        }
    }
}
