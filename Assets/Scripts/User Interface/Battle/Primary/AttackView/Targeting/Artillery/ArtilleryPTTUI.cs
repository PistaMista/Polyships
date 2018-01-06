using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArtilleryPTTUI : PrimaryTTUI
{
    protected override int GetInitialTokenCount()
    {
        return Battle.main.attackerCapabilities.maximumArtilleryCount;
    }

    protected override void CalculateTokenValue(Token_TTAgent token)
    {
        Tile candidateTargetTile = GetTileAtInputPosition();

        if (candidateTargetTile != null && !placedTokens.Find(x => x.value != null && x.value == candidateTargetTile))
        {
            token.value = candidateTargetTile;
            token.enabledPositions[1] = candidateTargetTile.transform.position + MiscellaneousVariables.it.boardUIRenderHeight * Vector3.up;
        }
    }

    protected override Vector3 CalculateHeldTokenTargetPosition(Vector3 inputPosition)
    {
        Vector3 localBoardPosition = inputPosition - managedBoard.transform.position;
        bool hoveringOverBoard = Mathf.Abs(localBoardPosition.x) < managedBoard.tiles.GetLength(0) / 2.0f && Mathf.Abs(localBoardPosition.z) < managedBoard.tiles.GetLength(1) / 2.0f;

        if (hoveringOverBoard)
        {
            inputPosition.y = hoveringOverBoard ? MiscellaneousVariables.it.boardUIRenderHeight : stackPedestal.transform.TransformPoint(heldToken.enabledPositions[0]).y;
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
        Tile[] targets = new Tile[placedTokens.Count];
        for (int i = 0; i < targets.Length; i++)
        {
            targets[i] = (Tile)placedTokens[i].value;
        }

        Battle.main.ExecuteArtilleryAttack(targets);
    }
}
