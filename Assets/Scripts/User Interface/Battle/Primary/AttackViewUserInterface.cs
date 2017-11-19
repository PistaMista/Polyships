﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackViewUserInterface : BoardViewUserInterface
{
    public TacticalTargetingBattleUserInterface selectedTargeter;
    public PrimaryTacticalTargetingBUI activePrimaryTargeter;
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
                CameraControl.GoToWaypoint(Battle.main.defender.boardCameraPoint, MiscellaneousVariables.it.playerCameraTransitionTime);
                break;
        }
    }
}
