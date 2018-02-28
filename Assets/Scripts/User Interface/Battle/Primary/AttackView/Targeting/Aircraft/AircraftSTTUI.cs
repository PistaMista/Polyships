using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AircraftSTTUI : SecondaryTTUI
{
    List<int> currentTargets;
    protected override int GetInitialTokenCount()
    {
        return Battle.main.attackerCapabilities.maximumAircraftCount;
    }

    protected override void PickupToken(Token_TTAgent token)
    {
        if (token.value != null)
        {
            currentTargets.Remove((int)token.value);
            UpdateCarrierTargets();
        }
        base.PickupToken(token);
    }

    protected override void DropHeldToken()
    {
        base.DropHeldToken();
        if (heldToken.value != null)
        {
            currentTargets.Add((int)heldToken.value);
            UpdateCarrierTargets();
        }
    }

    void UpdateCarrierTargets()
    {
        List<int> toAssign = new List<int>(currentTargets);
        foreach (Ship ship in Battle.main.attacker.board.ships)
        {
            if (ship.type == ShipType.CARRIER)
            {
                Carrier carrier = (Carrier)ship;
                carrier.reconTargets = toAssign.GetRange(0, Mathf.Clamp(carrier.aircraftCount, 0, toAssign.Count)).ToArray();
                toAssign.RemoveRange(0, Mathf.Clamp(carrier.aircraftCount, 0, toAssign.Count));
            }
        }
    }

    protected override void CalculateTokenValue(Token_TTAgent token)
    {

        Vector3 startingPosition = managedBoard.tiles[0, 0].transform.position;
        Vector3 rawPosition = ConvertToWorldInputPosition(currentInputPosition.screen) - startingPosition;
        Vector2Int calculatedPosition = new Vector2Int(Mathf.FloorToInt(rawPosition.x), Mathf.FloorToInt(rawPosition.z));

        if (calculatedPosition.y < 0 && calculatedPosition.x >= 0 && calculatedPosition.x < managedBoard.tiles.GetLength(0) - 1)
        {
            token.value = calculatedPosition.x;
            token.enabledPositions[1] = new Vector3(startingPosition.x + calculatedPosition.x + 0.5f, MiscellaneousVariables.it.boardUIRenderHeight, startingPosition.z - 1);
        }
        else if (calculatedPosition.x < 0 && calculatedPosition.y >= 0 && calculatedPosition.y < managedBoard.tiles.GetLength(1) - 1)
        {
            token.value = managedBoard.tiles.GetLength(0) - 1 + calculatedPosition.y;
            token.enabledPositions[1] = new Vector3(startingPosition.x - 1, MiscellaneousVariables.it.boardUIRenderHeight, startingPosition.z + calculatedPosition.y + 0.5f);
        }
    }

    protected override Vector3 CalculateHeldTokenTargetPosition(Vector3 inputPosition)
    {
        Vector3 startingPosition = managedBoard.tiles[0, 0].transform.position;
        Vector3 calculatedPosition = inputPosition - startingPosition;
        calculatedPosition.x = Mathf.FloorToInt(calculatedPosition.x);
        calculatedPosition.z = Mathf.FloorToInt(calculatedPosition.z);
        if ((calculatedPosition.z < 0 && calculatedPosition.x < managedBoard.tiles.GetLength(0) - 1) || (calculatedPosition.x < 0 && calculatedPosition.y < managedBoard.tiles.GetLength(1) - 1))
        {
            inputPosition.y = MiscellaneousVariables.it.boardUIRenderHeight;
            ((AircraftToken_TTAgent)heldToken).horizontal = calculatedPosition.x < 0;
            return inputPosition;
        }
        else
        {
            return base.CalculateHeldTokenTargetPosition(inputPosition);
        }
    }

    public override void ResetTargeting()
    {
        currentTargets = new List<int>();
        base.ResetTargeting();
    }

    protected override void SetState(UIState state)
    {
        base.SetState(state);
        RemoveDynamicAgents<LineMarker_UIAgent>("air_recon_line", false);
        if (state == UIState.ENABLING)
        {
            int[,] results = Battle.main.attackerCapabilities.airReconResults;
            for (int i = 0; i < results.GetLength(0); i++)
            {
                LineMarker_UIAgent marker = (LineMarker_UIAgent)CreateDynamicAgent("air_recon_line");
                marker.lineWidth = (1.00f - MiscellaneousVariables.it.boardTileSideLength) * 1.1f;

                int lineIndex = results[i, 0];
                int lineResult = results[i, 1];

                float linePosition = (lineIndex % (Battle.main.defender.board.tiles.GetLength(0) - 1)) + 1.0f - Battle.main.defender.board.tiles.GetLength(0) / 2.0f;
                bool lineVertical = lineIndex < (Battle.main.defender.board.tiles.GetLength(0) - 1);

                marker.transform.position = managedBoard.transform.position + linePosition * (lineVertical ? Vector3.right : Vector3.forward) + Vector3.up * (MiscellaneousVariables.it.boardUIRenderHeight + 0.0105f);
                //marker.transform.rotation = new Quaternion(0, 1, 0, lineVertical ? 2 : 3);
                marker.transform.rotation = Quaternion.Euler(Vector3.up * (lineVertical ? 0 : -90));

                Vector3[] nodes = new Vector3[5];
                int[][] connections = new int[][] { new int[] { 1 }, new int[] { 2 }, new int[] { 3 }, new int[] { 4 }, new int[0] };

                nodes[0] = Vector3.forward * managedBoard.tiles.GetLength(0) / 1.8f;
                nodes[4] = Vector3.back * managedBoard.tiles.GetLength(0) / 1.8f;

                nodes[1] = Vector3.forward * managedBoard.tiles.GetLength(0) / 8.0f;
                nodes[3] = Vector3.back * managedBoard.tiles.GetLength(0) / 8.0f;

                nodes[2] = Vector3.right * managedBoard.tiles.GetLength(0) / 8.0f * lineResult;

                marker.State = UIState.ENABLING;
                marker.Set(nodes, connections);
            }
        }
    }
}
