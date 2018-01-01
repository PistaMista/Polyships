using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardViewUI : InputEnabledUI
{
    public Board managedBoard;
    Tile_BoardViewAgent[,] tileAgents;

    protected override void SetState(UIState state)
    {
        if ((int)state < 2)
        {
            RemoveDynamicAgents<Tile_BoardViewAgent>("", state == UIState.DISABLED);
            RemoveDynamicAgents<LineMarker_UIAgent>("", state == UIState.DISABLED);
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

    protected void SetDestroyerFiringAreaMarkers(bool enabled)
    {
        RemoveDynamicAgents<LineMarker_UIAgent>("Destroyer Firing Area", false);

        if (enabled)
        {
            foreach (Ship ship in managedBoard.ships)
            {
                if (ship.tiles != null && ship.tiles.Length > 0 && ship.type == ShipType.DESTROYER && ship.health > 0)
                {
                    Destroyer destroyer = (Destroyer)ship;
                    LineMarker_UIAgent marker = (LineMarker_UIAgent)CreateDynamicAgent("destroyer_firing_area_marker");

                    Vector3[] nodes = new Vector3[managedBoard.tiles.GetLength(0) * 2];
                    Vector2Int destroyerPos = destroyer.tiles[1].coordinates;

                    for (int x = 0; x < managedBoard.tiles.GetLength(0); x++)
                    {
                        Vector3 position = managedBoard.tiles[x, destroyerPos.y].transform.position;
                        position.y = MiscellaneousVariables.it.boardUIRenderHeight + 0.01f;
                        nodes[x] = position;


                        int limitedY = destroyer.firingAreaBlockages[x];
                        if (limitedY >= 0)
                        {
                            position = managedBoard.tiles[x, limitedY].transform.position;
                            position.y = MiscellaneousVariables.it.boardUIRenderHeight + 0.01f;
                            nodes[x + managedBoard.tiles.GetLength(0)] = position;
                        }
                        else
                        {
                            position = managedBoard.tiles[x, 0].transform.position;
                            position.y = MiscellaneousVariables.it.boardUIRenderHeight + 0.01f;
                            position.z += managedBoard.tiles.GetLength(0) * 1.5f;
                            nodes[x + managedBoard.tiles.GetLength(0)] = position;
                        }
                    }

                    int[][] connections = new int[nodes.Length][];
                    for (int x = 0; x < managedBoard.tiles.GetLength(0); x++)
                    {
                        connections[x] = new int[(x == 0 || x == managedBoard.tiles.GetLength(0) - 1 ? 1 : 2) + (x == destroyerPos.x ? 1 : 0)];
                        connections[x + managedBoard.tiles.GetLength(0)] = new int[0];
                    }

                    for (int x = 0; x < managedBoard.tiles.GetLength(0); x++)
                    {
                        connections[x][0] = x + managedBoard.tiles.GetLength(0);
                        if (x < destroyerPos.x)
                        {
                            connections[x + 1][1] = x;
                        }
                        else if (x != managedBoard.tiles.GetLength(0) - 1)
                        {
                            connections[x][(x == 0 ? 1 : (x == destroyerPos.x ? 2 : 1))] = x + 1;
                        }
                    }

                    marker.Set(nodes, connections, destroyerPos.x);
                    marker.State = UIState.ENABLING;
                }
            }
        }
    }
    protected void RemoveTileAgent(Vector2Int position, bool instant)
    {
        if (tileAgents != null)
        {
            Tile_BoardViewAgent tileAgent = tileAgents[position.x, position.y];
            if (tileAgent != null)
            {
                RemoveDynamicAgent(tileAgent, instant);
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
            RemoveTileAgent(position, false);
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
