﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackViewUserInterface : BoardViewUserInterface
{
    public TacticalTargetingBattleUserInterface selectedTargeter;
    public PrimaryTacticalTargetingBUI activePrimaryTargeter;
    public TacticalTargetingBattleUserInterface[] targeters;
    public int referenceBoardWidthForPedestalScaling;
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
                managedBoard = Battle.main.defender.board;
                CameraControl.GoToWaypoint(Battle.main.defender.boardCameraPoint, MiscellaneousVariables.it.playerCameraTransitionTime);

                float targeterSpacing = managedBoard.tiles.GetLength(1) / (float)(targeters.Length - 1);
                Vector3 startingPosition = managedBoard.owner.transform.position + Vector3.right * (managedBoard.tiles.GetLength(0) / 2.0f + 3) + Vector3.forward * managedBoard.tiles.GetLength(1) / 2.0f;
                for (int i = 0; i < targeters.Length; i++)
                {
                    targeters[i].defaultPedestalPosition = startingPosition + Vector3.back * i * targeterSpacing;
                }
                break;
        }

        for (int i = 0; i < targeters.Length; i++)
        {
            targeters[i].State = state;
        }
    }
}
