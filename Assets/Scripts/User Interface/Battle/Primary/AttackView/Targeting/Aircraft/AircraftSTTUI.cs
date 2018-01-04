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
                carrier.polarSearchTargets = toAssign.GetRange(0, carrier.aircraftCount).ToArray();
                toAssign.RemoveRange(0, carrier.aircraftCount);
            }
        }
    }

    protected override void CalculateTokenValue(Token_TTAgent token)
    {
        float velocityThreshold = managedBoard.tiles.GetLength(0) / 5.0f;
        bool angleMatch = Vector3.Angle(token.globalVelocity, stackPedestal.enabledPositions[0] - token.transform.position) < 30;

        if (token.globalVelocity.magnitude > velocityThreshold && angleMatch)
        {
            token.value = null;
        }
        else
        {
            // Tile candidateTargetTile = GetTileAtInputPosition();

            // if (candidateTargetTile != null && !placedTokens.Find(x => x.value != null && ((Tile)x.value).coordinates.x == candidateTargetTile.coordinates.x) && Battle.main.attackerCapabilities.torpedoFiringArea[candidateTargetTile.coordinates.x])
            // {
            //     token.value = candidateTargetTile;
            //     token.enabledPositions[1] = new Vector3(candidateTargetTile.transform.position.x, MiscellaneousVariables.it.boardUIRenderHeight, managedBoard.tiles[0, managedBoard.tiles.GetLength(1) - 1].transform.position.z + 1.2f);
            // }
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

    public override void ResetTargeting()
    {
        currentTargets = new List<int>();
        base.ResetTargeting();
    }
}
