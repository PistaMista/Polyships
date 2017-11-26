using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArtilleryTTBUI : PrimaryTacticalTargetingBUI
{
    protected override int GetInitialTokenCount()
    {
        return Battle.main.attackerCapabilities.maximumArtilleryCount;
    }

    protected override void CalculateTokenValue(ActionToken token)
    {
        float velocityThreshold = managedBoard.tiles.GetLength(0) / 5.0f;
        bool angleMatch = Vector3.Angle(token.velocity, defaultPedestalPosition - token.transform.position) < 30;

        if (token.velocity.magnitude > velocityThreshold && angleMatch)
        {
            token.value = null;
        }
        else
        {
            Tile candidateTargetTile = GetTileAtInputPosition();

            if (candidateTargetTile != null && !placedTokens.Find(x => x.value && x.value == candidateTargetTile))
            {
                token.value = candidateTargetTile;
                token.targetPosition = candidateTargetTile.transform.position + MiscellaneousVariables.it.boardUIRenderHeight * Vector3.up;
            }
        }
    }

    protected override Vector3 CalculateHeldTokenTargetPosition(Vector3 inputPosition)
    {
        Vector3 localBoardPosition = inputPosition - managedBoard.transform.position;
        bool hoveringOverBoard = Mathf.Abs(localBoardPosition.x) < managedBoard.tiles.GetLength(0) / 2.0f && Mathf.Abs(localBoardPosition.z) < managedBoard.tiles.GetLength(1) / 2.0f;

        if (hoveringOverBoard)
        {
            inputPosition.y = hoveringOverBoard ? MiscellaneousVariables.it.boardUIRenderHeight : stackPedestal.transform.TransformPoint(heldToken.defaultPositionRelativeToPedestal).y;
            return inputPosition;
        }
        else
        {
            return base.CalculateHeldTokenTargetPosition(inputPosition);
        }

    }

    public override void ConfirmTargeting()
    {
        Tile[] targets = new Tile[placedTokens.Count];
        for (int i = 0; i < targets.Length; i++)
        {
            targets[i] = (Tile)placedTokens[i].value;
        }

        Battle.main.ExecuteArtilleryAttack(targets);
    }
}
