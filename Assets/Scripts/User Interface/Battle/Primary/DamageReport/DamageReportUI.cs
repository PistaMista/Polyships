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
        SetInteractable((int)state >= 2);
        switch (state)
        {
            case UIState.DISABLING:
                break;
            case UIState.ENABLING:
                CameraControl.GoToWaypoint(Battle.main.attacker.boardCameraPoint, MiscellaneousVariables.it.playerCameraTransitionTime);
                foreach (Tile tile in Battle.main.defender.hitTiles)
                {
                    SetTileSquareRender(tile.coordinates, tile.containedShip ? hitTileMaterial : missedTileMaterial, 1);
                }

                //DEBUG - Show the heat values of each tile
                if (Battle.main.defender.aiEnabled && MiscellaneousVariables.it.showAISituationHeat)
                {
                    Heatmap situation = Battle.main.defender.aiModule.situation;

                    List<Vector2Int> hottestTiles = new List<Vector2Int>(situation.GetExtremeTiles(3, Mathf.Infinity, false));
                    for (int x = 0; x < situation.tiles.GetLength(0); x++)
                    {
                        for (int y = 0; y < situation.tiles.GetLength(1); y++)
                        {
                            Tile tile = Battle.main.attacker.board.tiles[x, y];
                            float tileHeat = situation.tiles[x, y];

                            TextMesh text = CreateDynamicAgent("tile_heat_text").GetComponent<TextMesh>();
                            text.transform.position = tile.transform.position + Vector3.up * (0.05f + MiscellaneousVariables.it.boardUIRenderHeight);
                            text.text = (Mathf.Floor(Mathf.Clamp(tileHeat, -99.0f, 99.0f) * 10f) / 10f).ToString();

                            if (hottestTiles.Contains(new Vector2Int(x, y)))
                            {
                                text.color = Color.white;
                            }
                        }
                    }
                }
                break;
        }

        if ((int)state < 2)
        {
            RemoveDynamicAgents<UIAgent>("Tile Heat", true);
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
