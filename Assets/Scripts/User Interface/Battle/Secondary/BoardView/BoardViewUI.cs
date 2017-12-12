using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardViewUI : InputEnabledUI
{
    protected Board managedBoard;
    MovingUIAgent[,] tileParents;

    protected override void SetState(UIState state)
    {
        base.SetState(state);
        switch (state)
        {
            case UIState.ENABLING:
                if (childAgentDefaultParent)
                {
                    childAgentDefaultParent.transform.position = managedBoard.owner.transform.position;
                }
                break;
        }
    }

    protected void ResetAllTileParents()
    {
        if (tileParents != null)
        {
            for (int x = 0; x < tileParents.GetLength(0); x++)
            {
                for (int y = 0; y < tileParents.GetLength(1); y++)
                {
                    Destroy(tileParents[x, y]);
                    tileParents[x, y] = null;
                }
            }
        }

    }

    protected void ResetTileParent(Vector2Int position)
    {
        if (tileParents != null)
        {
            if (tileParents[position.x, position.y] != null)
            {
                DestroyDynamicAgent(tileParents[position.x, position.y]);
                tileParents[position.x, position.y] = null;
            }
        }
    }

    protected MovingUIAgent GetTileParent(Vector2Int position, bool reset)
    {
        if (tileParents == null)
        {
            tileParents = new MovingUIAgent[managedBoard.tiles.GetLength(0), managedBoard.tiles.GetLength(1)];
        }

        if (reset)
        {
            ResetTileParent(position);
        }

        MovingUIAgent parent = tileParents[position.x, position.y];
        if (parent == null)
        {
            Vector3 finalPosition = managedBoard.tiles[position.x, position.y].transform.position;
            if (childAgentDefaultParent)
            {
                finalPosition = childAgentDefaultParent.InverseTransformPoint(finalPosition);
            }

            parent = (MovingUIAgent)CreateDynamicAgent("tile_parent");
            parent.enabledPositions = new Vector3[1] { finalPosition };
            parent.disabledPosition = parent.enabledPositions[0];
            parent.disabledPosition.y = -10;

            parent.transform.localPosition = parent.disabledPosition;

            parent.movementTime = 0.01f + position.magnitude / 150.0f;
            tileParents[position.x, position.y] = parent;
        }

        return parent;
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
