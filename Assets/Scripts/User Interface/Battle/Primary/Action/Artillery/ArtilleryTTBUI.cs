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
            token.value = candidateTargetTile;

            if (candidateTargetTile != null)
            {
                token.targetPosition = candidateTargetTile.transform.position + MiscellaneousVariables.it.boardUIRenderHeight * Vector3.up;
            }
        }
    }

    protected override Vector3 CalculateHeldTokenTargetPosition(Vector3 inputPosition)
    {
        inputPosition.y = heldToken.transform.position.y;
        return inputPosition;
    }
}
