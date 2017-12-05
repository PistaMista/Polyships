using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageReportUI : BoardViewUI
{

    protected override void SetState(UIState state)
    {
        base.SetState(state);
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
