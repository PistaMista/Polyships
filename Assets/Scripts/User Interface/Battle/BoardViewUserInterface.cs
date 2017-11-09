using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardViewUserInterface : BattleUserInterface
{
    protected Board managedBoard;
    public FlagRendererSecondaryBUI flagRenderer;

    protected override void ChangeState(UIState state)
    {
        base.ChangeState(state);
        switch (state)
        {
            case UIState.ENABLING:
                if (flagRenderer.gameObject.activeInHierarchy)
                {
                    flagRenderer.onCameraOcclusion += DeployWorldElements;
                }
                else
                {
                    DeployWorldElements();
                }
                break;
            case UIState.DISABLING:
                flagRenderer.onCameraOcclusion += HideWorldElements;
                break;
        }
    }

    protected virtual void DeployWorldElements()
    {
        SetInteractable(true);
    }

    protected virtual void HideWorldElements()
    {

    }

    protected Tile GetTileAtInputPosition()
    {
        Vector3 startingPosition = managedBoard.tiles[0, 0].transform.position - new Vector3(1, 0, 1) * 0.5f;
        Vector3 calculatedPosition = ConvertToWorldInputPosition(currentInputPosition.screen) - startingPosition;
        calculatedPosition.x = Mathf.FloorToInt(calculatedPosition.x);
        calculatedPosition.z = Mathf.FloorToInt(calculatedPosition.z);

        if (calculatedPosition.x >= 0 && calculatedPosition.x < managedBoard.tiles.GetLength(0) && calculatedPosition.z >= 0 && calculatedPosition.z < managedBoard.tiles.GetLength(1))
        {
            return managedBoard.tiles[(int)calculatedPosition.x, (int)calculatedPosition.z];
        }

        return null;
    }
}
