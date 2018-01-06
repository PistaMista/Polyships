using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TorpedoPTTUI : PrimaryTTUI
{
    protected override int GetInitialTokenCount()
    {
        return Battle.main.attackerCapabilities.maximumTorpedoCount;
    }

    protected override void PickupToken(Token_TTAgent token)
    {
        if (placedTokens.Count < MiscellaneousVariables.it.maximumTorpedoAttacksPerTurn || !token.OnPedestal)
        {
            base.PickupToken(token);
            SetTargetableLaneMarkers(true);
        }
    }

    protected override void DropHeldToken()
    {
        base.DropHeldToken();
        SetTargetableLaneMarkers(false);
    }

    void SetTargetableLaneMarkers(bool enabled)
    {
        if (enabled)
        {
            bool[] lanes = Battle.main.attackerCapabilities.torpedoFiringArea;
            for (int x = 0; x < lanes.Length; x++)
            {
                if (lanes[x])
                {
                    LineMarker_UIAgent laneMarker = (LineMarker_UIAgent)CreateDynamicAgent("targetable_line");
                    Vector3 mod = Vector3.up * (MiscellaneousVariables.it.boardUIRenderHeight + 0.01f);
                    laneMarker.extensionTime = laneMarker.extensionTime * (0.6f + (float)x / lanes.Length * 0.6f);
                    laneMarker.Set(new Vector3[] { managedBoard.tiles[x, managedBoard.tiles.GetLength(1) - 1].transform.position + mod + Vector3.forward * managedBoard.tiles.GetLength(1), managedBoard.tiles[x, 0].transform.position + mod + Vector3.back }, new int[][] { new int[] { 1 }, new int[0] });
                    laneMarker.State = UIState.ENABLING;
                }
            }
        }
        else
        {
            RemoveDynamicAgents<LineMarker_UIAgent>("Destroyer Firing Area", false);
        }
    }

    protected override void CalculateTokenValue(Token_TTAgent token)
    {
        Tile candidateTargetTile = GetTileAtInputPosition();

        if (candidateTargetTile != null && !placedTokens.Find(x => x.value != null && (int)x.value == candidateTargetTile.coordinates.x) && Battle.main.attackerCapabilities.torpedoFiringArea[candidateTargetTile.coordinates.x])
        {
            token.value = candidateTargetTile.coordinates.x;
            token.enabledPositions[1] = new Vector3(candidateTargetTile.transform.position.x, MiscellaneousVariables.it.boardUIRenderHeight, managedBoard.tiles[0, managedBoard.tiles.GetLength(1) - 1].transform.position.z + 1.2f);
        }
    }

    protected override Vector3 CalculateHeldTokenTargetPosition(Vector3 inputPosition)
    {
        Vector3 localBoardPosition = inputPosition - managedBoard.transform.position;
        bool hoveringOverBoard = Mathf.Abs(localBoardPosition.x) < managedBoard.tiles.GetLength(0) / 2.0f && Mathf.Abs(localBoardPosition.z) < managedBoard.tiles.GetLength(1) / 2.0f;

        if (hoveringOverBoard)
        {
            inputPosition.y = hoveringOverBoard ? MiscellaneousVariables.it.boardUIRenderHeight : stackPedestal.transform.TransformPoint(heldToken.enabledPositions[0]).y;
            inputPosition.z = managedBoard.tiles[0, managedBoard.tiles.GetLength(1) - 1].transform.position.z + 1.2f;
            return inputPosition;
        }
        else
        {
            return base.CalculateHeldTokenTargetPosition(inputPosition);
        }
    }

    protected override void ConfirmAttack()
    {
        base.ConfirmAttack();
        int[] targets = new int[placedTokens.Count];
        for (int i = 0; i < targets.Length; i++)
        {
            targets[i] = (int)placedTokens[i].value;
        }

        Battle.main.ExecuteTorpedoAttack(targets);
    }
}
