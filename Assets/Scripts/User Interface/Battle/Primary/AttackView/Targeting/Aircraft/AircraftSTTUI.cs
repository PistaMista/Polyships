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
                carrier.polarSearchTargets = toAssign.GetRange(0, Mathf.Clamp(carrier.aircraftCount, 0, toAssign.Count)).ToArray();
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
        if (calculatedPosition.z < 0 || calculatedPosition.x < 0)
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
}
