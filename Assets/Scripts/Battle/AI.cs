using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

using Gameplay.Effects;
using Gameplay.Ships;

[Serializable]
public struct Heatmap : ICloneable
{
    public object Clone()
    {
        Heatmap result = new Heatmap();
        result.tiles = (float[,])tiles.Clone();

        return result;
    }
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
        Heatmap result = new Heatmap(map.tiles.GetLength(0), map.tiles.GetLength(1));

        for (int x = 0; x < result.tiles.GetLength(0); x++)
        {
            for (int y = 0; y < result.tiles.GetLength(1); y++)
            {
                result.tiles[x, y] = map.tiles[x, y] * mult;
            }
        }

        return result;
    }

    public static Heatmap operator +(Heatmap map1, Heatmap map2)
    {
        Heatmap result = new Heatmap(map1.tiles.GetLength(0), map1.tiles.GetLength(1));

        for (int x = 0; x < result.tiles.GetLength(0); x++)
        {
            for (int y = 0; y < result.tiles.GetLength(1); y++)
            {
                result.tiles[x, y] = map1.tiles[x, y] + map2.tiles[x, y];
            }
        }

        return result;
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
            Vector2Int coldestTile = ColdestTile;
            float lowestHeat = tiles[coldestTile.x, coldestTile.y];

            for (int x = 0; x < tiles.GetLength(0); x++)
            {
                for (int y = 0; y < tiles.GetLength(1); y++)
                {
                    float heat = tiles[x, y];
                    intermediate.tiles[x, y] = heat < float.MinValue ? 0 : heat - lowestHeat + 0.000001f;
                }
            }


            Vector2Int hottestTile = intermediate.HottestTile;
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

    public Vector2Int HottestTile
    {
        get
        {
            Vector2Int pos = Vector2Int.zero;
            float highest = Mathf.NegativeInfinity;

            for (int x = 0; x < tiles.GetLength(0); x++)
            {
                for (int y = 0; y < tiles.GetLength(1); y++)
                {
                    float heat = tiles[x, y];
                    if (heat >= highest) { pos = new Vector2Int(x, y); highest = heat; }
                }
            }

            return pos;
        }
    }

    public Vector2Int ColdestTile
    {
        get
        {
            Vector2Int pos = Vector2Int.zero;
            float lowest = Mathf.Infinity;

            for (int x = 0; x < tiles.GetLength(0); x++)
            {
                for (int y = 0; y < tiles.GetLength(1); y++)
                {
                    float heat = tiles[x, y];
                    if (heat <= lowest && heat > float.MinValue) { pos = new Vector2Int(x, y); lowest = heat; }
                }
            }

            return pos;
        }
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
    public class AI : UnityEngine.Object
    {
        public static Player processedPlayer;
        public static bool cycloneActive;
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
        public const float reconModifier = 0.2f;
        public const float reconChangeRate = 0.5f;
        public const float missHeat = -1.0f;
        public const float hitHeat = 5.0f;
        public const float destructionHeat = 1.5f;
        public const float heatDropoff = 0.55f;
        public const float hitConfidenceThreshold = 4.2f;
        public static void PlayTurnForPlayer(Player player)
        {
            processedPlayer = player;
            cycloneActive = Effect.GetEffectsInQueue(x => true, typeof(Cyclone), 1).Length == 1;

            if (player.board.ships == null)
            {
                player.board.SpawnShips();
                PlaceShips();

                if (player.aiEnabled)
                {
                    for (int i = 0; i < player.board.ships.Length; i++)
                    {
                        player.board.ships[i].gameObject.SetActive(false);
                    }
                }
            }
            else
            {
                Attack();
            }

            if (player.aiEnabled) Battle.main.NextTurn();
        }

        struct Maptile : ICloneable
        {
            public object Clone()
            {
                Maptile result;
                result.parent = parent;
                result.coordinates = coordinates;

                result.definitelyContainsShip = definitelyContainsShip;
                result.definitelyDoesntContainShip = definitelyDoesntContainShip;

                result.hit = hit;

                result.considered = considered;
                result.containedShipID = ContainedShipID;
                result.space = space;

                return result;
            }
            public Maptile(Datamap parent, Vector2Int coordinates)
            {
                this.parent = parent;
                this.coordinates = coordinates;

                hit = false;
                definitelyContainsShip = false;
                definitelyDoesntContainShip = false;

                considered = false;

                containedShipID = -1;
                space = Vector2Int.one * -1;
            }
            Vector2Int coordinates;
            Datamap parent;
            /// <summary>
            /// Whether this tile was considered by the spacing calculator.
            /// </summary>
            public bool considered;
            /// <summary>
            /// Whether this tile has been damaged/hit.
            /// </summary>
            public bool hit;
            /// <summary>
            /// Whether this tile definitely DOES contain a ship.
            /// </summary>
            public bool definitelyContainsShip;
            /// <summary>
            /// Whether this tile definitely DOES NOT contain a ship.
            /// </summary>
            public bool definitelyDoesntContainShip;
            /// <summary>
            /// The ship in this tile.
            /// </summary>
            int containedShipID;

            public int ContainedShipID
            {
                get
                {
                    if (definitelyContainsShip)
                    {
                        if (containedShipID < 0)
                        {
                            int longest = 0;

                            Ship[] ships = Battle.main.defender.board.ships;

                            for (int i = 0; i < ships.Length; i++)
                            {
                                if (parent.health[i] > 0 && ships[i].maxHealth <= MaxSpace && ships[i].maxHealth > longest)
                                {
                                    containedShipID = i;
                                    longest = ships[i].maxHealth;
                                }
                            }
                        }
                    }
                    else containedShipID = -1;

                    return containedShipID;
                }
                set
                {
                    containedShipID = value;
                }
            }
            /// <summary>
            /// The remaining health of the contained ship.
            /// </summary>
            /// <returns></returns>
            public int ContainedShipHealth
            {
                get
                {
                    int id = ContainedShipID;
                    return id >= 0 ? parent.health[id] : -1;
                }
            }

            public Vector2Int[] NeighbouringHits
            {
                get
                {
                    List<Vector2Int> result = new List<Vector2Int>();
                    for (int i = 0; i < 4; i++)
                    {
                        Vector2Int potentialNeighbour = coordinates + new Vector2Int(i < 2 ? (i == 0 ? 1 : -1) : 0, i >= 2 ? (i == 2 ? 1 : -1) : 0);
                        if (potentialNeighbour.x >= 0 && potentialNeighbour.y >= 0 && potentialNeighbour.x < parent.tiledata.GetLength(0) && potentialNeighbour.y < parent.tiledata.GetLength(1) && parent.tiledata[potentialNeighbour.x, potentialNeighbour.y].ContainedShipID >= 0)
                        {
                            result.Add(potentialNeighbour);
                        }
                    }

                    return result.ToArray();
                }
            }

            /// <summary>
            /// The longest ship length this tile can contain horizontally and vertically.
            /// </summary>
            public Vector2Int space;
            /// <summary>
            /// The longest ship length this tile can contain.
            /// </summary>
            /// <returns></returns>
            public int MaxSpace
            {
                get
                {
                    return definitelyDoesntContainShip ? 0 : Mathf.Max(space.x, space.y);
                }
            }

            /// <summary>
            /// Whether this tile is not worth shooting at.
            /// </summary>
            /// <returns></returns>
            public bool IsBlack
            {
                get
                {
                    return parent.smallestShipLength > MaxSpace || hit;
                }
            }
        }

        /// <summary>
        /// Provides information about health of enemy ships, tile predictions and how long of a ship each tile can contain.
        /// </summary>
        struct Datamap : ICloneable
        {
            public Datamap(Board board)
            {
                //Record what we know about the health of enemy ships
                health = Array.ConvertAll(board.ships, x => x.health == 0 ? 0 : x.maxHealth);

                //Force the tile space data to update
                spaceDataToDate = false;

                //Convert the board to a 2D struct array
                tiledata = new Maptile[board.tiles.GetLength(0), board.tiles.GetLength(1)];


                for (int x = 0; x < tiledata.GetLength(0); x++)
                {
                    for (int y = 0; y < tiledata.GetLength(1); y++)
                    {
                        Tile actual = board.tiles[x, y];

                        Maptile tile = new Maptile(this, new Vector2Int(x, y));

                        tile.hit = actual.hit;

                        if (actual.containedShip != null && actual.hit)
                        {
                            tile.definitelyContainsShip = true;
                            if (actual.containedShip.health == 0) tile.ContainedShipID = actual.containedShip.index;
                        }

                        tile.space = new Vector2Int(tiledata.GetLength(0), tiledata.GetLength(1));

                        tiledata[x, y] = tile;
                    }
                }

                //Predict the damage caused to intact ships
                for (int x = 0; x < Tiledata.GetLength(0); x++)
                {
                    for (int y = 0; y < Tiledata.GetLength(1); y++)
                    {
                        Maptile tile = Tiledata[x, y];
                        if (tile.definitelyContainsShip)
                        {
                            health[tile.ContainedShipID]--;
                        }
                    }
                }
            }
            public object Clone()
            {
                Datamap result;
                result.health = (int[])health.Clone();
                result.tiledata = (Maptile[,])tiledata.Clone();
                result.spaceDataToDate = spaceDataToDate;

                return result;
            }
            /// <summary>
            /// Expected remaining health of enemy ships.
            /// </summary>
            public int[] health;
            /// <summary>
            /// Information about all the tiles on the board.
            /// </summary>
            public Maptile[,] tiledata;
            /// <summary>
            /// The length of the smallest ship left intact.
            /// </summary>
            /// <returns></returns>
            public int smallestShipLength
            {
                get
                {
                    int result = 200;
                    for (int i = 0; i < health.Length; i++)
                    {
                        int shipLength = Battle.main.defender.board.ships[i].maxHealth;
                        if (health[i] > 0 && shipLength < result) result = shipLength;
                    }


                    return result < 200 ? result : 0;
                }
            }

            public bool spaceDataToDate;
            /// <summary>
            /// Gets the tilemap and ensures all space availability information is up to date.
            /// </summary>
            /// <returns></returns>
            public Maptile[,] Tiledata
            {
                get
                {
                    //If the space data is not updated update it
                    if (!spaceDataToDate)
                    {
                        //Records the IDs of tile lanes that have to be recalculated
                        List<int> lanesToUpdate = new List<int>();

                        //Calculate the blocking tiles and what lanes have to be updated
                        for (int x = 0; x < tiledata.GetLength(0); x++)
                        {
                            for (int y = 0; y < tiledata.GetLength(1); y++)
                            {
                                Maptile tile = tiledata[x, y];

                                if (tile.hit && !tile.considered)
                                {
                                    tile.considered = true;

                                    tile.definitelyDoesntContainShip = !tile.definitelyContainsShip;

                                    if (!lanesToUpdate.Contains(x)) lanesToUpdate.Add(x);
                                    int horizontal = tiledata.GetLength(0) + y;
                                    if (!lanesToUpdate.Contains(horizontal)) lanesToUpdate.Add(horizontal);

                                    //Block all diagonal neighbouring tiles
                                    if (tile.definitelyContainsShip)
                                    {
                                        for (int i = 0; i < 4; i++)
                                        {
                                            Vector2Int pos = new Vector2Int(x + (i < 2 ? 1 : -1), y + (i % 2 == 0 ? 1 : -1));
                                            if (pos.x >= 0 && pos.y >= 0 && pos.x < tiledata.GetLength(0) && pos.y < tiledata.GetLength(1))
                                            {
                                                tiledata[pos.x, pos.y].considered = true;

                                                tiledata[pos.x, pos.y].definitelyDoesntContainShip = true;

                                                if (!lanesToUpdate.Contains(pos.x)) lanesToUpdate.Add(pos.x);
                                                horizontal = tiledata.GetLength(0) + pos.y;
                                                if (!lanesToUpdate.Contains(horizontal)) lanesToUpdate.Add(horizontal);
                                            }
                                        }
                                    }
                                }

                                tiledata[x, y] = tile;
                            }
                        }

                        //Update the lanes
                        foreach (int lane in lanesToUpdate)
                        {
                            int coordinate = lane % tiledata.GetLength(0);
                            bool horizontal = coordinate < lane;
                            int targetDepth = tiledata.GetLength(horizontal ? 0 : 1);

                            int space = 0;
                            List<Vector2Int> consecutiveTiles = new List<Vector2Int>();

                            //Go along the lane
                            for (int depth = 0; depth < targetDepth; depth++)
                            {
                                Vector2Int tile = new Vector2Int(horizontal ? depth : coordinate, horizontal ? coordinate : depth);
                                bool blocking = tiledata[tile.x, tile.y].definitelyDoesntContainShip;

                                //If this tile is free, add its provided space to the consecutive space
                                if (!blocking)
                                {
                                    space++;
                                    Vector2Int startingSpace = tiledata[tile.x, tile.y].space;
                                    consecutiveTiles.Add(tile);
                                }

                                //If this tile is blocked or we reached the end of the lane, assign the consecutive space to the tiles that provided it and are not locked
                                if (blocking || depth == targetDepth - 1)
                                {
                                    foreach (Vector2Int u in consecutiveTiles)
                                    {
                                        if (horizontal) tiledata[u.x, u.y].space.x = space; else tiledata[u.x, u.y].space.y = space;
                                    }

                                    consecutiveTiles.Clear();
                                    space = 0;
                                }
                            }
                        }

                        spaceDataToDate = true;
                    }
                    return tiledata;
                }
            }



            /// <summary>
            /// Logs shots on a tile.
            /// </summary>
            /// <param name="coordinates">Position of the tile.</param>
            /// <param name="shipHit">False if miss, true if hit - contained ship is automatically predicted.</param>
            public void LogHit(Vector2Int coordinates, bool shipHit)
            {
                if (shipHit)
                {
                    int maxAvailableSpace = Tiledata[coordinates.x, coordinates.y].MaxSpace;
                    int longest = 0;
                    int predictedShipID = 0;

                    Ship[] ships = Battle.main.defender.board.ships;

                    for (int i = 0; i < ships.Length; i++)
                    {
                        if (health[i] > 0 && ships[i].maxHealth <= maxAvailableSpace && ships[i].maxHealth > longest)
                        {
                            predictedShipID = i;
                            longest = ships[i].maxHealth;
                        }
                    }

                    LogHit(coordinates, predictedShipID);
                }
                else
                {
                    spaceDataToDate = false;
                    tiledata[coordinates.x, coordinates.y].hit = true;
                }
            }
            /// <summary>
            /// Logs a hit on tile.
            /// </summary>
            /// <param name="pos">Position of the tile.</param>
            /// <param name="knownContainedShipID">The ship that was damaged.</param>
            public void LogHit(Vector2Int pos, int knownContainedShipID)
            {
                spaceDataToDate = false;
                tiledata[pos.x, pos.y].hit = true;
                tiledata[pos.x, pos.y].ContainedShipID = knownContainedShipID;
                health[knownContainedShipID]--;
            }
        }

        struct Situation : ICloneable
        {
            public Situation(Board board)
            {
                //Create a new datamap from the enemy's board
                datamap = new Datamap(board);

                //Initialize the heatmaps
                heatmap_statistical = new Heatmap(board.tiles.GetLength(0), board.tiles.GetLength(1));
                heatmap_transitional = new Heatmap(board.tiles.GetLength(0), board.tiles.GetLength(1));

                targetmap = new Heatmap(board.tiles.GetLength(0), board.tiles.GetLength(1));


                //Assign gun count and information about torpedoes and aircraft
                AmmoRegistry ammo = Battle.main.attacker.arsenal;
                totalArtilleryCount = ammo.guns;

                Effect torpedoCooldown = Effect.GetEffectsInQueue(x => x.visibleTo == ammo.targetedPlayer, typeof(TorpedoCooldown), 1).FirstOrDefault();
                this.torpedoCooldown = torpedoCooldown ? torpedoCooldown.duration : 0;
                Effect torpedoReload = Effect.GetEffectsInQueue(x => x.visibleTo == ammo.targetedPlayer, typeof(TorpedoReload), 1).FirstOrDefault();
                this.torpedoReload = torpedoReload ? torpedoReload.duration : 0;

                this.totalTorpedoCount = ammo.torpedoes;
                this.loadedTorpedoCount = ammo.loadedTorpedoes;
            }
            public object Clone()
            {
                Situation result;
                result.datamap = (Datamap)datamap.Clone();
                result.heatmap_statistical = new Heatmap(heatmap_statistical.tiles.GetLength(0), heatmap_statistical.tiles.GetLength(1));
                result.heatmap_transitional = new Heatmap(heatmap_statistical.tiles.GetLength(0), heatmap_statistical.tiles.GetLength(1));
                result.targetmap = new Heatmap(heatmap_statistical.tiles.GetLength(0), heatmap_statistical.tiles.GetLength(1));

                result.totalArtilleryCount = totalArtilleryCount;

                result.torpedoReload = torpedoReload;
                result.torpedoCooldown = torpedoCooldown;
                result.loadedTorpedoCount = loadedTorpedoCount;
                result.totalTorpedoCount = totalTorpedoCount;

                return result;
            }

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
                        if (tile.hit) heatmap_statistical.Heat(new Vector2Int(x, y), tile.ContainedShipID >= 0 ? (tile.ContainedShipHealth <= 0 ? destructionHeat : hitHeat) : missHeat, heatDropoff);
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
                targetmap = (heatmap_statistical.normalized + heatmap_transitional + AI.processedPlayer.heatmap_recon);

                for (int x = 0; x < targetmap.tiles.GetLength(0); x++)
                {
                    for (int y = 0; y < targetmap.tiles.GetLength(1); y++)
                    {
                        if (datamap.Tiledata[x, y].IsBlack) targetmap.tiles[x, y] = Mathf.NegativeInfinity;
                    }
                }

                targetmap = targetmap.normalized;
            }

            public int totalArtilleryCount;
            public int totalTorpedoCount;

            public int loadedTorpedoCount;
            public int torpedoReload;
            public int torpedoCooldown;


            public Plan[] GetStrategy(int[] sequence)
            {
                //Load the first number of the sequence and make that many plans
                Plan[] results = new Plan[sequence[0] * 2];

                //Discard the first number
                sequence = sequence.Skip(1).ToArray();

                //Create the plans and provide each with the remaining sequence
                for (int i = 0; i < results.Length; i++)
                {
                    int torpedoCount = Mathf.CeilToInt(Mathf.Clamp01((i - results.Length / 2 + 1) / (float)results.Length) * loadedTorpedoCount);
                    results[i] = new Plan(this, ref heatmap_transitional, torpedoCount, sequence);
                }
                return results;
            }
        }

        struct Plan
        {
            public float rating
            {
                get
                {
                    float result = situation.heatmap_statistical.normalized.averageHeat;
                    for (int i = 0; i < successives.Length; i++)
                    {
                        result += successives[i].rating;
                    }

                    return result;
                }
            }
            public Situation situation;
            /// <summary>
            /// Creates a new plan for a source situation.
            /// </summary>
            /// <param name="state">Situation to be planned on.</param>
            /// <param name="heatmap_transitional">References the transmap of the previous layer.</param>
            /// <param name="sequence">The branching of plans considered ahead.</param>
            public Plan(Situation state, ref Heatmap heatmap_transitional, int torpedoCount, int[] sequence)
            {
                this = new Plan();

                //Copy the original situation
                situation = (Situation)state.Clone();
                situation.heatmap_transitional = heatmap_transitional;

                //Initialize targeting maps
                situation.ConstructTargetmap();

                //Choose the gun targets
                List<Tile> artilleryTargets = new List<Tile>();
                for (int i = 0; i < situation.totalArtilleryCount; i++)
                {
                    artilleryTargets.Add(GetNextArtilleryTarget());
                }
                artillery = artilleryTargets.ToArray();

                torpedoes = new TorpedoAttack.Target[0];
                aircraft = new int[0];

                //Choose the torpedo targets
                List<TorpedoAttack.Target> torpedoTargets = new List<TorpedoAttack.Target>();
                for (int i = 0; i < torpedoCount && i < situation.loadedTorpedoCount; i++)
                {
                    torpedoTargets.Add(GetNextTorpedoTarget());
                }
                torpedoes = torpedoTargets.ToArray();

                heatmap_transitional = (Heatmap)heatmap_transitional.Clone();

                //If a sequence is provided branch this plan further with it
                if (sequence.Length > 0) successives = situation.GetStrategy(sequence); else successives = new Plan[0];
            }

            public Tile GetNextArtilleryTarget()
            {
                Vector2Int bestTarget = situation.targetmap.HottestTile;
                situation.heatmap_transitional.Heat(bestTarget, cycloneActive ? -1.0f : -0.2f, 0.3f);
                situation.datamap.tiledata[bestTarget.x, bestTarget.y].hit = true;
                situation.datamap.tiledata[bestTarget.x, bestTarget.y].definitelyContainsShip = situation.targetmap.tiles[bestTarget.x, bestTarget.y] / situation.targetmap.averageHeat > hitConfidenceThreshold;

                situation.datamap.spaceDataToDate = false;
                situation.ConstructTargetmap();

                return Battle.main.defender.board.tiles[bestTarget.x, bestTarget.y];
            }

            public TorpedoAttack.Target GetNextTorpedoTarget()
            {
                //Get the ID of the hottest targeting lane
                int bestLane = situation.targetmap.GetExtremeLanes(1)[0];

                //Get the x/y position of the lane
                int position = bestLane % situation.targetmap.tiles.GetLength(0);

                //Determine whether the lane is horizontal
                bool horizontal = position < bestLane;
                int maxDepth = situation.targetmap.tiles.GetLength(horizontal ? 0 : 1);

                bool negativeDrop = false;
                for (int depth = 0; depth < maxDepth; depth++)
                {
                    Vector2Int tile = horizontal ? new Vector2Int(depth, position) : new Vector2Int(position, depth);


                    bool chosenForTargeting = situation.targetmap.tiles[tile.x, tile.y] / situation.targetmap.averageHeat > hitConfidenceThreshold || situation.datamap.Tiledata[tile.x, tile.y].definitelyContainsShip;
                    situation.datamap.tiledata[tile.x, tile.y].definitelyContainsShip = chosenForTargeting;

                    if (chosenForTargeting)
                    {
                        negativeDrop = depth < maxDepth / 2;
                        for (int i = negativeDrop ? maxDepth - 1 : 0; negativeDrop ? i >= depth : i <= depth; i = i + (negativeDrop ? -1 : 1))
                        {
                            Vector2Int x = new Vector2Int(horizontal ? i : position, horizontal ? position : i);
                            situation.datamap.tiledata[x.x, x.y].hit = true;
                            situation.heatmap_transitional.Heat(x, -0.2f, 0.3f);
                        }
                        break;
                    }
                }

                situation.ConstructTargetmap();

                int dropDepth = negativeDrop ? maxDepth - 1 : 0;
                return new TorpedoAttack.Target(Battle.main.defender.board.tiles[horizontal ? dropDepth : position, horizontal ? position : dropDepth], new Vector2Int(horizontal ? (negativeDrop ? -1 : 1) : 0, horizontal ? 0 : (negativeDrop ? -1 : 1)));
            }

            public void TargetAircraft(int count)
            {
                aircraft = situation.targetmap.GetExtremeGridLines(count);
            }

            public Plan[] successives;
            public Tile[] artillery;
            public TorpedoAttack.Target[] torpedoes;
            public int[] aircraft;
        }

        static void Attack()
        {
            //In order to perform a attack we need to know the targets for artillery and torpedoes

            //Convert the current game state into a struct
            Situation situation = new Situation(Battle.main.defender.board);

            //Create plans for striking the most likely enemy ship positions and rate them based on their consequences
            Plan[] plans = situation.GetStrategy(new int[] { 3, 1 });

            Destroy(debugObjectParent);

            debugObjectParent = new GameObject("Debug Object Parent");
            //RenderDebugPlanTree(plans, Vector3.up * 40, 280f, 20f);

            //Execute the plan with the highest rating
            Plan best = plans.OrderByDescending(x => x.rating).First();
            if (processedPlayer.arsenal.aircraft > 0 && Effect.GetEffectsInQueue(x => x.targetedPlayer != processedPlayer, typeof(AircraftRecon), int.MaxValue).Length == 0) best.TargetAircraft(processedPlayer.arsenal.aircraft);
            ExecutePlan(best);
        }

        static GameObject debugObjectParent;
        static void RenderDebugPlanTree(Plan[] plans, Vector3 linkPoint, float space, float layerSpacing)
        {
            float spacing = plans.Length > 1 ? space / (plans.Length - 1) : 0;
            Vector3 startingPosition = linkPoint + new Vector3(layerSpacing, 0, space / 2.0f);

            space /= (float)plans.Length * 1.2f;

            for (int i = 0; i < plans.Length; i++)
            {
                Plan plan = plans[i];

                Vector3 centerPosition = startingPosition + Vector3.back * i * spacing;
                Vector3 boardCornerPosition = centerPosition - new Vector3(Battle.main.defender.board.tiles.GetLength(0), 0, Battle.main.defender.board.tiles.GetLength(1));
                Situation renderedSituation = plan.situation;
                Heatmap targetMap = renderedSituation.targetmap;

                Debug.DrawLine(linkPoint, boardCornerPosition, Color.blue, Mathf.Infinity, true);

                for (int x = 0; x < targetMap.tiles.GetLength(0); x++)
                {
                    for (int y = 0; y < targetMap.tiles.GetLength(1); y++)
                    {
                        Vector3 lineBeginningPosition = boardCornerPosition + new Vector3(x, 0, y);
                        float cubeHeight = targetMap.tiles[x, y] * 0.5f + 0.002f;
                        // Debug.DrawLine(lineBeginningPosition, lineBeginningPosition + Vector3.up * cubeHeight, renderedSituation.map[x, y, 3] != null ? Color.red : Color.black, Mathf.Infinity, true);

                        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

                        cube.transform.SetParent(debugObjectParent.transform);

                        cube.transform.position = lineBeginningPosition + Vector3.up * cubeHeight / 2.0f;
                        cube.transform.localScale = new Vector3(1, cubeHeight, 1);

                        Renderer r = cube.GetComponent<Renderer>();

                        MaterialPropertyBlock block = new MaterialPropertyBlock();
                        block.SetColor("_Color", renderedSituation.datamap.tiledata[x, y].definitelyContainsShip ? Color.red : (plan.artillery.Any(t => t.coordinates == new Vector2Int(x, y)) ? Color.blue : (renderedSituation.datamap.tiledata[x, y].hit) ? Color.black : Color.white));

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
        static void ExecutePlan(Plan plan)
        {
            for (int i = 0; i < plan.artillery.Length; i++)
            {
                ArtilleryAttack attack = Effect.CreateEffect(typeof(ArtilleryAttack)) as ArtilleryAttack;
                attack.target = plan.artillery[i];
                attack.targetedPlayer = Battle.main.defender;
                attack.visibleTo = processedPlayer;

                if (!Effect.AddToQueue(attack)) Destroy(attack.gameObject);
            }

            for (int i = 0; i < plan.torpedoes.Length; i++)
            {
                TorpedoAttack attack = Effect.CreateEffect(typeof(TorpedoAttack)) as TorpedoAttack;
                attack.target = plan.torpedoes[i];
                attack.targetedPlayer = Battle.main.defender;
                attack.visibleTo = processedPlayer;

                if (!Effect.AddToQueue(attack)) Destroy(attack.gameObject);
            }

            for (int i = 0; i < plan.aircraft.Length; i++)
            {
                AircraftRecon r = Effect.CreateEffect(typeof(AircraftRecon)) as AircraftRecon;
                r.target = plan.aircraft[i];
                r.targetedPlayer = Battle.main.defender;
                r.visibleTo = processedPlayer;

                if (!Effect.AddToQueue(r)) Destroy(r.gameObject);
            }
        }

        static void PlaceShips()
        {
            //Remove any placed owner.board.ships from the board
            for (int i = 0; i < processedPlayer.board.ships.Length; i++)
            {
                Ship ship = processedPlayer.board.ships[i];
                ship.Pickup();
                ship.Place(null);
            }

            //Each ship gets a heatmap of best placement spots
            Heatmap[] shipLocationHeatmaps = new Heatmap[processedPlayer.board.ships.Length];
            for (int i = 0; i < processedPlayer.board.ships.Length; i++)
            {
                shipLocationHeatmaps[i] = new Heatmap(processedPlayer.board.tiles.GetLength(0), processedPlayer.board.tiles.GetLength(1));
            }

            //Determine heatmaps by individual tactical choices
            //1.Tactic - Dispersion
            float dispersionValue = UnityEngine.Random.Range(0.000f, 1.000f);
            for (int i = 0; i < processedPlayer.board.ships.Length; i++)
            {
                shipLocationHeatmaps[i].Heat(new Vector2Int(UnityEngine.Random.Range(0, processedPlayer.board.tiles.GetLength(0)), UnityEngine.Random.Range(0, processedPlayer.board.tiles.GetLength(1))), 8.0f * dispersionValue, 0.15f);
            }

            //2.Tactic - Camouflage
            float concealmentAccuracyValue = 1.0f - (float)Math.Pow(UnityEngine.Random.Range(0.000f, 1.000f), 4);
            List<int> cruiserIDs = new List<int>();
            for (int i = 0; i < processedPlayer.board.ships.Length; i++)
            {
                if (processedPlayer.board.ships[i] is Cruiser)
                {
                    cruiserIDs.Add(i);
                }
            }

            int[] shipsToConcealIDs = new int[cruiserIDs.Count];
            for (int s = 0; s < shipsToConcealIDs.Length; s++)
            {
                int[] ranges = new int[processedPlayer.board.ships.Length];
                for (int i = 0; i < processedPlayer.board.ships.Length; i++)
                {
                    int lastRange = i > 0 ? ranges[i - 1] : 0;
                    ranges[i] = lastRange + processedPlayer.board.ships[i].concealmentAIValue;
                }

                int chosen = UnityEngine.Random.Range(0, ranges[ranges.Length - 1] + 1);
                for (int i = 0; i < processedPlayer.board.ships.Length; i++)
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

            for (int i = 0; i < processedPlayer.board.ships.Length; i++)
            {
                if (!sortedShipIDs.Contains(i))
                {
                    sortedShipIDs.Add(i);
                }
            }



            //Place ships in whatever the best available spot left is
            foreach (int shipID in sortedShipIDs)
            {
                Ship ship = processedPlayer.board.ships[shipID];
                ship.Pickup();

                float[,] heatmap = shipLocationHeatmaps[shipID].tiles;

                for (int x = 0; x < ship.maxHealth; x++)
                {
                    Tile bestChoice = processedPlayer.board.placementInfo.selectableTiles[0];

                    foreach (Tile tile in processedPlayer.board.placementInfo.selectableTiles)
                    {
                        if ((heatmap[tile.coordinates.x, tile.coordinates.y] > heatmap[bestChoice.coordinates.x, bestChoice.coordinates.y]) || (heatmap[tile.coordinates.x, tile.coordinates.y] == heatmap[bestChoice.coordinates.x, bestChoice.coordinates.y] && UnityEngine.Random.Range(0, 2) == 0))
                        {
                            bestChoice = tile;
                        }
                    }

                    processedPlayer.board.SelectTileForPlacement(bestChoice);
                }

                if (ship is Cruiser)
                {
                    (ship as Cruiser).ConcealAlreadyPlacedShipsInConcealmentArea();
                }

                if (processedPlayer.board.placementInfo.selectableTiles.Count == 0)
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