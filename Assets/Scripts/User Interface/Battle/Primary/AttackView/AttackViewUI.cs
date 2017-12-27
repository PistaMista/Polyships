﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackViewUI : BoardViewUI
{
    public TTUI selectedTargeter;
    public PrimaryTTUI activePrimaryTargeter;
    public TTUI[] targeters;
    public int referenceBoardWidthForPedestalScaling;
    public FireButton_AttackViewAgent fireButton;
    public Material hitTileMaterial;
    public Material missedTileMaterial;
    protected override void SetState(UIState state)
    {
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
                    targeters[i].stackPedestal.enabledPositions[0] = startingPosition + Vector3.back * i * targeterSpacing;
                }

                fireButton.owner = this;
                fireButton.enabledPositions = new Vector3[2];
                fireButton.enabledPositions[0] = new Vector3(-managedBoard.tiles.GetLength(0) / 2.0f - 4, MiscellaneousVariables.it.boardUIRenderHeight, -managedBoard.tiles.GetLength(1) / 2.0f + 4);
                fireButton.enabledPositions[1] = fireButton.enabledPositions[0] + Vector3.left * managedBoard.tiles.GetLength(0);
                fireButton.disabledPosition = fireButton.enabledPositions[1];
                fireButton.disabledPosition.y = -10;
                fireButton.transform.position = fireButton.disabledPosition;


                break;
        }
        base.SetState(state);

        switch (state)
        {
            case UIState.ENABLING:
                foreach (Tile tile in Battle.main.attacker.hitTiles)
                {
                    SetTileSquareRender(tile.coordinates, tile.containedShip ? hitTileMaterial : missedTileMaterial, 1);
                }
                break;
        }
    }

    protected override void ProcessInput()
    {
        base.ProcessInput();
        if (tap && !fireButton.pushed)
        {
            if (!GetTileAtInputPosition())
            {
                State = UIState.DISABLING;
                BattleUIMaster.EnablePrimaryBUI(BattleUIType.BATTLE_OVERVIEW);
            }
        }


        if (activePrimaryTargeter)
        {
            if (endPress && fireButton.pushed)
            {
                activePrimaryTargeter.ConfirmTargeting();
            }

            if (pressed)
            {
                Vector3 position = ConvertToWorldInputPosition(currentInputPosition.screen);
                fireButton.Push(position);
            }
            else
            {
                fireButton.pushed = false;
            }
        }
        else
        {
            fireButton.pushed = false;
        }
    }
}
