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
                    float heat = tiles[x, y];
                    if (heat > float.MinValue) sum += heat;
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
                    float heat = tiles[x, y];
                    if (heat > float.MinValue) sum += heat;
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
                    if (heat > float.MinValue) result += heat;
                }
            }

            return result;
        }
    }

    public float averageHeat
    {
        get
        {
            int validTiles = 0;
            for (int x = 0; x < tiles.GetLength(0); x++)
            {
                for (int y = 0; y < tiles.GetLength(1); y++)
                {
                    if (tiles[x, y] > float.MinValue) validTiles++;
                }
            }

            return totalHeat / validTiles;
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

                        result.tiles[coord.x, coord.y] = storedHeat;

                        float heat = tiles[coord.x, coord.y];
                        storedHeat += heat;
                    }
                }
            }
        }

        return result;
    }

    public Heatmap normalized
    {
        get
        {
            Heatmap result = new Heatmap(tiles.GetLength(0), tiles.GetLength(1));

            Heatmap intermediate = new Heatmap(tiles.GetLength(0), tiles.GetLength(1));
            Vector2Int coldestTile = GetExtremeTiles(1, float.MinValue, true)[0];
            float lowestHeat = tiles[coldestTile.x, coldestTile.y];

            for (int x = 0; x < tiles.GetLength(0); x++)
            {
                for (int y = 0; y < tiles.GetLength(1); y++)
                {
                    float heat = tiles[x, y];
                    intermediate.tiles[x, y] = heat < float.MinValue ? 0 : heat - lowestHeat;
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
                    result.tiles[x, y] = intermediate.tiles[x, y] / highestHeat;
                }
            }

            return result;
        }
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
        public Player owner;
        /*
            Transcript
        Process
        I. Package the current situation into its data structure.
        II. Get several strategies for that situation.
            1. Are there any unfinished plans?
            YES:
                1. Create a new plan.
                    1.Copy the situation onto the plan to let it manipulate it freely.
                    2.Pick the ideal target from the targeting map.
                    3.Is it possible to blacklist?
                    YES:
                     4.Blacklist it and any other considered invalid tiles in the transmap - set their value to -infinity.
                     5.Discard the target and start again.
                    NO:
                     4.Confirm the target.
                     5.Apply contextual transmap heat - ranging from -1 to 1.
                    6. Branch the plan further - get strategy for the resulting situation (II.)
                2. Go to II. -> 1.
            NO:
                1. Rate each plan.
        */
        public const float missHeat = -1.0f;
        public const float hitHeat = 1.0f;
        public const float destructionHeat = 1.0f;
        public const float heatDropoff = 0.4f;
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

        struct Maptile
        {
            public Maptile(Datamap parent, Vector2Int coordinates)
            {
                this.parent = parent;
                this.coordinates = coordinates;
                hit = false;
                containedShipID = -1;
            }
            Vector2Int coordinates;
            Datamap parent;
            public bool hit;
            public int containedShipID;
            public int ContainedShipHealth
            {
                get
                {
                    return containedShipID >= 0 ? parent.health[containedShipID] : -1;
                }
            }
        }

        struct Datamap
        {
            public int[] health;
            Maptile[,] tiledata;
            bool[,] blackmap;

            public bool[,] Blackmap
            {
                get
                {
                    return blackmap;
                }
            }

            public Maptile[,] Tiledata
            {
                get
                {
                    return tiledata;
                }
            }


        }

        struct Situation
        {
            /// <summary>
            /// Used to determine heatmap - provides information about current board status.
            /// </summary>
            public Datamap datamap;
            /// <summary>
            /// Used to determine targetmap - provides transitional information about what tiles are prefered for targeting. Uses values from -1 to 1.
            /// </summary>
            public Heatmap heatmap_transitional;

            /// <summary>
            /// Used to determine targetmap - provides ABSOLUTE values about the likelyhood of a ship occupying any given tile. DOES NOT take placement rules into account. Shouldn't be modified by anything.
            /// </summary>
            public Heatmap heatmap_statistical;
            public void ConstructStatisticalHeatmap()
            {
                heatmap_statistical = new Heatmap(datamap.Tiledata.GetLength(0), datamap.Tiledata.GetLength(1));
                for (int x = 0; x < datamap.Tiledata.GetLength(0); x++)
                {
                    for (int y = 0; y < datamap.Tiledata.GetLength(1); y++)
                    {
                        Maptile tile = datamap.Tiledata[x, y];
                        if (tile.hit) heatmap_statistical.Heat(new Vector2Int(x, y), tile.containedShipID >= 0 ? (tile.ContainedShipHealth <= 0 ? destructionHeat : hitHeat) : missHeat, heatDropoff);
                    }
                }
            }
            /// <summary>
            /// Used to determine best attack options - provides NORMALIZED values about the likelyhood of a ship occupying any given tile. DOES take placement rules into account.
            /// </summary>
            public Heatmap targetmap;
            public void ConstructTargetmap()
            {
                ConstructStatisticalHeatmap();

                //Combine maps
                targetmap = (heatmap_statistical.normalized + heatmap_transitional);

                for (int x = 0; x < targetmap.tiles.GetLength(0); x++)
                {
                    for (int y = 0; y < targetmap.tiles.GetLength(1); y++)
                    {
                        if (datamap.Blackmap[x, y]) targetmap.tiles[x, y] = Mathf.NegativeInfinity;
                    }
                }

                targetmap = targetmap.normalized;
            }


            public Plan[] GetStrategy(int[] sequence)
            {
                Plan[] results = new Plan[sequence[0]];

                sequence = sequence.Skip(1).ToArray();

                for (int i = 0; i < results.Length; i++)
                {
                    results[i] = new Plan(this, ref heatmap_transitional, sequence);
                }
                return results;
            }
        }

        struct Plan
        {
            Situation situation;
            /// <summary>
            /// Creates a new plan for a source situation.
            /// </summary>
            /// <param name="state">Situation to be planned on.</param>
            /// <param name="heatmap_transitional">References the transmap of the previous layer.</param>
            /// <param name="sequence">The branching of plans considered ahead.</param>
            public Plan(Situation state, ref Heatmap heatmap_transitional, int[] sequence)
            {
                this = new Plan();

                situation = state.Clone(); //The resulting situation is based on the starting situation.

                /*
                    Targeting
                Every targeting procedure will work as follows:
                1.Pick the ideal target from the targeting map.
                2.Is it possible to blacklist?
                YES:
                 3.Blacklist it and any other considered invalid tiles in the transmap - set their value to -infinity.
                 4.Discard the target and start again.
                NO:
                 3.Confirm the target.
                 4.Apply contextual transmap heat - ranging from -1 to 1.

                    Blacklisting
                Sets a value in the transmap reference parameter to -infinity. This will inform other plans to ignore these tiles.
                    Contextual heating (contextual heat)
                Applies heat to the transmap informing other plans to shoot somewhere else. Also informs other attacks in THIS plan to go elsewhere if there is a cyclone or something similar.
                */

                TargetArtillery();

                TargetTorpedoes();

                TargetAircraft();


                heatmap_transitional = situation.heatmap_transitional; //Set the previous plan's transmap to the result's transmap

                if (sequence.Length > 0) successives = situation.GetStrategy(sequence); else successives = new Plan[0];
            }

            public void TargetArtillery()
            {

            }

            public void TargetTorpedoes()
            {

            }

            public void TargetAircraft()
            {

            }

            public Plan[] successives;
        }

        void Attack()
        {
            Situation situation;

            Plan[] plans = situation.GetStrategy(new int[] { 4, 2, 2 });

        }

        // void RenderDebugPlanTree(Plan[] plans, Vector3 linkPoint, float space, float layerSpacing)
        // {
        //     float spacing = plans.Length > 1 ? space / (plans.Length - 1) : 0;
        //     Vector3 startingPosition = linkPoint + new Vector3(layerSpacing, 0, space / 2.0f);

        //     space /= (float)plans.Length * 1.2f;

        //     for (int i = 0; i < plans.Length; i++)
        //     {
        //         Plan plan = plans[i];

        //         Vector3 centerPosition = startingPosition + Vector3.back * i * spacing;
        //         Vector3 boardCornerPosition = centerPosition - new Vector3(Battle.main.defender.board.tiles.GetLength(0), 0, Battle.main.defender.board.tiles.GetLength(1));
        //         Situation renderedSituation = plan.post_situation;
        //         Heatmap targetMap = plan.pre_situation.targetingMap;
        //         Heatmap heatmap = targetMap.GetNormalizedMap();

        //         Debug.DrawLine(linkPoint, boardCornerPosition, Color.blue, Mathf.Infinity, true);

        //         for (int x = 0; x < renderedSituation.map.GetLength(0); x++)
        //         {
        //             for (int y = 0; y < renderedSituation.map.GetLength(1); y++)
        //             {
        //                 Vector3 lineBeginningPosition = boardCornerPosition + new Vector3(x, 0, y);
        //                 float cubeHeight = heatmap.tiles[x, y] * 0.5f + 0.002f;
        //                 // Debug.DrawLine(lineBeginningPosition, lineBeginningPosition + Vector3.up * cubeHeight, renderedSituation.map[x, y, 3] != null ? Color.red : Color.black, Mathf.Infinity, true);

        //                 GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

        //                 cube.transform.position = lineBeginningPosition + Vector3.up * cubeHeight / 2.0f;
        //                 cube.transform.localScale = new Vector3(1, cubeHeight, 1);

        //                 Renderer r = cube.GetComponent<Renderer>();

        //                 MaterialPropertyBlock block = new MaterialPropertyBlock();
        //                 block.SetColor("_Color", renderedSituation.IsTileShipHit(new Vector2Int(x, y)) ? Color.red : (plan.artilleryTargets.Any(t => t.coordinates == new Vector2Int(x, y)) ? Color.blue : (renderedSituation.IsTileMiss(new Vector2Int(x, y)) ? Color.black : Color.white)));

        //                 r.SetPropertyBlock(block);
        //             }
        //         }

        //         Vector3 lPoint = centerPosition + Vector3.right * Battle.main.defender.board.tiles.GetLength(0) * 1.1f;
        //         if (plan.successives.Length > 0) RenderDebugPlanTree(plan.successives, lPoint, space, layerSpacing);
        //     }
        // }

        /// <summary>
        /// Executes a final plan and ends the turn.
        /// </summary>
        /// <param name="plan"></param>
        void ExecutePlan(Plan plan)
        {
            // for (int i = 0; i < plan.artilleryTargets.Length; i++)
            // {
            //     ArtilleryAttack attack = Effect.CreateEffect(typeof(ArtilleryAttack)) as ArtilleryAttack;
            //     attack.target = plan.artilleryTargets[i];
            //     attack.targetedPlayer = Battle.main.defender;
            //     attack.visibleTo = owner;

            //     if (!Effect.AddToQueue(attack)) Destroy(attack.gameObject);
            // }

            // for (int i = 0; i < plan.torpedoTargets.Length; i++)
            // {
            //     TorpedoAttack attack = Effect.CreateEffect(typeof(TorpedoAttack)) as TorpedoAttack;
            //     attack.target = plan.torpedoTargets[i];
            //     attack.targetedPlayer = Battle.main.defender;
            //     attack.visibleTo = owner;

            //     if (!Effect.AddToQueue(attack)) Destroy(attack.gameObject);
            // }

            // for (int i = 0; i < plan.aircraftTargets.Length; i++)
            // {
            //     AircraftRecon r = Effect.CreateEffect(typeof(AircraftRecon)) as AircraftRecon;
            //     r.target = plan.aircraftTargets[i];
            //     r.targetedPlayer = Battle.main.defender;
            //     r.visibleTo = owner;

            //     if (!Effect.AddToQueue(r)) Destroy(r.gameObject);
            // }


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