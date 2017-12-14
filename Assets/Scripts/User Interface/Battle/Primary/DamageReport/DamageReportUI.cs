using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageReportUI : BoardViewUI
{
    public Material hitTileMaterial;
    public Material missedTileMaterial;
    protected override void SetState(UIState state)
    {
        managedBoard = Battle.main.attacker.board;
        base.SetState(state);
        SetInteractable(state == UIState.ENABLED);
        switch (state)
        {
            case UIState.DISABLING:
                break;
            case UIState.ENABLING:
                CameraControl.GoToWaypoint(Battle.main.attacker.boardCameraPoint, MiscellaneousVariables.it.playerCameraTransitionTime);
                foreach (Tile tile in Battle.main.defender.hitTiles)
                {
                    SetTileSquareRender(tile.coordinates, tile.containedShip ? hitTileMaterial : missedTileMaterial);
                }
                break;
        }
    }

    protected override void ProcessInput()
    {
        base.ProcessInput();
        if (tap)
        {
            BattleUIMaster.EnablePrimaryBUI(BattleUIType.TURN_NOTIFIER);
            State = UIState.DISABLING;
        }
    }
}
