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
            TILE_SELECTED_FOR_PLACEMENT
        }
        protected override void GatherRequiredAgents()
        {
            base.GatherRequiredAgents();
            Board managedBoard = player.board;
            Gridline[] gridLines = Array.ConvertAll(HookToThis<Gridline>("", player, managedBoard.tiles.GetLength(0) + managedBoard.tiles.GetLength(1) - 2, true), (item) => { return (Gridline)item; });

            float lineWidth = 1.00f - MiscellaneousVariables.it.boardTileSideLength;
            for (int i = 0; i < gridLines.Length; i++)
            {
                Gridline positionedLine = gridLines[i];
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
                if (tile != null) tile.Unhook();
            }
            else
            {
                Material actualMaterial = tileGraphicMaterials[(int)material - 1];


                if (tile == null)
                {
                    tile = ((Agents.Tile[])HookToThis<Agents.Tile>("", player, 1, true))[0];
                    tile.ServiceDehooker += () => { tiles[tileCoordinates.x, tileCoordinates.y] = null; };

                    tile.hookedPosition = player.board.tiles[tileCoordinates.x, tileCoordinates.y].transform.position;

                    tile.unhookedPosition = tile.hookedPosition;
                    tile.unhookedPosition.y = -10f;

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
