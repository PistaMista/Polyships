using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
[Serializable]
public struct Heatmap
{
    public float[,] tiles;
    public float[] verticalLines
    {
        get
        {
            float[] result = new float[tiles.GetLength(0)];

            for (int x = 0; x < result.Length; x++)
            {
                float sum = 0;
                for (int y = 0; y < tiles.GetLength(1); y++)
                {
                    sum += tiles[x, y];
                }

                result[x] = sum;
            }

            return result;
        }
    }
    public float[] horizontalLines
    {
        get
        {
            float[] result = new float[tiles.GetLength(1)];

            for (int y = 0; y < result.Length; y++)
            {
                float sum = 0;
                for (int x = 0; x < tiles.GetLength(0); x++)
                {
                    sum += tiles[x, y];
                }

                result[y] = sum;
            }

            return result;
        }
    }
    public float[] gridLines
    {
        get
        {
            float[] result = new float[tiles.GetLength(0) + tiles.GetLength(1) - 2];
            float[] horizontalLines = this.horizontalLines;
            float[] verticalLines = this.verticalLines;

            for (int i = 0; i < result.Length; i++)
            {
                float[] inspectedArray = i < (tiles.GetLength(0) - 1) ? verticalLines : horizontalLines;
                int index = i % (tiles.GetLength(0) - 1);

                result[i] = inspectedArray[index] + inspectedArray[index + 1];
            }
            return result;
        }
    }

    public float totalHeat
    {
        get
        {
            float result = 0;
            for (int x = 0; x < tiles.GetLength(0); x++)
            {
                for (int y = 0; y < tiles.GetLength(1); y++)
                {
                    result += tiles[x, y];
                }
            }

            return result;
        }
    }

    public Heatmap(int dimX, int dimY)
    {
        tiles = new float[dimX, dimY];
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
        Vector2Int coldestTile = GetExtremeTiles(1, Mathf.NegativeInfinity, true)[0];
        float lowestHeat = tiles[coldestTile.x, coldestTile.y];

        for (int x = 0; x < tiles.GetLength(0); x++)
        {
            for (int y = 0; y < tiles.GetLength(1); y++)
            {
                intermediate.Heat(new Vector2Int(x, y), tiles[x, y] - lowestHeat, 1);
            }
        }


        Vector2Int hottestTile = intermediate.GetExtremeTiles(1, Mathf.Infinity, false)[0];
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

    public Vector2Int[] GetExtremeTiles(int count, float threshold, bool coldest)
    {
        return Utilities.GetExtremeArrayElements(tiles, count, coldest, threshold);
    }

    public int[] GetExtremeGridLines(int count, float threshold, bool coldest)
    {
        return Utilities.GetExtremeArrayElements(gridLines, count, coldest, threshold);
    }

    public int[] GetExtremeVerticalLines(int count, float threshold, bool coldest)
    {
        return Utilities.GetExtremeArrayElements(verticalLines, count, coldest, threshold);
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

        //Blur the map
        situation = situation.GetBlurredMap(agressivity);

        airReconMap = airReconMap.GetNormalizedMap();
        situation += airReconMap * reconResultWeight;

        //Cool the tiles which cannot be targeted
        foreach (Tile tile in owner.hitTiles)
        {
            situation.Heat(tile.coordinates, -20f, 1);
        }

        //Normalize the map
        situation = situation.GetNormalizedMap();


        //Evaluate torpedo possibility
        float[] verticalLines = situation.verticalLines;
        for (int i = 0; i < verticalLines.Length; i++)
        {
            if (!Battle.main.attackerCapabilities.torpedoFiringArea[i])
            {
                verticalLines[i] = Mathf.NegativeInfinity;
            }
        }

        int[] torpedoCandidates = Utilities.GetExtremeArrayElements(verticalLines, Battle.main.attackerCapabilities.maximumTorpedoCount, false, Mathf.Infinity);
        float torpedoHeat = 0;
        float[] gridLanes = situation.verticalLines;
        for (int i = 0; i < torpedoCandidates.Length; i++)
        {
            torpedoHeat += gridLanes[torpedoCandidates[i]];
        }

        torpedoHeat /= Battle.main.attackerCapabilities.maximumArtilleryCount;

        //Evaluate artillery possibility
        Vector2Int[] artilleryCandidates = situation.GetExtremeTiles(Battle.main.attackerCapabilities.maximumArtilleryCount, Mathf.Infinity, false);
        float artilleryHeat = 0;
        foreach (Vector2Int coord in artilleryCandidates)
        {
            artilleryHeat += situation.tiles[coord.x, coord.y];
        }


        //REDIRECT PLANES
        float[] gridLines = situation.gridLines;
        float worstLine = gridLines[Utilities.GetExtremeArrayElements(gridLines, 1, true, Mathf.NegativeInfinity)[0]];
        for (int i = 0; i < gridLines.Length; i++)
        {
            gridLines[i] -= worstLine;
        }

        float[] targetProbabilities = new float[gridLines.Length];
        float probabilityConstructorSum = 0;
        for (int i = 0; i < gridLines.Length; i++)
        {
            float value = gridLines[i];
            probabilityConstructorSum += value + 1.00f;

            targetProbabilities[i] = probabilityConstructorSum;
        }

        int[] planeTargets = new int[Battle.main.attackerCapabilities.maximumAircraftCount];
        for (int i = 0; i < planeTargets.Length; i++)
        {
            float roll = UnityEngine.Random.Range(0.00f, targetProbabilities[targetProbabilities.Length - 1]);
            int selectedLine = -1;

            for (int x = 0; x < targetProbabilities.Length; x++)
            {
                if (roll < targetProbabilities[x])
                {
                    if (selectedLine == -1)
                    {
                        selectedLine = x;
                    }

                    targetProbabilities[x] -= gridLines[selectedLine];
                }
            }

            planeTargets[i] = selectedLine;
        }

        List<int> toAssign = new List<int>(planeTargets);
        foreach (Ship ship in owner.board.ships)
        {
            if (ship.type == ShipType.CARRIER)
            {
                Carrier carrier = (Carrier)ship;
                carrier.reconTargets = toAssign.GetRange(0, Mathf.Clamp(carrier.aircraftCount, 0, toAssign.Count)).ToArray();
                toAssign.RemoveRange(0, Mathf.Clamp(carrier.aircraftCount, 0, toAssign.Count));
            }
        }

        //ATTACK
        if (torpedoHeat > artilleryHeat)
        {
            Battle.main.ExecuteTorpedoAttack(torpedoCandidates);
        }
        else
        {
            Tile[] targets = new Tile[artilleryCandidates.Length];
            for (int i = 0; i < artilleryCandidates.Length; i++)
            {
                Vector2Int coord = artilleryCandidates[i];
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
