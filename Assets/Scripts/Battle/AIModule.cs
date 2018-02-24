using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
[Serializable]
public struct Heatmap
{
    public float[,] tiles;
    public float totalHeat;

    public Heatmap(int dimX, int dimY)
    {
        tiles = new float[dimX, dimY];
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

    public Heatmap GetBlurredMap(float intensity)
    {
        Heatmap result = new Heatmap(tiles.GetLength(0), tiles.GetLength(1));

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
                        result.Heat(coord, storedHeat, 1);
                        storedHeat += tiles[coord.x, coord.y];
                    }
                }
            }
        }

        return result;
    }

    public Heatmap GetNormalizedMap()
    {
        Heatmap result = new Heatmap(tiles.GetLength(0), tiles.GetLength(1));

        Heatmap intermediate = new Heatmap(tiles.GetLength(0), tiles.GetLength(1));
        Vector2Int coldestTile = GetHottestTiles(1, Mathf.NegativeInfinity, true)[0];
        float lowestHeat = tiles[coldestTile.x, coldestTile.y];

        for (int x = 0; x < tiles.GetLength(0); x++)
        {
            for (int y = 0; y < tiles.GetLength(1); y++)
            {
                intermediate.Heat(new Vector2Int(x, y), tiles[x, y] - lowestHeat, 1);
            }
        }


        Vector2Int hottestTile = intermediate.GetHottestTiles(1, Mathf.Infinity)[0];
        float highestHeat = intermediate.tiles[hottestTile.x, hottestTile.y];

        if (highestHeat == 0)
        {
            return result;
        }

        for (int x = 0; x < tiles.GetLength(0); x++)
        {
            for (int y = 0; y < tiles.GetLength(1); y++)
            {
                result.Heat(new Vector2Int(x, y), intermediate.tiles[x, y] / highestHeat, 1);
            }
        }

        return result;
    }

    public Vector2Int[] GetHottestTiles(int count, float threshold)
    {
        return GetHottestTiles(count, threshold, false);
    }

    public Vector2Int[] GetHottestTiles(int count, float threshold, bool coldest)
    {
        List<Vector2Int> bestTiles = new List<Vector2Int>();

        float lastBestTileHeat = coldest ? Mathf.NegativeInfinity : Mathf.Infinity;
        for (int i = 0; i < count; i++)
        {
            float bestTileHeat = coldest ? Mathf.Infinity : Mathf.NegativeInfinity;
            Vector2Int bestTile = Vector2Int.right;
            for (int x = 0; x < tiles.GetLength(0); x++)
            {
                for (int y = 0; y < tiles.GetLength(1); y++)
                {
                    Vector2Int examinedTile = new Vector2Int(x, y);
                    float examinedTileHeat = tiles[x, y];

                    bool lower = coldest ? examinedTileHeat < bestTileHeat : examinedTileHeat > bestTileHeat;
                    bool upper = coldest ? examinedTileHeat > lastBestTileHeat : examinedTileHeat < lastBestTileHeat;
                    bool limiter = coldest ? examinedTileHeat > threshold : examinedTileHeat < threshold;

                    if (!bestTiles.Contains(examinedTile) && upper && lower && limiter)
                    {
                        bestTileHeat = examinedTileHeat;
                        bestTile = examinedTile;
                    }
                }
            }

            bestTiles.Add(bestTile);
            lastBestTileHeat = bestTileHeat;
        }

        return bestTiles.ToArray();
    }
}
public class AIModule : ScriptableObject
{
    [Serializable]
    public struct AIModuleData
    {
        public Heatmap situation;
        public Heatmap airReconMap;
        public float recklessness;
        public float agressivity;
        public float reconResultWeight;
        public float reconResultMemory;
        public static implicit operator AIModuleData(AIModule module)
        {
            AIModuleData result;
            result.situation = module.situation;
            result.airReconMap = module.airReconMap;

            result.recklessness = module.recklessness;
            result.agressivity = module.agressivity;
            result.reconResultWeight = module.reconResultWeight;
            result.reconResultMemory = module.reconResultMemory;
            return result;
        }
    }
    public Player owner;

    public virtual void Initialize(AIModuleData data)
    {
        situation = data.situation;
        airReconMap = data.airReconMap;

        recklessness = data.recklessness;
        agressivity = data.agressivity;
        reconResultMemory = data.reconResultMemory;
        reconResultWeight = data.reconResultWeight;
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



    public Heatmap situation;
    public Heatmap airReconMap;
    public float recklessness;
    public float agressivity;
    public float reconResultMemory; //The air recon heatmap will be multiplied by this every turn
    public float reconResultWeight; //How much will the AI use the recon results
    void Attack()
    {
        Board target = Battle.main.defender.board;

        //Construct a situation heatmap
        situation = new Heatmap(target.tiles.GetLength(0), target.tiles.GetLength(1));

        //Add heat for hit tiles
        foreach (Tile hit in owner.hitTiles)
        {
            if (hit.containedShip != null && hit.containedShip.health > 0)
            {
                situation.Heat(hit.coordinates, 12.0f, 1.0f - recklessness);
            }
            else
            {
                situation.Heat(hit.coordinates, -2.0f, 1.0f - recklessness);
            }
        }

        //Use the air recon results to enhance chances of hitting
        int[,] reconResults = Battle.main.attackerCapabilities.airReconResults;
        for (int i = 0; i < reconResults.GetLength(0); i++)
        {
            int lineIndex = reconResults[i, 0];
            int result = reconResults[i, 1];

            int linePosition = (lineIndex % (Battle.main.defender.board.tiles.GetLength(0) - 1));
            bool lineVertical = lineIndex == linePosition;

            for (int x = lineVertical ? (result == 1 ? linePosition + 1 : 0) : 0; x < (lineVertical ? (result != 1 ? linePosition + 1 : situation.tiles.GetLength(0)) : situation.tiles.GetLength(0)); x++)
            {
                for (int y = !lineVertical ? (result == 1 ? linePosition + 1 : 0) : 0; y < (lineVertical ? (result != 1 ? linePosition + 1 : situation.tiles.GetLength(1)) : situation.tiles.GetLength(1)); y++)
                {
                    airReconMap.Heat(new Vector2Int(x, y), 1.0f / (0.2f + reconResultMemory), 1);
                }
            }
        }

        airReconMap = airReconMap.GetNormalizedMap();
        situation += airReconMap * reconResultWeight;



        //Blur the map
        situation = situation.GetBlurredMap(agressivity);

        //Cool the tiles which cannot be targeted
        foreach (Tile tile in owner.hitTiles)
        {
            situation.Heat(tile.coordinates, Mathf.NegativeInfinity, 1);
        }

        //Rate the different attack possibilities
        Vector2Int[] hottestTiles = situation.GetHottestTiles(Battle.main.attackerCapabilities.maximumArtilleryCount, Mathf.Infinity);



        float artilleryHeat = 0;
        foreach (Vector2Int coord in hottestTiles)
        {
            artilleryHeat += situation.tiles[coord.x, coord.y];
        }


        int[] hottestLines = new int[Mathf.Min(Battle.main.attackerCapabilities.maximumTorpedoCount, Battle.main.attackerCapabilities.torpedoFiringAreaSize)];
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
        torpedoHeat *= (float)Battle.main.attackerCapabilities.maximumTorpedoCount / (float)Battle.main.attackerCapabilities.maximumTorpedoCount;

        //REDIRECT PLANES


        //ATTACK
        if (torpedoHeat > artilleryHeat)
        {
            Battle.main.ExecuteTorpedoAttack(hottestLines);
        }
        else
        {
            Tile[] targets = new Tile[hottestTiles.Length];
            for (int i = 0; i < hottestTiles.Length; i++)
            {
                Vector2Int coord = hottestTiles[i];
                targets[i] = target.tiles[coord.x, coord.y];
            }
            Battle.main.ExecuteArtilleryAttack(targets);
        }
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
            shipLocationHeatmaps[i] = new Heatmap(owner.board.tiles.GetLength(0), owner.board.tiles.GetLength(1));
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

        airReconMap = new Heatmap(Battle.main.defender.board.tiles.GetLength(0), Battle.main.defender.board.tiles.GetLength(1));

        //Determine the personality of this AI when attacking
        recklessness = Mathf.Pow(UnityEngine.Random.Range(0.000f, 1.000f), 3);
        float roll = UnityEngine.Random.Range(0.000f, 1.000f);
        agressivity = Mathf.Pow(roll, 0.5f + 1.5f * roll);


        reconResultWeight = 1.0f - Mathf.Pow(UnityEngine.Random.Range(0.000f, 1.000f), 4);
        reconResultMemory = Mathf.Pow(UnityEngine.Random.Range(0.000f, 1.000f), 2);
    }
}
