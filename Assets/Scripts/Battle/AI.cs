using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

using Gameplay.Effects;
using Gameplay.Ships;
using Heatmapping;



namespace Gameplay
{
    public static class AI
    {
        struct Map
        {
            public Map(Board board)
            {
                ratings = new float[board.tiles.GetLength(0), board.tiles.GetLength(1)];

                tiles = new Tile[board.tiles.GetLength(0), board.tiles.GetLength(1)];

                float[,] gaussian_map = tiles.ExtractArray(x => x.gauss);
                bool[,] permablock_map = new bool[tiles.GetLength(0), tiles.GetLength(1)];

                for (int x = 0; x < tiles.GetLength(0); x++)
                {
                    for (int y = 0; y < tiles.GetLength(1); y++)
                    {
                        Gameplay.Tile tile = board.tiles[x, y];
                        if (tile.hit)
                        {
                            if (tile.containedShip != null)
                            {
                                bool shipInTileDestroyed = tile.containedShip.health <= 0;
                                permablock_map[x, y] = shipInTileDestroyed;
                                for (int i = 0; i < 8; i += shipInTileDestroyed ? 1 : 2)
                                {
                                    int x_toblock = x + Math.Sign((i - 1) * (5 - i));
                                    int y_toblock = y + Math.Sign((3 - i) * (7 - i));
                                    if (x_toblock >= 0 && x_toblock < permablock_map.GetLength(0) && y_toblock >= 0 && y_toblock < permablock_map.GetLength(1)) permablock_map[x_toblock, y_toblock] = true;
                                }

                                gaussian_map.AddHeat(tile.coordinates, dist => Mathf.Pow(0.6f, dist) * 3.0f);
                            }
                            else
                            {
                                gaussian_map.AddHeat(tile.coordinates, dist => Mathf.Pow(0.35f, dist) * UnityEngine.Random.Range(-1.0f, 0.5f));
                                permablock_map[x, y] = true;
                            }
                        }
                    }
                }

                gaussian_map = gaussian_map.Normalize();
                tiles.InjectArray(gaussian_map, (ref Tile a, float b) => a.gauss = b);


                for (int i = 0; i < 2; i++)
                {
                    for (int a = 0; a < tiles.GetLength(i); a++)
                    {
                        int sequence_start = 0;
                        int sequence = 0;
                        for (int b = 0; b < tiles.GetLength(1 - i); b++)
                        {
                            int x = i == 0 ? a : b;
                            int y = i == 0 ? b : a;
                            bool permablocked = permablock_map[x, y];

                            if (!permablocked)
                            {
                                if (sequence == 0) sequence_start = b;
                                sequence++;
                            }

                            if (b == tiles.GetLength(1 - i) - 1 || permablocked)
                            {
                                for (int depth = 0; depth < sequence; depth++)
                                {
                                    //!FIX
                                    tiles[i == 0 ? a : sequence_start + depth, i == 0 ? sequence_start + depth : a].possibleShips = board.ships.Where(ship => ship.maxHealth <= sequence && ship.health > 0).ToArray();
                                }
                                sequence = 0;
                            }
                        }
                    }
                }

                int combined_ship_health = board.ships.Sum(ship => ship.health);
                for (int x = 0; x < ratings.GetLength(0); x++)
                {
                    for (int y = 0; y < ratings.GetLength(1); y++)
                    {
                        Tile tile = tiles[x, y];
                        ratings[x, y] = tile.gauss + (tile.importance + 1.0f) * (tile.possibleShips.Sum(ship => ship.health) / (float)combined_ship_health);
                    }
                }
            }
            public Tile[,] tiles;
            public float[,] ratings;
        }

        struct Tile
        {
            public float gauss;
            public float importance;
            public Ship[] possibleShips;
        }

        public static void Process(Player player)
        {
            if (Battle.main.fighting) FightFor(player); else PlaceFleetFor(player);
        }

        static void FightFor(Player player)
        {
            Map map = new Map((Battle.main.attacker == player ? Battle.main.defender : Battle.main.attacker).board);

        }

        static void PlaceFleetFor(Player player)
        {
            Board board = player.board;
            if (board.ships == null) board.SpawnShips();

            Array.ForEach(board.ships, x => { x.Pickup(); x.Place(null); });

            float[][,] maps = new float[board.ships.Length][,];
            for (int i = 0; i < maps.Length; i++) maps[i] = new float[board.tiles.GetLength(0), board.tiles.GetLength(1)];

            int[] order = Array.ConvertAll<Ship, int>(board.ships.OrderByDescending<Ship, int>(x => x.placementPriority).ToArray(), x => x.index);

            for (int i = 0; i < order.Length; i++)
            {
                int ship_index = order[i];
                Ship ship = board.ships[ship_index];

                maps[ship_index] = ship.GetPreferredMap();

                if (ship is Cruiser) (ship as Cruiser).ConsiderConcealmentOfShipsInOrder(order, ref maps);
            }

            order = Array.ConvertAll<Ship, int>(board.ships.OrderByDescending<Ship, int>(x => x.placementPriority).ToArray(), x => x.index);

            for (int i = 0; i < order.Length; i++)
            {
                int ship_index = order[i];
                Ship ship = board.ships[ship_index];
                float[,] map = maps[ship_index];

                ship.Pickup();
                for (int p = 0; p < ship.maxHealth; p++) board.SelectTileForPlacement(board.placementInfo.selectableTiles.OrderByDescending(x => map[x.coordinates.x, x.coordinates.y]).First());


                if (board.placementInfo.selectableTiles.Count == 0) { PlaceFleetFor(player); break; }
            }
        }

        public static float[,] GetPreferredMap(this Ship ship)
        {
            float[,] map = new float[ship.parentBoard.tiles.GetLength(0), ship.parentBoard.tiles.GetLength(1)];

            if (!(ship is Cruiser))
                for (int i = 0; i < 2; i++)
                {
                    map = map.AddHeat
                   (new Vector2Int(UnityEngine.Random.Range(0, map.GetLength(0)), UnityEngine.Random.Range(0, map.GetLength(1))),
                   dist => Mathf.Pow(0.85f, dist));
                }

            return map;
        }


        public static void ConsiderConcealmentOfShipsInOrder(this Cruiser ship, int[] order, ref float[][,] maps)
        {
            Board board = ship.parentBoard;
            float baseConcealChance = 1.00f / board.ships.Sum(x => x.concealmentAIValue);
            float[,] cruiser_map = maps[ship.index];

            for (int concealee_order = 0; concealee_order < order.Length; concealee_order++)
            {
                int concealee_index = order[concealee_order];
                Ship concealee = board.ships[concealee_index];
                float[,] concealee_map = maps[concealee_index];

                if (concealee != ship && UnityEngine.Random.Range(0.00f, 1.00f) < baseConcealChance * concealee.concealmentAIValue)
                {
                    Vector2Int concealee_map_max = concealee_map.Max();
                    float concealee_map_average = concealee_map.Average();

                    cruiser_map = cruiser_map.AddHeat(concealee_map_max, dist => Mathf.Pow(0.85f, dist) * concealee_map_average);
                    cruiser_map = cruiser_map.AddHeat(concealee_map_max, dist => dist < 2 ? -concealee_map_average : 0);

                    ship.placementPriority = int.MaxValue - concealee_order * 2;
                    concealee.placementPriority = int.MaxValue - 1 - concealee_order * 2;
                    break;
                }
            }

            maps[ship.index] = cruiser_map;
        }
    }


}
