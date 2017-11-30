using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardViewUserInterface : BattleUserInterface
{
    protected Board managedBoard;
    GameObject[,] tileParents;

    protected override void ChangeState(UIState state)
    {
        base.ChangeState(state);
        switch (state)
        {
            case UIState.ENABLING:
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

    protected void ResetTileParent(Vector2 position)
    {
        if (tileParents != null)
        {
            Destroy(tileParents[(int)position.x, (int)position.y]);
            tileParents[(int)position.x, (int)position.y] = null;
        }
    }

    protected GameObject GetTileParent(Vector2Int position, bool reset)
    {
        if (tileParents == null)
        {
            tileParents = new GameObject[managedBoard.tiles.GetLength(0), managedBoard.tiles.GetLength(1)];
        }

        if (reset)
        {
            ResetTileParent(position);
        }

        GameObject parent = tileParents[position.x, position.y];
        if (parent == null)
        {
            // parent = new GameObject("Tile parent: " + position);
            // parent.transform.position = managedBoard.tiles[(int)position.x, (int)position.y].transform.position;
            // tileParents[(int)position.x, (int)position.y] = parent;
            parent = CreateDynamicAgent("tile_parent").gameObject;
            parent.transform.position = managedBoard.tiles[position.x, position.y].transform.position;
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
