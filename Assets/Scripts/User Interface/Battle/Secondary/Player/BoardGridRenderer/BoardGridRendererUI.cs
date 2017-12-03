using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardGridRendererUI : CombatantUI
{
    protected override void ChangeState(UIState state)
    {
        base.ChangeState(state);
        switch (state)
        {
            case UIState.ENABLING:
                if (dynamicUIAgents.Count == 0)
                {
                    AddGrid();
                }
                break;
        }
    }

    public void AddGrid()
    {
        childAgentDefaultParent.transform.position = managedBoard.owner.transform.position;

        float lineWidth = (1.00f - MiscellaneousVariables.it.boardTileSideLength);
        float lineLength = managedBoard.tiles.GetLength(0);
        float startingPosition = (-managedBoard.tiles.GetLength(0) / 2.0f);
        for (int x = 1; x < managedBoard.tiles.GetLength(0); x++)
        {
            GridLine_BoardGridRendererAgent line = (GridLine_BoardGridRendererAgent)CreateDynamicAgent("grid_line");

            line.transform.localScale = new Vector3(lineWidth, 1.0f, lineLength);

            line.enabledPositions = new Vector3[1] { Vector3.right * (startingPosition + x) + Vector3.up * MiscellaneousVariables.it.boardUIRenderHeight };
            line.disabledPosition = line.enabledPositions[0];
            line.disabledPosition.y = -10;

            line.movementTime = line.movementTime * Random.Range(0.8f, 1.2f);

            line.transform.localPosition = line.disabledPosition;
        }

        lineLength = managedBoard.tiles.GetLength(1);
        startingPosition = (-managedBoard.tiles.GetLength(1) / 2.0f);
        for (int y = 1; y < managedBoard.tiles.GetLength(1); y++)
        {
            GridLine_BoardGridRendererAgent line = (GridLine_BoardGridRendererAgent)CreateDynamicAgent("grid_line");

            line.transform.localScale = new Vector3(lineLength, 1.0f, lineWidth);

            line.enabledPositions = new Vector3[1] { Vector3.forward * (startingPosition + y) + Vector3.up * MiscellaneousVariables.it.boardUIRenderHeight };
            line.disabledPosition = line.enabledPositions[0];
            line.disabledPosition.y = -10;

            line.movementTime = line.movementTime * Random.Range(0.8f, 1.2f);

            line.transform.localPosition = line.disabledPosition;
        }
    }
}
