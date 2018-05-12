using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

using Gameplay.Effects;
using Gameplay.Ships;

[Serializable]
public struct Heatmap
{
    public delegate void Heater(ref Heatmap map, Vector2Int affectedCoordinates);
    public const float invalidTileHeat = -100;
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
                    result += heat;
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


        Vector2Int hottestTile = intermediate.GetExtremeTiles(1)[0];
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
                AIModuleData result = new AIModuleData();
                // result.airReconMap = module.airReconMap;

                // result.recklessness = module.recklessness;
                // result.agressivity = module.agressivity;
                // result.reconResultWeight = module.reconValue;
                // result.reconResultMemory = module.reconMemory;
                return result;
            }
        }
        public Player owner;

        public virtual void Initialize(AIModuleData data)
        {
            // airReconMap = data.airReconMap;

            // recklessness = data.recklessness;
            // agressivity = data.agressivity;
            // reconMemory = data.reconResultMemory;
            // reconValue = data.reconResultWeight;
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

        public const float theoreticalHitHeat = 5.0f;
        public const float certainHitHeat = 35.0f;
        public const float destroyedTileHeat = 1.0f;
        public const float missedTileHeat = -1.75f;
        public const float hitMissHeatDropoff = 0.4f;
        public const float mapBlur = 0.7f;
        public const float reconResultMemory = 0.4f;
        public const float reconResultValue = 5.0f;
        public const float hitConfidenceThreshold = 1.8f;
        public const float variationMultiplier = 1.5f;

        struct Situation : ICloneable
        {
            public object Clone()
            {
                Situation result;

                result.map = map.Clone() as Heatmap.Heater[,,];
                result.time = time;
                result.torpedoReload = torpedoReload;
                result.torpedoCooldown = torpedoCooldown;

                result.reconMap = new Heatmap(reconMap.tiles.GetLength(0), reconMap.tiles.GetLength(1));
                result.reconMap.tiles = reconMap.tiles.Clone() as float[,];

                result.expectedEnemyShipHealth = expectedEnemyShipHealth.Clone() as int[];
                result.totalArtilleryCount = totalArtilleryCount;
                result.aircraftCooldowns = aircraftCooldowns.Clone() as int[];
                result.totalTorpedoCount = totalTorpedoCount;
                result.loadedTorpedoCount = loadedTorpedoCount;

                return result;
            }
            public Situation(Board enemyBoard, AmmoRegistry ammo)
            {
                this.time = 0;

                Vector2Int boardDimensions = new Vector2Int(enemyBoard.tiles.GetLength(0), enemyBoard.tiles.GetLength(1));

                reconMap = new Heatmap(boardDimensions.x, boardDimensions.y);
                map = new Heatmap.Heater[boardDimensions.x, boardDimensions.y, 5];

                for (int x = 0; x < boardDimensions.x; x++)
                {
                    for (int y = 0; y < boardDimensions.y; y++)
                    {
                        Tile tile = enemyBoard.tiles[x, y];
                        if (tile.hit)
                        {
                            map[x, y, 4] = (ref Heatmap m, Vector2Int pos) => { m.tiles[pos.x, pos.y] = Heatmap.invalidTileHeat; };
                            bool hit = tile.containedShip != null;
                            bool destroyed = hit && tile.containedShip.health == 0;
                            map[x, y, hit ? 3 : 2] = (ref Heatmap m, Vector2Int pos) => { m.Heat(pos, hit ? (destroyed ? destroyedTileHeat : certainHitHeat) : missedTileHeat, hitMissHeatDropoff); };
                        }
                    }
                }

                AircraftRecon[] recon = Array.ConvertAll(Effect.GetEffectsInQueue(x => { return x.targetedPlayer == enemyBoard.owner; }, typeof(AircraftRecon), int.MaxValue), x => x as AircraftRecon);

                for (int i = 0; i < recon.GetLength(0); i++)
                {
                    AircraftRecon line = recon[i];

                    int linePosition = (line.target % (Battle.main.defender.board.tiles.GetLength(0) - 1));
                    bool lineVertical = line.target == linePosition;

                    for (int x = lineVertical ? (line.result == 1 ? linePosition + 1 : 0) : 0; x < (lineVertical ? (line.result != 1 ? linePosition + 1 : map.GetLength(0)) : map.GetLength(0)); x++)
                    {
                        for (int y = !lineVertical ? (line.result == 1 ? linePosition + 1 : 0) : 0; y < (lineVertical ? (line.result != 1 ? linePosition + 1 : map.GetLength(1)) : map.GetLength(1)); y++)
                        {
                            reconMap.Heat(new Vector2Int(x, y), 1.0f / (0.2f + reconResultMemory), 1);
                        }
                    }
                }
                reconMap = reconMap.GetNormalizedMap() * reconResultValue;

                //Assign other data
                totalArtilleryCount = ammo.guns;

                aircraftCooldowns = new int[ammo.aircraft];
                for (int i = 0; i < recon.Length && i < aircraftCooldowns.Length; i++)
                {
                    aircraftCooldowns[i] = recon[i].duration;
                }

                Effect torpedoCooldown = Effect.GetEffectsInQueue(x => x.visibleTo == ammo.targetedPlayer, typeof(TorpedoCooldown), 1).FirstOrDefault();
                this.torpedoCooldown = torpedoCooldown ? torpedoCooldown.duration : 0;
                Effect torpedoReload = Effect.GetEffectsInQueue(x => x.visibleTo == ammo.targetedPlayer, typeof(TorpedoReload), 1).FirstOrDefault();
                this.torpedoReload = torpedoReload ? torpedoReload.duration : 0;

                this.totalTorpedoCount = ammo.torpedoes;
                this.loadedTorpedoCount = ammo.loadedTorpedoes;
                this.expectedEnemyShipHealth = System.Array.ConvertAll(Battle.main.defender.board.ships, x => { return x.health == 0 ? 0 : x.maxHealth; });
            }
            public int time;

            /// <summary>
            /// Map which consists of several layers to make up the final targeting map. Layer 4 - Nullification layer, 3 - Hit heating layer, 2 - Miss heating layer, 1 - Ship predictor layer, 0 - Cyclone displacement layer.
            /// </summary>
            public Heatmap.Heater[,,] map;
            public Heatmap reconMap;
            public int[] expectedEnemyShipHealth;
            public Heatmap targetingMap
            {
                get
                {
                    Heatmap result = new Heatmap(map.GetLength(0), map.GetLength(1));
                    for (int z = 0; z < map.GetLength(2); z++)
                    {
                        if (z == map.GetLength(2) - 1)
                        {
                            result += reconMap;
                            result = result.GetBlurredMap(mapBlur);
                        }

                        for (int x = 0; x < map.GetLength(0); x++)
                        {
                            for (int y = 0; y < map.GetLength(1); y++)
                            {
                                Heatmap.Heater heater = map[x, y, z];
                                if (heater != null)
                                {
                                    heater(ref result, new Vector2Int(x, y));
                                }
                            }
                        }
                    }

                    return result;
                }
            }

            const float maxHeatRating = 40.0f;
            const float torpedoReloadRating = -3.5f;
            public float rating
            {
                get
                {
                    float result = 100.0f;

                    Heatmap evaluator = targetingMap.GetNormalizedMap();

                    result += evaluator.averageHeat * maxHeatRating;
                    result += (torpedoCooldown + torpedoReload) * torpedoReloadRating;
                    for (int i = 0; i < expectedEnemyShipHealth.Length; i++)
                    {
                        result -= expectedEnemyShipHealth[i] * Battle.main.defender.board.ships[i].importanceAIValue;
                    }


                    return result;
                }
            }

            public bool IsTileShipHit(Vector2Int tile)
            {
                return map[tile.x, tile.y, 3] != null;
            }

            public bool IsTileMiss(Vector2Int tile)
            {
                return map[tile.x, tile.y, 2] != null;
            }


            public int totalArtilleryCount;
            public int[] aircraftCooldowns;
            public int totalTorpedoCount;

            public int loadedTorpedoCount;
            public int torpedoReload;
            public int torpedoCooldown;
            public Plan[] GetStrategy(int[] sequence)
            {
                Plan[] results = new Plan[sequence[0]];

                sequence = sequence.Skip(1).ToArray();

                for (int i = 0; i < results.Length; i++)
                {
                    int variation = Mathf.FloorToInt(i * variationMultiplier);

                    float torpedoPart = Mathf.Clamp01((i - results.Length / 2.0f) / (results.Length / 2.0f));
                    results[i] = new Plan(this, Mathf.CeilToInt(torpedoPart * loadedTorpedoCount), variation, sequence);
                }

                return results;
            }
        }

        struct Plan
        {
            public Plan(Situation situation, int maximumTorpedoCount, int variation, int[] sequence)
            {
                this = new Plan();

                pre_situation = (Situation)situation.Clone();

                Heatmap targetingMap = situation.targetingMap;
                TargetArtillery(targetingMap, variation);
                TargetTorpedoes(targetingMap, maximumTorpedoCount, variation);
                TargetAircraft(targetingMap, variation);

                post_situation = (Situation)pre_situation.Clone();


                PredictEventAdvancement();
                PredictAmmoConsumption();
                PredictTargetHits(targetingMap);

                if (sequence.Length > 0) successives = post_situation.GetStrategy(sequence); else successives = new Plan[0];
            }
            void TargetArtillery(Heatmap targetingMap, int variation)
            {
                Vector2Int[] targets = targetingMap.GetExtremeTiles(pre_situation.totalArtilleryCount + variation);
                targets.OrderByDescending(x => UnityEngine.Random.Range(0.0f, 100.0f));
                targets = targets.Skip(variation).ToArray();

                artilleryTargets = Array.ConvertAll(targets, x => Battle.main.defender.board.tiles[x.x, x.y]);
            }

            void TargetTorpedoes(Heatmap targetingMap, int limit, int variation)
            {
                Board defenderBoard = Battle.main.defender.board;
                limit = Mathf.Min(pre_situation.loadedTorpedoCount, limit);

                int[] possibleLanes = targetingMap.GetExtremeLanes(limit + variation);
                possibleLanes.OrderByDescending(x => UnityEngine.Random.Range(0.0f, 100.0f));
                possibleLanes = possibleLanes.Skip(variation).ToArray();

                torpedoTargets = Array.ConvertAll(possibleLanes, lane =>
                {
                    int position = lane % targetingMap.tiles.GetLength(0);

                    bool horizontal = position < lane;
                    bool reverse = UnityEngine.Random.Range(0, 2) == 0;

                    TorpedoAttack.Target result;

                    result.torpedoHeading = (horizontal ? Vector2Int.right : Vector2Int.up) * (reverse ? -1 : 1);

                    Vector2Int dropoff = new Vector2Int(position, reverse ? defenderBoard.tiles.GetLength(1) - 1 : 0);
                    if (horizontal) dropoff = new Vector2Int(dropoff.y, dropoff.x);

                    result.torpedoDropPoint = defenderBoard.tiles[dropoff.x, dropoff.y];

                    return result;
                });
            }

            void TargetAircraft(Heatmap targetingMap, int variation)
            {
                aircraftTargets = targetingMap.GetExtremeGridLines(pre_situation.aircraftCooldowns.Count(x => x == 0));
            }

            void PredictAmmoConsumption()
            {
                int firedTorpedoes = torpedoTargets.Length;
                if (firedTorpedoes > 0)
                {
                    post_situation.loadedTorpedoCount -= firedTorpedoes;
                    post_situation.totalTorpedoCount -= firedTorpedoes;

                    post_situation.torpedoReload = Effect.RetrieveEffectPrefab(typeof(TorpedoReload)).duration;
                    post_situation.torpedoCooldown = (Effect.RetrieveEffectPrefab(typeof(TorpedoCooldown)) as TorpedoCooldown).durations[firedTorpedoes];
                }
                else
                {
                    if (post_situation.torpedoCooldown > 0) post_situation.torpedoCooldown--;
                    else
                    {
                        post_situation.torpedoReload--;
                        if (post_situation.torpedoReload == 0)
                        {
                            post_situation.loadedTorpedoCount = Mathf.Clamp(post_situation.loadedTorpedoCount + 1, 0, post_situation.totalTorpedoCount);
                            post_situation.torpedoReload = Effect.RetrieveEffectPrefab(typeof(TorpedoReload)).duration;
                        }
                    }
                }

                AircraftRecon aircraftPrefab = Effect.RetrieveEffectPrefab(typeof(AircraftRecon)) as AircraftRecon;
                for (int i = 0; i < aircraftTargets.Length; i++)
                {
                    for (int x = 0; x < post_situation.aircraftCooldowns.Length; x++)
                    {
                        if (post_situation.aircraftCooldowns[x] == 0) post_situation.aircraftCooldowns[x] = aircraftPrefab.duration;
                    }
                }
            }
            void PredictEventAdvancement()
            {
                post_situation.time++;
            }
            void PredictTargetHits(Heatmap targetingMap)
            {
                List<Vector2Int> potentialHits = new List<Vector2Int>();
                potentialHits.AddRange(
                    Array.ConvertAll(artilleryTargets, x => x.coordinates)
                );

                for (int i = 0; i < torpedoTargets.Length; i++)
                {
                    Tile currentPosition = torpedoTargets[i].torpedoDropPoint;
                    while (currentPosition != null)
                    {
                        if (!potentialHits.Contains(currentPosition.coordinates)) potentialHits.Add(currentPosition.coordinates);

                        Vector2Int newCoordinates = currentPosition.coordinates + torpedoTargets[i].torpedoHeading;
                        currentPosition = (newCoordinates.x >= 0 && newCoordinates.x < post_situation.map.GetLength(0) && newCoordinates.y >= 0 && newCoordinates.y < post_situation.map.GetLength(1)) ? Battle.main.defender.board.tiles[newCoordinates.x, newCoordinates.y] : null;
                    }
                }

                Heatmap normalizedTargeting = targetingMap.GetNormalizedMap();

                List<Vector2Int> hits = potentialHits.FindAll(x => (normalizedTargeting.tiles[x.x, x.y] / normalizedTargeting.averageHeat) > hitConfidenceThreshold);
                List<Vector2Int> misses = new List<Vector2Int>();

                foreach (Vector2Int target in hits)
                {
                    int minimumShipLength = 1;
                    int maximumShipLength = 0;

                    Vector2Int estabilishedDirection = Vector2Int.zero;

                    //Determine which direction the ship being shot is pointing
                    for (int x = 0; x < 4; x++)
                    {
                        Vector2Int direction = new Vector2Int(x < 2 ? (x == 0 ? 1 : -1) : 0, x < 2 ? 0 : (x == 2 ? 1 : -1));
                        Vector2Int candidate = target + direction;

                        if (candidate.x >= 0 && candidate.y >= 0 && candidate.x < pre_situation.map.GetLength(0) && candidate.y < pre_situation.map.GetLength(1))
                        {
                            if (pre_situation.IsTileShipHit(candidate))
                            {
                                estabilishedDirection = direction;
                                break;
                            }
                        }
                    }

                    //Determine the minimum and maximum length of the ship that was theoretically hit
                    for (int directional = (estabilishedDirection.y == 0 ? 0 : 1); directional < (estabilishedDirection.x == 0 ? 2 : 1); directional++)
                    {
                        int maximumDirectionLength = 0;
                        int minimumDirectionLength = 0;

                        bool pastTarget = false;
                        bool chaining = estabilishedDirection != Vector2Int.zero;

                        for (int d = 0; d < pre_situation.map.GetLength(directional); d++)
                        {
                            Vector2Int examined = new Vector2Int(directional == 0 ? d : target.x, directional == 0 ? target.y : d);

                            if (examined == target) pastTarget = true;

                            if (pre_situation.IsTileMiss(examined) || pre_situation.IsTileShipHit(examined))
                            {
                                if (pastTarget) break;
                                else maximumDirectionLength = 0;
                            }
                            else maximumDirectionLength++;



                            if (chaining)
                            {
                                if (!(pre_situation.IsTileShipHit(examined) || examined == target))
                                {
                                    if (pastTarget) chaining = false;
                                    else minimumDirectionLength = 1;
                                }
                                else minimumDirectionLength++;
                            }
                        }

                        if (maximumDirectionLength > maximumShipLength) maximumShipLength = maximumDirectionLength;
                        if (minimumDirectionLength > minimumShipLength) minimumShipLength = minimumDirectionLength;
                    }

                    int longestPossibleLurkingShipIndex = -1;
                    int longestPossibleLurkingShipLength = -1;

                    for (int i = 0; i < Battle.main.defender.board.ships.Length; i++)
                    {
                        Ship candidate = Battle.main.defender.board.ships[i];
                        if (pre_situation.expectedEnemyShipHealth[i] > 0 && candidate.maxHealth >= minimumShipLength && candidate.maxHealth <= maximumShipLength)
                        {
                            if (candidate.maxHealth > longestPossibleLurkingShipLength)
                            {
                                longestPossibleLurkingShipIndex = i;
                                longestPossibleLurkingShipLength = candidate.maxHealth;
                            }
                        }
                    }

                    if (longestPossibleLurkingShipIndex >= 0) post_situation.expectedEnemyShipHealth[longestPossibleLurkingShipIndex]--; else misses.Add(target);
                }



                foreach (Vector2Int candidate in potentialHits)
                {
                    post_situation.map[candidate.x, candidate.y, 4] = (ref Heatmap m, Vector2Int pos) => { m.tiles[pos.x, pos.y] = Heatmap.invalidTileHeat; };
                    bool hit = hits.Contains(candidate) && !misses.Contains(candidate);
                    post_situation.map[candidate.x, candidate.y, hit ? 3 : 2] = (ref Heatmap m, Vector2Int pos) => { m.Heat(pos, hit ? theoreticalHitHeat : missedTileHeat, hitMissHeatDropoff); };
                }
            }
            public Situation pre_situation;
            public Situation post_situation;
            public Plan[] successives;
            public float rating
            {
                get
                {
                    return successives.Length > 0 ? successives.Max(x => x.rating) : post_situation.rating;
                }
            }

            public Tile[] artilleryTargets;
            public TorpedoAttack.Target[] torpedoTargets;
            public int[] aircraftTargets;
        }

        void Attack()
        {
            Plan[] plans = new Situation(Battle.main.defender.board, owner.arsenal).GetStrategy(new int[] { 6, 1, 2 });

            Heatmap e = plans[0].post_situation.targetingMap;

            //RenderDebugPlanTree(plans, Vector3.up * 20, 230f, 40f);

            Debug.Log("--------------------------PLANS--------------------");

            for (int i = 0; i < plans.Length; i++)
            {
                Plan plan = plans[i];
                Debug.Log("Plan " + i + " rating " + plan.rating + " guns " + plan.artilleryTargets.Length + " torpedoes " + plan.torpedoTargets.Length);
            }

            ExecutePlan(plans.OrderByDescending(x => x.rating).First()); //3. Execute the best plan
        }

        void RenderDebugPlanTree(Plan[] plans, Vector3 linkPoint, float space, float layerSpacing)
        {
            float spacing = plans.Length > 1 ? space / (plans.Length - 1) : 0;
            Vector3 startingPosition = linkPoint + new Vector3(layerSpacing, 0, space / 2.0f);

            space /= (float)plans.Length * 1.2f;

            for (int i = 0; i < plans.Length; i++)
            {
                Plan plan = plans[i];

                Vector3 centerPosition = startingPosition + Vector3.back * i * spacing;
                Vector3 boardCornerPosition = centerPosition - new Vector3(Battle.main.defender.board.tiles.GetLength(0), 0, Battle.main.defender.board.tiles.GetLength(1));
                Situation renderedSituation = plan.post_situation;
                Heatmap targetMap = renderedSituation.targetingMap;
                Heatmap heatmap = targetMap.GetNormalizedMap();

                Debug.DrawLine(linkPoint, boardCornerPosition, Color.blue, Mathf.Infinity, true);

                for (int x = 0; x < renderedSituation.map.GetLength(0); x++)
                {
                    for (int y = 0; y < renderedSituation.map.GetLength(1); y++)
                    {
                        Vector3 lineBeginningPosition = boardCornerPosition + new Vector3(x, 0, y);
                        float cubeHeight = heatmap.tiles[x, y] * 0.5f + 0.002f;
                        // Debug.DrawLine(lineBeginningPosition, lineBeginningPosition + Vector3.up * cubeHeight, renderedSituation.map[x, y, 3] != null ? Color.red : Color.black, Mathf.Infinity, true);

                        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

                        cube.transform.position = lineBeginningPosition + Vector3.up * cubeHeight / 2.0f;
                        cube.transform.localScale = new Vector3(1, cubeHeight, 1);

                        Renderer r = cube.GetComponent<Renderer>();

                        MaterialPropertyBlock block = new MaterialPropertyBlock();
                        block.SetColor("_Color", renderedSituation.IsTileShipHit(new Vector2Int(x, y)) ? Color.red : (plan.artilleryTargets.Any(t => t.coordinates == new Vector2Int(x, y)) ? Color.blue : (renderedSituation.IsTileMiss(new Vector2Int(x, y)) ? Color.black : Color.white)));

                        r.SetPropertyBlock(block);
                    }
                }

                Vector3 lPoint = centerPosition + Vector3.right * Battle.main.defender.board.tiles.GetLength(0) * 1.1f;
                if (plan.successives.Length > 0) RenderDebugPlanTree(plan.successives, lPoint, space, layerSpacing);
            }
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

            // airReconMap = new Heatmap(Battle.main.defender.board.tiles.GetLength(0), Battle.main.defender.board.tiles.GetLength(1));

            // //Determine the personality of this AI when attacking
            // recklessness = 0.7f * Mathf.Pow(UnityEngine.Random.Range(0.000f, 1.000f), 3);
            // float roll = UnityEngine.Random.Range(0.000f, 1.000f);
            // agressivity = 0.9f * Mathf.Pow(roll, 0.7f - 0.5f * roll);


            // reconValue = 1.0f - Mathf.Pow(UnityEngine.Random.Range(0.000f, 1.000f), 4);
            // reconMemory = Mathf.Pow(UnityEngine.Random.Range(0.000f, 1.000f), 2);
        }
    }
}