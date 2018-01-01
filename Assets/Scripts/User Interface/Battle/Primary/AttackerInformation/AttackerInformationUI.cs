using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackerInformationUI : BoardViewUI
{
    public Material missedTileMaterial;
    public Material hitTileMaterial;
    public Material intactTileMaterial;
    public Material concealedShipTileMaterial;
    protected override void SetState(UIState state)
    {
        managedBoard = Battle.main.attacker.board;
        base.SetState(state);
        SetInteractable((int)state >= 2);
        switch (state)
        {
            case UIState.ENABLING:
                CameraControl.GoToWaypoint(Battle.main.attacker.boardCameraPoint, MiscellaneousVariables.it.playerCameraTransitionTime);
                for (int x = 0; x < managedBoard.tiles.GetLength(0); x++)
                {
                    for (int y = 0; y < managedBoard.tiles.GetLength(1); y++)
                    {
                        Tile tile = managedBoard.tiles[x, y];
                        Material tileMaterial = null;
                        if (tile.hit)
                        {
                            if (tile.containedShip)
                            {
                                tileMaterial = hitTileMaterial;
                            }
                            else
                            {
                                tileMaterial = missedTileMaterial;
                            }
                        }
                        else if (tile.containedShip)
                        {
                            tileMaterial = tile.containedShip.concealedBy == null ? intactTileMaterial : concealedShipTileMaterial;
                        }


                        if (tileMaterial != null)
                        {
                            SetTileSquareRender(tile.coordinates, tileMaterial, 1);
                        }
                    }
                }

                SetDestroyerFiringAreaMarkers(true);
                break;
        }
    }

    protected override void ProcessInput()
    {
        base.ProcessInput();
        if (tap)
        {
            State = UIState.DISABLING;
            BattleUIMaster.EnablePrimaryBUI(BattleUIType.BATTLE_OVERVIEW);
        }
    }
}
