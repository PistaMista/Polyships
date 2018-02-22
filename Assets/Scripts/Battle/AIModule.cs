﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AIModule : ScriptableObject
{
    [Serializable]
    public struct AIModuleData
    {
        public float recklessness;
        public static implicit operator AIModuleData(AIModule module)
        {
            AIModuleData result;
            result.recklessness = module.recklessness;
            return result;
        }
    }
    public Player owner;

    public virtual void Initialize(AIModuleData data)
    {
        recklessness = data.recklessness;
    }

    public virtual void AssignReferences(AIModuleData data)
    {

    }

    public void DoTurn()
    {
        if (owner.board.ships == null)
        {
            owner.board.SpawnShips();
            PlaceShips();
            for (int i = 0; i < owner.board.ships.Length; i++)
            {
                owner.board.ships[i].gameObject.SetActive(false);
            }
            Battle.main.NextTurn();
        }
        else
        {
            Attack();
        }
    }

    struct Heatmap
    {
        public float[,] tiles;
        public float totalHeat;

        public Heatmap(Vector2Int dimensions)
        {
            tiles = new float[dimensions.x, dimensions.y];
            totalHeat = 0;
        }

        public static Heatmap operator *(Heatmap map, float mult)
        {
            for (int x = 0; x < map.tiles.GetLength(0); x++)
            {
                for (int y = 0; y < map.tiles.GetLength(1); y++)
                {
                    map.tiles[x, y] *= mult;
                }
            }

            return map;
        }

        public static Heatmap operator +(Heatmap map1, Heatmap map2)
        {
            for (int x = 0; x < map1.tiles.GetLength(0); x++)
            {
                for (int y = 0; y < map1.tiles.GetLength(1); y++)
                {
                    map1.tiles[x, y] += map2.tiles[x, y];
                }
            }

            return map1;
        }

        public void Heat(Vector2Int source, float heat, float dropoff)
        {
            if (dropoff >= 1.0f)
            {
                tiles[source.x, source.y] += heat;
                totalHeat += heat;
            }
            else
            {
                for (int x = 0; x < tiles.GetLength(0); x++)
                {
                    for (int y = 0; y < tiles.GetLength(1); y++)
                    {
                        Vector2Int relative = new Vector2Int(x, y) - source;
                        int distance = Mathf.Abs(relative.x) + Mathf.Abs(relative.y);
                        float increase = heat * Mathf.Pow(1.0f - dropoff, distance);

                        totalHeat += increase;
                        tiles[x, y] += increase;
                    }
                }
            }
        }

        // public Heatmap GetAntimap(Vector2Int pivot)
        // {
        //     Heatmap result = new Heatmap(new Vector2Int(tiles.GetLength(0), tiles.GetLength(1)));
        //     for (int x = 0; x < tiles.GetLength(0); x++)
        //     {
        //         for (int y = 0; y < tiles.GetLength(1); y++)
        //         {
        //             Vector2Int opposite = pivot - new Vector2Int(x, y);
        //             if (opposite.x >= 0 && opposite.x < tiles.GetLength(0) && opposite.y >= 0 && opposite.y < tiles.GetLength(1))
        //             {
        //                 result.tiles[x, y] = tiles[opposite.x, opposite.y];
        //             }
        //         }
        //     }

        //     return result;
        // }

        public Heatmap GetBlurredMap(float intensity)
        {
            Heatmap result = new Heatmap(new Vector2Int(tiles.GetLength(0), tiles.GetLength(1)));

            for (int axis = 0; axis < 2; axis++)
            {
                for (int column = 0; column < (axis == 0 ? tiles.GetLength(0) : tiles.GetLength(1)); column++)
                {
                    int lineLength = axis == 0 ? tiles.GetLength(0) : tiles.GetLength(1);

                    for (int direction = 0; direction < 2; direction++)
                    {
                        float storedHeat = 0;
                        for (int tile = direction == 0 ? 0 : (lineLength - 1); direction == 0 ? tile < lineLength : tile >= 0; tile += direction == 0 ? 1 : -1)
                        {
                            Vector2Int coord = new Vector2Int(axis == 0 ? tile : column, axis == 0 ? column : tile);
                            storedHeat *= intensity;
                            result.tiles[coord.x, coord.y] += storedHeat;
                            result.totalHeat += storedHeat;
                            storedHeat += tiles[coord.x, coord.y];
                        }
                    }
                }
            }

            return result;
        }
    }

    float recklessness;
    void Attack()
    {
        Board target = Battle.main.defender.board;

        //Construct a situation heatmap
        Heatmap situation = new Heatmap(new Vector2Int(target.tiles.GetLength(0), target.tiles.GetLength(1)));

        //Add heat for hit tiles
        foreach (Tile hit in owner.hitTiles)
        {
            if (hit.containedShip != null && hit.containedShip.health > 0)
            {
                situation.Heat(hit.coordinates, 12.0f, 0.8f - recklessness);
            }
            else
            {
                situation.Heat(hit.coordinates, -2.0f, 0.8f - recklessness);
            }
        }

        //Linearize the map
        situation = situation.GetBlurredMap(0.5f);

        //Cool the tiles which cannot be targeted
        foreach (Tile tile in owner.hitTiles)
        {
            situation.Heat(tile.coordinates, Mathf.NegativeInfinity, 1);
        }

        //Rate the different attack possibilities
        Vector2Int[] hottestTiles = new Vector2Int[Battle.main.attackerCapabilities.maximumArtilleryCount];
        float[] hottestTileHeatValues = new float[hottestTiles.Length];
        for (int i = 0; i < hottestTileHeatValues.Length; i++)
        {
            hottestTiles[i] = Vector2Int.down;
            hottestTileHeatValues[i] = Mathf.NegativeInfinity;
        }

        for (int x = 0; x < situation.tiles.GetLength(0); x++)
        {
            for (int y = 0; y < situation.tiles.GetLength(1); y++)
            {
                Vector2Int examinedTile = new Vector2Int(x, y);
                float examinedTileHeat = situation.tiles[x, y];

                for (int i = 0; i <= hottestTiles.Length; i++)
                {
                    float rankedTileHeat = i < hottestTiles.Length ? hottestTileHeatValues[i] : Mathf.Infinity;

                    if (examinedTileHeat <= rankedTileHeat)
                    {
                        if (i > 0)
                        {
                            Vector2Int previousRankedTile = hottestTiles[i - 1];

                            if (examinedTile != previousRankedTile)
                            {
                                hottestTiles[i - 1] = examinedTile;
                                hottestTileHeatValues[i - 1] = examinedTileHeat;
                            }
                        }
                        break;
                    }
                }
            }
        }

        float artilleryHeat = 0;
        for (int i = 0; i < hottestTileHeatValues.Length; i++)
        {
            artilleryHeat += hottestTileHeatValues[i];
        }


        int[] hottestLines = new int[Battle.main.attackerCapabilities.maximumTorpedoCount];
        float[] hottestLineHeatValues = new float[hottestLines.Length];
        for (int i = 0; i < hottestLines.Length; i++)
        {
            hottestLines[i] = -1;
            hottestLineHeatValues[i] = Mathf.NegativeInfinity;
        }

        for (int examinedLine = 0; examinedLine < situation.tiles.GetLength(0); examinedLine++)
        {
            float examinedLineHeat = 0;

            for (int y = 0; y < situation.tiles.GetLength(1); y++)
            {
                examinedLineHeat += situation.tiles[examinedLine, y];
            }

            for (int i = 0; i <= hottestLines.Length; i++)
            {
                float rankedLineHeat = i < hottestLines.Length ? hottestLineHeatValues[i] : Mathf.Infinity;

                if (examinedLineHeat < rankedLineHeat)
                {
                    if (i > 0)
                    {
                        int previousRankedLine = hottestLines[i - 1];

                        if (examinedLine != previousRankedLine)
                        {
                            hottestLines[i - 1] = examinedLine;
                            hottestLineHeatValues[i - 1] = examinedLineHeat;
                        }
                    }
                    break;
                }
            }
        }

        float torpedoHeat = 0;
        for (int i = 0; i < hottestLineHeatValues.Length; i++)
        {
            torpedoHeat += hottestLineHeatValues[i];
        }

        //TEST
        Tile[] targets = new Tile[hottestTiles.Length];
        for (int i = 0; i < hottestTiles.Length; i++)
        {
            Vector2Int coord = hottestTiles[i];
            targets[i] = target.tiles[coord.x, coord.y];
        }
        Battle.main.ExecuteArtilleryAttack(targets);
        //TEST
    }


    public void PlaceShips()
    {
        //Remove any placed owner.board.ships from the board
        for (int i = 0; i < owner.board.ships.Length; i++)
        {
            Ship ship = owner.board.ships[i];
            ship.Pickup();
            ship.Place(null);
        }

        //Each ship gets a heatmap of best placement spots
        Heatmap[] shipLocationHeatmaps = new Heatmap[owner.board.ships.Length];
        for (int i = 0; i < owner.board.ships.Length; i++)
        {
            shipLocationHeatmaps[i] = new Heatmap(new Vector2Int(owner.board.tiles.GetLength(0), owner.board.tiles.GetLength(1)));
        }

        //Determine heatmaps by individual tactical choices
        //1.Tactic - Dispersion
        float dispersionValue = UnityEngine.Random.Range(0.000f, 1.000f);
        for (int i = 0; i < owner.board.ships.Length; i++)
        {
            shipLocationHeatmaps[i].Heat(new Vector2Int(UnityEngine.Random.Range(0, owner.board.tiles.GetLength(0)), UnityEngine.Random.Range(0, owner.board.tiles.GetLength(1))), 8.0f * dispersionValue, 0.15f);
        }

        //2.Tactic - Destroyer location
        float agressivityValue = 1.0f - (float)Math.Pow(UnityEngine.Random.Range(0.000f, 1.000f), 2);
        float discretionValue = 1.0f + (float)Math.Pow(UnityEngine.Random.Range(0.000f, 1.000f), 5);

        for (int x = 0; x < owner.board.tiles.GetLength(0); x++)
        {
            for (int i = 0; i < owner.board.ships.Length; i++)
            {
                float heat = owner.board.ships[i].type == ShipType.DESTROYER ? agressivityValue * 12.0f : -discretionValue * 9.0f;
                float dropoff = owner.board.ships[i].type == ShipType.DESTROYER ? 0.1f : 0.7f;
                shipLocationHeatmaps[i].Heat(new Vector2Int(x, owner.board.tiles.GetLength(1) - 1 - UnityEngine.Random.Range(0, 4)), heat, dropoff);
            }
        }

        //3.Tactic - Camouflage
        float concealmentAccuracyValue = 1.0f - (float)Math.Pow(UnityEngine.Random.Range(0.000f, 1.000f), 4);
        List<int> cruiserIDs = new List<int>();
        for (int i = 0; i < owner.board.ships.Length; i++)
        {
            if (owner.board.ships[i].type == ShipType.CRUISER)
            {
                cruiserIDs.Add(i);
            }
        }

        int[] shipsToConcealIDs = new int[cruiserIDs.Count];
        for (int s = 0; s < shipsToConcealIDs.Length; s++)
        {
            int[] ranges = new int[owner.board.ships.Length];
            for (int i = 0; i < owner.board.ships.Length; i++)
            {
                int lastRange = i > 0 ? ranges[i - 1] : 0;
                ranges[i] = lastRange + owner.board.ships[i].concealmentAIValue;
            }

            int chosen = UnityEngine.Random.Range(0, ranges[ranges.Length - 1] + 1);
            for (int i = 0; i < owner.board.ships.Length; i++)
            {
                if (chosen <= ranges[i])
                {
                    shipsToConcealIDs[s] = i;
                    break;
                }
            }
        }

        for (int i = 0; i < shipsToConcealIDs.Length; i++)
        {
            int shipID = shipsToConcealIDs[i];
            int cruiserID = cruiserIDs[i];
            shipLocationHeatmaps[cruiserID] = shipLocationHeatmaps[cruiserID] + shipLocationHeatmaps[shipID] * 3.0f;
        }


        //Sort the ships so they get placed in the right order
        List<int> sortedShipIDs = new List<int>();

        sortedShipIDs.AddRange(shipsToConcealIDs);
        sortedShipIDs.AddRange(cruiserIDs);

        for (int i = 0; i < owner.board.ships.Length; i++)
        {
            if (!sortedShipIDs.Contains(i))
            {
                sortedShipIDs.Add(i);
            }
        }



        //Place ships in whatever the best available spot left is
        foreach (int shipID in sortedShipIDs)
        {
            Ship ship = owner.board.ships[shipID];
            ship.Pickup();

            float[,] heatmap = shipLocationHeatmaps[shipID].tiles;

            for (int x = 0; x < ship.maxHealth; x++)
            {
                Tile bestChoice = owner.board.placementInfo.selectableTiles[0];

                foreach (Tile tile in owner.board.placementInfo.selectableTiles)
                {
                    if ((heatmap[tile.coordinates.x, tile.coordinates.y] > heatmap[bestChoice.coordinates.x, bestChoice.coordinates.y]) || (heatmap[tile.coordinates.x, tile.coordinates.y] == heatmap[bestChoice.coordinates.x, bestChoice.coordinates.y] && UnityEngine.Random.Range(0, 2) == 0))
                    {
                        bestChoice = tile;
                    }
                }

                owner.board.SelectTileForPlacement(bestChoice);
            }

            if (ship.type == ShipType.CRUISER)
            {
                ((Cruiser)ship).ConcealAlreadyPlacedShipsInConcealmentArea();
            }

            if (owner.board.placementInfo.selectableTiles.Count == 0)
            {
                PlaceShips();
                break;
            }
        }

        //Determine the personality of this AI when attacking
        recklessness = Mathf.Pow(UnityEngine.Random.Range(0.000f, 1.000f), 3);
    }
}
