﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

using Gameplay.Effects;
using Gameplay.Ships;

[Serializable]
public struct Heatmap
{
    public const float invalidTileHeat = -10000;
    public float[,] tiles;
    public float[] verticalLanes
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
    public float[] horizontalLanes
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
    public float[] gridLanes
    {
        get
        {
            float[] vertical = verticalLanes;
            float[] horizontal = horizontalLanes;

            float[] results = new float[tiles.GetLength(0) + tiles.GetLength(1)];
            System.Array.Copy(vertical, 0, results, 0, vertical.Length);
            System.Array.Copy(horizontal, 0, results, tiles.GetLength(0), horizontal.Length);

            return results;
        }
    }
    public float[] gridLines
    {
        get
        {
            float[] result = new float[tiles.GetLength(0) + tiles.GetLength(1) - 2];
            float[] horizontalLines = this.horizontalLanes;
            float[] verticalLines = this.verticalLanes;

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
                    float heat = tiles[x, y];
                    if (heat > invalidTileHeat) result += heat;
                }
            }

            return result;
        }
    }

    public float averageHeat
    {
        get
        {
            return totalHeat / (tiles.GetLength(0) * tiles.GetLength(1));
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

    public Vector2Int[] GetExtremeTiles(int count = int.MaxValue, float threshold = float.MaxValue, bool coldest = false)
    {
        return Utilities.GetExtremeArrayElements(tiles, count, coldest, threshold);
    }

    public int[] GetExtremeGridLines(int count = int.MaxValue, float threshold = float.MaxValue, bool coldest = false)
    {
        return Utilities.GetExtremeArrayElements(gridLines, count, coldest, threshold);
    }

    public int[] GetExtremeLanes(int count = int.MaxValue, float threshold = float.MaxValue, bool coldest = false)
    {
        return Utilities.GetExtremeArrayElements(gridLanes, count, coldest, threshold);
    }
}

namespace Gameplay
{
    public class AI : ScriptableObject
    {
        [Serializable]
        public struct AIModuleData
        {
            public Heatmap airReconMap;
            public float recklessness;
            public float agressivity;
            public float reconResultWeight;
            public float reconResultMemory;
            public static implicit operator AIModuleData(AI module)
            {
                AIModuleData result;
                result.airReconMap = module.airReconMap;

                result.recklessness = module.recklessness;
                result.agressivity = module.agressivity;
                result.reconResultWeight = module.reconValue;
                result.reconResultMemory = module.reconMemory;
                return result;
            }
        }
        public Player owner;

        public virtual void Initialize(AIModuleData data)
        {
            airReconMap = data.airReconMap;

            recklessness = data.recklessness;
            agressivity = data.agressivity;
            reconMemory = data.reconResultMemory;
            reconValue = data.reconResultWeight;
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

        public const float hitTileHeat = 10.0f;
        public const float missedTileHeat = -2.0f;

        struct Situation
        {
            public int time;

            public bool[,,] certaintyMap;
            public Heatmap[] shipPositionMaps;
            public Heatmap reconMap;
            public Heatmap priorityMap
            {
                get
                {

                }
            }

            public float rating
            {
                get
                {
                    float result = 0.0f;

                    result += shipPositionMaps.Average(x => x.averageHeat);
                    result -= torpedoCooldown + torpedoReload;

                    return result;
                }
            }

            public int[] expectedEnemyShipHealth;


            public float pressure;
            public int totalArtilleryCount;
            public int[] aircraftCooldowns;
            public int totalTorpedoCount;

            public int loadedTorpedoCount;
            public int torpedoReload;
            public int torpedoCooldown;
            public int destroyerDamage;
        }

        struct Plan
        {
            public static Plan[] GetSituationStrategy(Situation situation, int[] sequence)
            {
                Plan[] results = new Plan[sequence[0]];

                sequence = sequence.Skip(1).ToArray();

                for (int i = 0; i < results.Length; i++)
                {
                    float torpedoPart = Mathf.Clamp01((i - results.Length / 2.0f) / (results.Length / 2.0f));
                    results[i] = new Plan(situation, Mathf.CeilToInt(torpedoPart * situation.loadedTorpedoCount), i, sequence);
                }

                return results;
            }

            Plan(Situation situation, int maximumTorpedoCount, int variability, int[] sequence)
            {
                this = new Plan();

                pre_situation = situation;
                TargetArtillery();
                TargetTorpedoes();
                TargetAircraft();

                PredictOutcome();

                if (successives.Length > 0) successives = GetSituationStrategy(post_situation, sequence); else successives = new Plan[0];
            }
            void TargetArtillery()
            {

            }

            void TargetTorpedoes()
            {

            }

            void TargetAircraft()
            {

            }

            void PredictOutcome()
            {

            }
            public Situation pre_situation;
            public Situation post_situation;
            public Plan[] successives;
            public float rating
            {
                get
                {
                    float result = post_situation.rating;
                    for (int i = 0; i < successives.Length; i++)
                    {
                        result += successives[i].rating;
                    }

                    return result;
                }
            }

            public Tile[] artilleryTargets;
            public TorpedoAttack.Target[] torpedoTargets;
            public int[] aircraftTargets;
        }

        void Attack()
        {
            Situation currentSituation = ReconstructCurrentSituation(); //1. Reconstruct the current situation map
            Plan[] plans = Plan.GetSituationStrategy(currentSituation, new int[] { 6, 2, 2, 2 });
            ExecutePlan(plans.OrderByDescending(x => x.rating).First()); //3. Execute the best plan
        }

        /// <summary>
        /// Executes a final plan and ends the turn.
        /// </summary>
        /// <param name="plan"></param>
        void ExecutePlan(Plan plan)
        {
            for (int i = 0; i < plan.artilleryTargets.Length; i++)
            {
                ArtilleryAttack attack = Effect.CreateEffect(typeof(ArtilleryAttack)) as ArtilleryAttack;
                attack.target = plan.artilleryTargets[i];
                attack.targetedPlayer = Battle.main.defender;
                attack.visibleTo = owner;

                if (!Effect.AddToQueue(attack)) Destroy(attack.gameObject);
            }

            for (int i = 0; i < plan.torpedoTargets.Length; i++)
            {
                TorpedoAttack attack = Effect.CreateEffect(typeof(TorpedoAttack)) as TorpedoAttack;
                attack.target = plan.torpedoTargets[i];
                attack.targetedPlayer = Battle.main.defender;
                attack.visibleTo = owner;

                if (!Effect.AddToQueue(attack)) Destroy(attack.gameObject);
            }

            for (int i = 0; i < plan.aircraftTargets.Length; i++)
            {
                AircraftRecon r = Effect.CreateEffect(typeof(AircraftRecon)) as AircraftRecon;
                r.target = plan.aircraftTargets[i];
                r.targetedPlayer = Battle.main.defender;
                r.visibleTo = owner;

                if (!Effect.AddToQueue(r)) Destroy(r.gameObject);
            }


            Battle.main.NextTurn();
        }

        /// <summary>
        /// Takes the current state of battle and fills out a situation form.
        /// </summary>
        /// <returns>Situation form.</returns>
        Situation ReconstructCurrentSituation()
        {
            Situation currentSituation;
            //Construct current situation
            currentSituation.time = 0;
            //Construct current situation heatmap
            Board target = Battle.main.defender.board;

            //Construct the base maps
            Vector2Int boardDimensions = new Vector2Int(target.tiles.GetLength(0), target.tiles.GetLength(1));
            currentSituation.probabilityMap = new Heatmap(boardDimensions.x, boardDimensions.y);
            currentSituation.reconMap = new Heatmap(boardDimensions.x, boardDimensions.y);

            currentSituation.certaintyMap = new bool[2, boardDimensions.x, boardDimensions.y];
            currentSituation.destroyedTileMap = new bool[boardDimensions.x, boardDimensions.y];

            currentSituation.enemyShipImportance = new int[Battle.main.defender.board.ships.Length];

            foreach (Tile tile in owner.hitTiles)
            {
                currentSituation.certaintyMap[tile.containedShip != null ? 1 : 0, tile.coordinates.x, tile.coordinates.y] = true;
            }

            //Assign other data
            currentSituation.totalArtilleryCount = owner.arsenal.guns;

            currentSituation.aircraftCooldowns = new int[owner.arsenal.aircraft];
            AircraftRecon[] deployedPlanes = System.Array.ConvertAll(Effect.GetEffectsInQueue(x => x.visibleTo == owner, typeof(AircraftRecon), owner.arsenal.aircraft), x => x as AircraftRecon);
            for (int i = 0; i < deployedPlanes.Length; i++)
            {
                currentSituation.aircraftCooldowns[i] = deployedPlanes[i].duration;
            }

            Effect torpedoCooldown = Effect.GetEffectsInQueue(x => x.visibleTo == owner, typeof(TorpedoCooldown), 1).FirstOrDefault();
            currentSituation.torpedoCooldown = torpedoCooldown ? torpedoCooldown.duration : 0;
            Effect torpedoReload = Effect.GetEffectsInQueue(x => x.visibleTo == owner, typeof(TorpedoReload), 1).FirstOrDefault();
            currentSituation.torpedoReload = torpedoReload ? torpedoReload.duration : 0;

            currentSituation.totalTorpedoCount = owner.arsenal.torpedoes;
            currentSituation.loadedTorpedoCount = owner.arsenal.loadedTorpedoes;
            currentSituation.destroyerDamage = 0;

            //Assign expected ship health
            currentSituation.expectedEnemyShipHealth = System.Array.ConvertAll(Battle.main.defender.board.ships, x => { return x.health == 0 ? 0 : x.maxHealth; });

            //Calculate pressure
            currentSituation.pressure = 0;

            currentSituation.pressure += (1.00f - recklessness) * 30; //Base pressure for not being completely careless

            for (int i = 0; i < owner.board.ships.Length; i++) //Adds pressure for each damaged/destroyed friendly ship 
            {
                Ship ship = owner.board.ships[i];
                if (ship.health < ship.maxHealth)
                {
                    //The harder the ship is to hit, the more valuable it is
                    switch (ship.maxHealth)
                    {
                        case 5:
                            currentSituation.pressure += 10;
                            break;
                        case 4:
                            currentSituation.pressure += 12;
                            break;
                        case 3:
                            currentSituation.pressure += 16;
                            break;
                        case 2:
                            currentSituation.pressure += 28;
                            break;
                    }
                }
                else
                {
                    if (ship is Destroyer)
                    {
                        currentSituation.destroyerDamage += ship.maxHealth - ship.health;
                    }
                }
            }

            currentSituation.pressure += currentSituation.destroyerDamage * 20;

            return currentSituation;
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

            //2.Tactic - Camouflage
            float concealmentAccuracyValue = 1.0f - (float)Math.Pow(UnityEngine.Random.Range(0.000f, 1.000f), 4);
            List<int> cruiserIDs = new List<int>();
            for (int i = 0; i < owner.board.ships.Length; i++)
            {
                if (owner.board.ships[i] is Cruiser)
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

                if (ship is Cruiser)
                {
                    (ship as Cruiser).ConcealAlreadyPlacedShipsInConcealmentArea();
                }

                if (owner.board.placementInfo.selectableTiles.Count == 0)
                {
                    PlaceShips();
                    break;
                }
            }

            airReconMap = new Heatmap(Battle.main.defender.board.tiles.GetLength(0), Battle.main.defender.board.tiles.GetLength(1));

            //Determine the personality of this AI when attacking
            recklessness = 0.7f * Mathf.Pow(UnityEngine.Random.Range(0.000f, 1.000f), 3);
            float roll = UnityEngine.Random.Range(0.000f, 1.000f);
            agressivity = 0.9f * Mathf.Pow(roll, 0.7f - 0.5f * roll);


            reconValue = 1.0f - Mathf.Pow(UnityEngine.Random.Range(0.000f, 1.000f), 4);
            reconMemory = Mathf.Pow(UnityEngine.Random.Range(0.000f, 1.000f), 2);
        }
    }
}