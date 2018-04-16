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
            if (!linked)
            {
                Board managedBoard = player.board;
                Delinker += () =>
                {
                    foreach (Ship ship in managedBoard.ships)
                    {
                        ship.gameObject.SetActive(false);
                    }
                };

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

        public Gameplay.Tile GetTileAtPosition(Vector3 position)
        {
            Vector3Int calculatedPosition = GetFlatTileCoordinateAtPosition(position);
            if (calculatedPosition.x >= 0 && calculatedPosition.x < player.board.tiles.GetLength(0) && calculatedPosition.z >= 0 && calculatedPosition.z < player.board.tiles.GetLength(1))
            {
                return player.board.tiles[calculatedPosition.x, calculatedPosition.z];
            }

            return null;
        }

        public Vector3Int GetFlatTileCoordinateAtPosition(Vector3 position)
        {
            Vector3 startingPosition = player.board.tiles[0, 0].transform.position - new Vector3(1, 0, 1) * 0.5f;
            Vector3 calculatedPosition = position - startingPosition;


            return new Vector3Int(Mathf.FloorToInt(calculatedPosition.x), 0, Mathf.FloorToInt(calculatedPosition.z));
        }

        public void ShowInformation(bool undamagedShips, bool damagedShips, bool destroyedShips, bool hitTiles)
        {
            List<Gameplay.Tile> drawnTiles = new List<Gameplay.Tile>();
            for (int i = 0; i < player.board.ships.Length; i++)
            {
                Ship ship = player.board.ships[i];
                if ((undamagedShips && ship.health == ship.maxHealth) || (damagedShips && ship.health < ship.maxHealth && ship.health > 0) || (destroyedShips && ship.health == 0))
                {
                    ship.transform.position = ship.placementInfo.boardPosition + ship.parentBoard.transform.position + Vector3.up * MiscellaneousVariables.it.boardUIRenderHeight;
                    ship.transform.rotation = ship.placementInfo.boardRotation;

                    ship.gameObject.SetActive(true);
                    foreach (Gameplay.Tile tile in ship.tiles)
                    {
                        drawnTiles.Add(tile);
                        SetTileGraphic(tile.coordinates, ship.concealedBy != null ? TileGraphicMaterial.SHIP_CONCEALED : TileGraphicMaterial.SHIP_INTACT, Color.white);
                    }
                }
            }

            if (hitTiles)
            {
                Player attacker = player != Battle.main.attacker ? Battle.main.attacker : Battle.main.defender;
                foreach (Gameplay.Tile tile in attacker.hitTiles)
                {
                    drawnTiles.Add(tile);
                    SetTileGraphic(tile.coordinates, tile.containedShip != null ? TileGraphicMaterial.SHIP_DAMAGED : TileGraphicMaterial.TILE_HIT, Color.white);
                }
            }

            foreach (Gameplay.Tile tile in player.board.tiles)
            {
                if (!drawnTiles.Contains(tile)) SetTileGraphic(tile.coordinates, TileGraphicMaterial.NONE, Color.clear);
            }
        }
    }
}
