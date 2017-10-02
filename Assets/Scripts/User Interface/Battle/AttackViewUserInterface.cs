using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackViewUserInterface : BoardViewUserInterface
{

    bool attackConfirmed;
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
                CameraControl.GoToWaypoint(Battle.main.attacked.cameraPoint, MiscellaneousVariables.it.playerCameraTransitionTime);
                break;
        }
    }

    public void ConfirmAttack()
    {

    }
}
