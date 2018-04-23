using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using Gameplay.Effects;
using Gameplay.Ships;

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

namespace Gameplay
{
    public class AIModule : ScriptableObject
    {
        [Serializable]
        public struct AIModuleData
        {
            public Heatmap airReconMap;
            public float recklessness;
            public float agressivity;
            public float reconResultWeight;
            public float reconResultMemory;
            public static implicit operator AIModuleData(AIModule module)
            {
                AIModuleData result;
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

        struct Situation
        {
            public int offset;
            public Heatmap heatmap;
            public float pressure;
            public int guns;
            public int aircraft;
            public int totalTorpedoCount;
            public int loadedTorpedoCount;
            public int destroyerDamage;
        }

        struct Plan
        {
            public Situation situation;
            public Situation reason;
            public Tile[] artilleryTargets;
            public Tile[] torpedoDroppoints;
            public Vector2Int[] torpedoHeadings;
            public int[] aircraftTargets;
        }

        public Heatmap airReconMap;
        public float recklessness;
        public float agressivity;
        public float reconResultMemory; //The air recon heatmap will be multiplied by this every turn
        public float reconResultWeight; //How much will the AI use the recon results
        void Attack()
        {
            Situation currentSituation;
            //Construct current situation
            currentSituation.offset = 0;
            //Construct current situation heatmap
            Board target = Battle.main.defender.board;

            //Construct a situation heatmap
            currentSituation.heatmap = new Heatmap(target.tiles.GetLength(0), target.tiles.GetLength(1));

            //Add heat for hit tiles
            foreach (Tile hit in owner.hitTiles)
            {
                if (hit.containedShip != null)
                {
                    if (hit.containedShip.health > 0)
                    {
                        currentSituation.heatmap.Heat(hit.coordinates, 12.0f, 1.0f - recklessness);
                    }
                }
                else
                {
                    currentSituation.heatmap.Heat(hit.coordinates, -2.0f, 1.0f - recklessness);
                }
            }

            //Use the air recon results to enhance chances of hitting
            AircraftRecon[] recon = Array.ConvertAll(Effect.GetEffectsInQueue(x => { return x.targetedPlayer != owner; }, typeof(AircraftRecon), int.MaxValue), x => { return x as AircraftRecon; });

            for (int i = 0; i < recon.GetLength(0); i++)
            {
                AircraftRecon line = recon[i];

                int linePosition = (line.target % (Battle.main.defender.board.tiles.GetLength(0) - 1));
                bool lineVertical = line.target == linePosition;

                for (int x = lineVertical ? (line.result == 1 ? linePosition + 1 : 0) : 0; x < (lineVertical ? (line.result != 1 ? linePosition + 1 : currentSituation.heatmap.tiles.GetLength(0)) : currentSituation.heatmap.tiles.GetLength(0)); x++)
                {
                    for (int y = !lineVertical ? (line.result == 1 ? linePosition + 1 : 0) : 0; y < (lineVertical ? (line.result != 1 ? linePosition + 1 : currentSituation.heatmap.tiles.GetLength(1)) : currentSituation.heatmap.tiles.GetLength(1)); y++)
                    {
                        airReconMap.Heat(new Vector2Int(x, y), 1.0f / (0.2f + reconResultMemory), 1);
                    }
                }
            }

            //Blur the map
            currentSituation.heatmap = currentSituation.heatmap.GetBlurredMap(agressivity);

            airReconMap = airReconMap.GetNormalizedMap();
            currentSituation.heatmap += airReconMap * reconResultWeight;

            //Cool the tiles which cannot be targeted
            foreach (Tile tile in owner.hitTiles)
            {
                currentSituation.heatmap.Heat(tile.coordinates, -20f, 1);
            }

            //Normalize the map
            currentSituation.heatmap = currentSituation.heatmap.GetNormalizedMap();

            //Assign other data
            currentSituation.guns = owner.arsenal.guns;
            currentSituation.aircraft = owner.arsenal.aircraft;
            currentSituation.totalTorpedoCount = owner.arsenal.torpedoes;
            currentSituation.loadedTorpedoCount = owner.arsenal.loadedTorpedoes;
            currentSituation.destroyerDamage = 0;

            //Calculate pressure
            currentSituation.pressure = 0;

            currentSituation.pressure += (1.00f - recklessness) * 30; //Base pressure for not being completely careless

            for (int i = 0; i < owner.board.ships.Length; i++) //Adds pressure for each destroyed friendly ship 
            {
                Ship ship = owner.board.ships[i];
                if (ship.health <= 0)
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





            //Plan out possible actions
            Plan[] actions = PlanForSituation(currentSituation, 5, 100f - currentSituation.pressure);

            int[] planning = new int[] { 2, 2, 2 };

            //Rate all actions by planning ahead and select the best one
            float highestRating = RatePlan(actions[0], planning);
            Plan final = actions[0];
            for (int i = 1; i < actions.Length; i++)
            {
                float rating = RatePlan(actions[i], planning);
                if (rating > highestRating)
                {
                    highestRating = rating;
                    final = actions[i];
                }
            }

            //Execute the plan
            for (int i = 0; i < final.artilleryTargets.Length; i++)
            {
                ArtilleryAttack attack = Effect.CreateEffect(typeof(ArtilleryAttack)) as ArtilleryAttack;
                attack.target = final.artilleryTargets[i];

                if (!Effect.AddToQueue(attack)) Destroy(attack.gameObject);
            }

            for (int i = 0; i < final.torpedoDroppoints.Length; i++)
            {
                TorpedoAttack attack = Effect.CreateEffect(typeof(TorpedoAttack)) as TorpedoAttack;
                attack.torpedoDropPoint = final.torpedoDroppoints[i];
                attack.torpedoHeading = final.torpedoHeadings[i];

                if (!Effect.AddToQueue(attack)) Destroy(attack.gameObject);
            }

            for (int i = 0; i < final.aircraftTargets.Length; i++)
            {
                AircraftRecon r = Effect.CreateEffect(typeof(AircraftRecon)) as AircraftRecon;
                r.target = final.aircraftTargets[i];

                if (!Effect.AddToQueue(r)) Destroy(r.gameObject);
            }


            Battle.main.NextTurn();
        }

        /// <summary>
        /// Rates a plan based on how effective it is and what can be done afterwards.
        /// </summary>
        /// <param name="plan">The plan to rate.</param>
        /// <param name="planning">Planning - specifies number of options considered at each turn ahead.</param>
        /// <returns>How effective a plan is.</returns>
        float RatePlan(Plan plan, int[] planning, int progress = 0)
        {
            Situation outcome = PredictResultingSituation(plan);
            float rating = 0;

            rating -= outcome.heatmap.averageHeat;

            if (progress < planning.Length)
            {
                Plan[] followups = PlanForSituation(outcome, planning[progress], 100f - outcome.pressure);
                for (int i = 0; i < planning[progress]; i++)
                {
                    rating += RatePlan(followups[i], planning, progress + 1);
                }
            }

            return rating;
        }

        /// <summary>
        /// Creates plans to maximize damage done in a situation.
        /// </summary>
        /// <param name="situation">Source situation.</param>
        /// <param name="planCount">Number of plans to create.</param>
        /// <param name="experimentationChance">Chance that a plan will be created randomly.</param>
        /// <returns>Plans.</returns>
        Plan[] PlanForSituation(Situation situation, int planCount, float experimentationChance)
        {
            Plan[] plans = new Plan[planCount];

            for (int i = 0; i < plans.Length; i++)
            {
                plans[i].situation = situation;

                bool experimental = UnityEngine.Random.Range(0, 100) < experimentationChance;
                Situation reason = situation;
                plans[i].artilleryTargets = GetArtilleryTargets(ref reason, experimental);

                //Work in progress
                plans[i].torpedoDroppoints = new Tile[0];
                plans[i].torpedoHeadings = new Vector2Int[0];
                plans[i].aircraftTargets = new int[0];



                plans[i].reason = reason;
                experimentationChance *= 1.25f;
            }

            return plans;
        }

        Tile[] GetArtilleryTargets(ref Situation situation, bool experimental)
        {
            Vector2Int[] possiblePositions = situation.heatmap.GetExtremeTiles(situation.guns * (experimental ? 4 : 1), Mathf.Infinity, false);
            List<Tile> possibleTargets = new List<Tile>();
            for (int i = 0; i < possiblePositions.Length; i++)
            {
                possibleTargets.Add(Battle.main.defender.board.tiles[possiblePositions[i].x, possiblePositions[i].y]);
            }


            if (experimental)
            {
                List<Tile> actualTargets = new List<Tile>();
                for (int i = 0; i < situation.guns; i++)
                {
                    Tile randomTarget = possibleTargets[UnityEngine.Random.Range(0, possibleTargets.Count)];
                    actualTargets.Add(randomTarget);
                    possibleTargets.Remove(randomTarget);
                }

                return actualTargets.ToArray();
            }
            else
            {
                return possibleTargets.ToArray();
            }
        }

        /// <summary>
        /// Predicts the results a plan will have.
        /// </summary>
        /// <param name="plan"></param>
        /// <returns></returns>
        Situation PredictResultingSituation(Plan plan)
        {
            Situation outcome = plan.situation;
            outcome.offset++;
            outcome.loadedTorpedoCount -= plan.torpedoDroppoints.Length;
            outcome.totalTorpedoCount -= plan.torpedoDroppoints.Length;

            for (int i = 0; i < plan.artilleryTargets.Length; i++)
            {
                outcome.heatmap.Heat(plan.artilleryTargets[i].coordinates, -1.0f, 0.5f);
            }

            outcome.heatmap = outcome.heatmap.GetNormalizedMap();
            return outcome;
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
            recklessness = 0.7f * Mathf.Pow(UnityEngine.Random.Range(0.000f, 1.000f), 3);
            float roll = UnityEngine.Random.Range(0.000f, 1.000f);
            agressivity = 0.9f * Mathf.Pow(roll, 0.7f - 0.5f * roll);


            reconResultWeight = 1.0f - Mathf.Pow(UnityEngine.Random.Range(0.000f, 1.000f), 4);
            reconResultMemory = Mathf.Pow(UnityEngine.Random.Range(0.000f, 1.000f), 2);
        }
    }
}