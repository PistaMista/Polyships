using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardViewUI : InputEnabledUI
{
    public Board managedBoard;
    Tile_BoardViewAgent[,] tileAgents;
    List<LineMarker_UIAgent> lineMarkers = new List<LineMarker_UIAgent>();

    protected override void SetState(UIState state)
    {
        if ((int)state < 2)
        {
            RemoveAllTileAgents();
        }

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

    void UpdateLineMarkers()
    {
        RemoveLineMarkers();
    }

    void RemoveLineMarkers()
    {
        lineMarkers.ForEach(x => { x.State = UIState.DISABLING; dynamicUIAgents.Remove(x); });
        lineMarkers = new List<LineMarker_UIAgent>();
    }

    protected void RemoveAllTileAgents()
    {
        DestroyDynamicAgents<Tile_BoardViewAgent>("");
        tileAgents = new Tile_BoardViewAgent[managedBoard.tiles.GetLength(0), managedBoard.tiles.GetLength(1)];
    }

    protected void RemoveTileAgent(Vector2Int position)
    {
        if (tileAgents != null)
        {
            if (tileAgents[position.x, position.y] != null)
            {
                Tile_BoardViewAgent tileAgent = tileAgents[position.x, position.y];
                tileAgent.State = UIState.DISABLING;
                dynamicUIAgents.Remove(tileAgent);
                tileAgents[position.x, position.y] = null;
            }
        }
    }

    protected Tile_BoardViewAgent GetTileAgent(Vector2Int position, bool reset)
    {
        if (tileAgents == null)
        {
            tileAgents = new Tile_BoardViewAgent[managedBoard.tiles.GetLength(0), managedBoard.tiles.GetLength(1)];
        }

        if (reset)
        {
            RemoveTileAgent(position);
        }

        Tile_BoardViewAgent tileAgent = tileAgents[position.x, position.y];
        if (tileAgent == null)
        {
            Vector3 finalPosition = managedBoard.tiles[position.x, position.y].transform.position;
            if (childAgentDefaultParent)
            {
                finalPosition = childAgentDefaultParent.InverseTransformPoint(finalPosition);
            }

            tileAgent = (Tile_BoardViewAgent)CreateDynamicAgent("tile_parent");
            tileAgent.enabledPositions = new Vector3[1] { finalPosition };
            tileAgent.disabledPosition = tileAgent.enabledPositions[0];
            tileAgent.disabledPosition.y = -10;

            tileAgent.transform.localPosition = tileAgent.disabledPosition;

            tileAgent.movementTime = 0.01f + position.magnitude / 150.0f;
            tileAgents[position.x, position.y] = tileAgent;
        }

        return tileAgent;
    }

    protected void SetTileSquareRender(Vector2Int position, Material material, int id)
    {
        Tile_BoardViewAgent tileAgent = GetTileAgent(position, false);


        if (tileAgent.id != id)
        {
            tileAgent = GetTileAgent(position, true);
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Quad);
            marker.transform.SetParent(tileAgent.transform);
            marker.transform.localPosition = Vector3.up * MiscellaneousVariables.it.boardUIRenderHeight;

            marker.transform.localScale = Vector3.one * MiscellaneousVariables.it.boardTileSideLength;
            marker.transform.Rotate(90, 0, 0);
            Renderer renderer = marker.GetComponent<Renderer>();
            renderer.material = material;
            tileAgent.id = id;
        }
    }

    protected void SetTileSquareRender(Vector2Int position, Material material, Color color)
    {
        Tile_BoardViewAgent tileAgent = GetTileAgent(position, true);

        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Quad);
        marker.transform.SetParent(tileAgent.transform);
        marker.transform.localPosition = Vector3.up * MiscellaneousVariables.it.boardUIRenderHeight;

        marker.transform.localScale = Vector3.one * MiscellaneousVariables.it.boardTileSideLength;
        marker.transform.Rotate(90, 0, 0);
        Renderer renderer = marker.GetComponent<Renderer>();
        renderer.material = material;

        MaterialPropertyBlock block = new MaterialPropertyBlock();
        block.SetColor("Color", color);
        renderer.SetPropertyBlock(block);
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
