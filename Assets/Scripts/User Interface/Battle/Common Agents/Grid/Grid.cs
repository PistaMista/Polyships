using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BattleUIAgents.Agents;
using BattleUIAgents.Base;
using Gameplay;

namespace BattleUIAgents.Agents
{
    public class Grid : BattleUIAgent
    {
        Agents.Tile[,] tiles;
        public Material[] tileGraphicMaterials;
        public enum TileGraphicMaterial
        {
            NONE,
            SHIP_INTACT,
            SHIP_DAMAGED,
            SHIP_CONCEALED,
            TILE_RESTRICTED,
            TILE_CONCEALMENT_AREA,
            TILE_SELECTED_FOR_PLACEMENT,
            TILE_HIT
        }
        protected override void PerformLinkageOperations()
        {
            base.PerformLinkageOperations();
            Board managedBoard = player.board;
            Gridline[] gridLines = Array.ConvertAll(CreateAndLinkAgents<Gridline>("", managedBoard.tiles.GetLength(0) + managedBoard.tiles.GetLength(1) - 2), (item) => { return (Gridline)item; });

            float lineWidth = 1.00f - MiscellaneousVariables.it.boardTileSideLength;
            for (int i = 0; i < gridLines.Length; i++)
            {
                Gridline positionedLine = gridLines[i];
                positionedLine.movementTime *= UnityEngine.Random.Range(0.800f, 1.200f);

                int verticalIndex = i - (managedBoard.tiles.GetLength(0) - 1);

                if (verticalIndex < 0)
                {
                    positionedLine.transform.localScale = new Vector3(lineWidth, 1, managedBoard.tiles.GetLength(1));
                    positionedLine.hookedPosition = new Vector3(-managedBoard.tiles.GetLength(0) / 2.0f + i + 1, 0, 0);
                }
                else
                {
                    positionedLine.transform.localScale = new Vector3(managedBoard.tiles.GetLength(0), 1, lineWidth);
                    positionedLine.hookedPosition = new Vector3(0, 0, -managedBoard.tiles.GetLength(1) / 2.0f + verticalIndex + 1);
                }

                positionedLine.hookedPosition += managedBoard.transform.position + Vector3.up * MiscellaneousVariables.it.boardUIRenderHeight;
                positionedLine.unhookedPosition = positionedLine.hookedPosition - Vector3.up * positionedLine.hookedPosition.y * 1.25f;
                positionedLine.transform.position = positionedLine.unhookedPosition;
            }

            tiles = new Agents.Tile[managedBoard.tiles.GetLength(0), managedBoard.tiles.GetLength(1)];
        }

        public void SetTileGraphic(Vector2Int tileCoordinates, TileGraphicMaterial material, Color color)
        {
            Agents.Tile tile = tiles[tileCoordinates.x, tileCoordinates.y];
            if (material == TileGraphicMaterial.NONE)
            {
                if (tile != null) tile.Delinker();
            }
            else
            {
                Material actualMaterial = tileGraphicMaterials[(int)material - 1];


                if (tile == null)
                {
                    tile = (Agents.Tile)CreateAndLinkAgent<Agents.Tile>("");
                    tile.Delinker += () => { tiles[tileCoordinates.x, tileCoordinates.y] = null; };

                    tile.hookedPosition = player.board.tiles[tileCoordinates.x, tileCoordinates.y].transform.position + MiscellaneousVariables.it.boardUIRenderHeight * Vector3.up;

                    tile.unhookedPosition = tile.hookedPosition;
                    tile.unhookedPosition.y = -10f;
                    tile.transform.position = tile.unhookedPosition;
                    tile.transform.localScale = new Vector3(1, 0, 1) * MiscellaneousVariables.it.boardTileSideLength / 10.0f + Vector3.up;

                    tiles[tileCoordinates.x, tileCoordinates.y] = tile;
                }



                tile.SetMaterialAndColor(actualMaterial, color);
            }
        }

        public Gameplay.Tile GetTileAtCurrentInputPosition()
        {
            Vector3 startingPosition = player.board.tiles[0, 0].transform.position - new Vector3(1, 0, 1) * 0.5f;
            Vector3 calculatedPosition = currentInputPosition.world - startingPosition;
            calculatedPosition.x = Mathf.FloorToInt(calculatedPosition.x);
            calculatedPosition.z = Mathf.FloorToInt(calculatedPosition.z);

            if (calculatedPosition.x >= 0 && calculatedPosition.x < player.board.tiles.GetLength(0) && calculatedPosition.z >= 0 && calculatedPosition.z < player.board.tiles.GetLength(1))
            {
                return player.board.tiles[(int)calculatedPosition.x, (int)calculatedPosition.z];
            }

            return null;
        }
    }
}
