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

                float[,] gaussian_map = new float[board.tiles.GetLength(0), board.tiles.GetLength(1)];
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

                                Gameplay.Tile[] neighbours = new Gameplay.Tile[4];
                                for (int i = 0; i < 4; i++)
                                {
                                    int nx = x + (1 - i) % 2;
                                    int ny = y + (2 - i) % 2;
                                    if (nx >= 0 && nx < tiles.GetLength(0) && ny >= 0 && ny < tiles.GetLength(1)) neighbours[i] = board.tiles[nx, ny];
                                }

                                int hit_neighbours = neighbours.Count(c => c != null && c.containedShip != null);

                                if (hit_neighbours == 0)
                                {
                                    for (int i = 0; i < 4; i++)
                                    {
                                        Gameplay.Tile neighbour = neighbours[i];
                                        if (neighbour != null) tiles[neighbour.coordinates.x, neighbour.coordinates.y].importance = 2.0f;
                                    }
                                }
                                else if (hit_neighbours == 1)
                                {
                                    for (int i = 0; i < 4; i++)
                                    {
                                        Gameplay.Tile neighbour = neighbours[i];
                                        if (neighbour != null && neighbour.containedShip != null)
                                        {
                                            Gameplay.Tile opposite = neighbours[(i + 2) % 4];
                                            if (opposite != null) tiles[opposite.coordinates.x, opposite.coordinates.y].importance = 3.5f;
                                            break;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                gaussian_map.AddHeat(tile.coordinates, dist => Mathf.Pow(0.35f, dist) * UnityEngine.Random.Range(-1.0f, 0.5f));
                                permablock_map[x, y] = true;
                            }
                        }
                    }
                }

                AircraftRecon[] recon = Array.ConvertAll(Battle.main.effects.Where(x => x is AircraftRecon && x.targetedPlayer == board.owner).ToArray(), x => x as AircraftRecon);

                for (int i = 0; i < recon.Length; i++)
                {
                    int result = recon[i].result;
                    int line = recon[i].target;
                    int line_position = line % (board.tiles.GetLength(0) - 1);
                    bool line_vertical = line == line_position;

                    int start = result > 0 ? line_position + 1 : 0;
                    int end = result > 0 ? board.tiles.GetLength(line_vertical ? 0 : 1) : line_position + 1;

                    for (int a = start; a < end; a++)
                    {
                        for (int b = 0; b < board.tiles.GetLength(line_vertical ? 1 : 0); b++)
                        {
                            int x = line_vertical ? a : b;
                            int y = line_vertical ? b : a;
                            gaussian_map[x, y] *= 1.5f;
                        }
                    }
                }

                gaussian_map = gaussian_map.Normalize();
                tiles.InjectArray(gaussian_map, (ref Tile a, float b) => a.gauss = b);

                int[,] space_map = new int[board.tiles.GetLength(0), board.tiles.GetLength(1)];

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
                                    Vector2Int pos = new Vector2Int(i == 0 ? a : sequence_start + depth, i == 0 ? sequence_start + depth : a);
                                    if (sequence > space_map[pos.x, pos.y]) space_map[pos.x, pos.y] = sequence;
                                }
                                sequence = 0;
                            }
                        }
                    }
                }

                tiles.InjectArray(space_map, (ref Tile x, int y) => x.possibleShips = board.ships.Where(ship => ship.health > 0 && ship.maxHealth <= y).ToArray());

                int combined_ship_health = board.ships.Sum(ship => ship.health > 0 ? ship.maxHealth : 0);
                for (int x = 0; x < ratings.GetLength(0); x++)
                {
                    for (int y = 0; y < ratings.GetLength(1); y++)
                    {
                        Tile tile = tiles[x, y];
                        ratings[x, y] = tile.gauss + (tile.importance + 1.0f) * (tile.possibleShips.Sum(ship => ship.maxHealth) / (float)combined_ship_health);
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
            Player attacked_player = Battle.main.attacker == player ? Battle.main.defender : Battle.main.attacker;
            Map map = new Map(attacked_player.board);

            float[,] priority_map = map.ratings.Normalize();

            float advantage = player.board.ships.Sum(x => x.health) / attacked_player.board.ships.Sum(x => x.health > 0 ? x.maxHealth : 0);
            int prefered_torpedocount = Mathf.FloorToInt(Mathf.Clamp(player.arsenal.torpedoes, 0, player.arsenal.loadedTorpedoCap) * ((Mathf.Cos(advantage * Mathf.PI) + 1.0f) / 2.0f));

            int gun_targetcount = player.arsenal.guns;
            int torpedo_targetcount = player.arsenal.loadedTorpedoes >= prefered_torpedocount || player.arsenal.loadedTorpedoes == player.arsenal.torpedoes ? prefered_torpedocount : 0;
            int aircraft_targetcount = Mathf.CeilToInt(Mathf.Clamp(player.arsenal.aircraft - Battle.main.effects.Count(x => x is AircraftRecon && x.visibleTo == player), 0, int.MaxValue) * priority_map.Average());

            bool cyclone_active = Battle.main.effects.Exists(x => x is Cyclone);

            for (int ti = 0; ti < gun_targetcount; ti++)
            {
                Vector2Int target = priority_map.Max();

                ArtilleryAttack attack = Effect.CreateEffect(typeof(ArtilleryAttack)) as ArtilleryAttack;
                attack.target = attacked_player.board.tiles[target.x, target.y];

                attack.visibleTo = player;
                attack.targetedPlayer = attacked_player;

                Effect.AddToStack(attack);

                float average = priority_map.Average();
                priority_map = priority_map.AddHeat(target, dist => -Mathf.Pow(cyclone_active ? 0.6f : 0.3f, dist) * average);
            }

            for (int ti = 0; ti < torpedo_targetcount; ti++)
            {
                float best_lane_rating = Mathf.NegativeInfinity;
                TorpedoAttack.Target best_target = new TorpedoAttack.Target();

                for (int i = 0; i < 4; i++)
                {
                    for (int a = 0; a < priority_map.GetLength(i % 2); a++)
                    {
                        float rating = 0;
                        for (int b = 0; b < priority_map.GetLength((i + 1) % 2) / 2; b++)
                        {
                            int x = i % 2 == 0 ? a : (i == 1 ? b : priority_map.GetLength(0) - b - 1);
                            int y = i % 2 == 1 ? a : (i == 0 ? b : priority_map.GetLength(1) - b - 1);

                            rating += priority_map[x, y];
                        }

                        if (rating > best_lane_rating)
                        {
                            best_lane_rating = rating;

                            best_target.torpedoDropPoint = attacked_player.board.tiles[i % 2 == 0 ? a : (i == 1 ? 0 : (priority_map.GetLength(0) - 1)), i % 2 == 1 ? a : (i == 0 ? 0 : (priority_map.GetLength(1) - 1))];
                            best_target.torpedoHeading = new Vector2Int((2 - i) % 2, (1 - i) % 2);
                        }
                    }
                }

                TorpedoAttack attack = Effect.CreateEffect(typeof(TorpedoAttack)) as TorpedoAttack;
                attack.target = best_target;

                attack.visibleTo = player;
                attack.targetedPlayer = attacked_player;

                Effect.AddToStack(attack);

                float average = priority_map.Average();
                priority_map = priority_map.AddHeat(best_target.torpedoDropPoint.coordinates, dist => -Mathf.Pow(0.5f, dist) * average);
            }

            for (int ti = 0; ti < aircraft_targetcount; ti++)
            {
                float best_line_rating = Mathf.NegativeInfinity;
                int best_line = 0;
                int line_count = attacked_player.board.tiles.GetLength(0) + attacked_player.board.tiles.GetLength(1) - 2;

                for (int line = 0; line < line_count; line++)
                {
                    int line_position = line % (attacked_player.board.tiles.GetLength(0) - 1);
                    bool lineVertical = line_position == line;
                    float line_rating = 0;

                    for (int a = 0; a < 2; a++)
                    {
                        for (int b = 0; b < attacked_player.board.tiles.GetLength(lineVertical ? 1 : 0); b++)
                        {
                            int x = lineVertical ? a + line_position : b;
                            int y = lineVertical ? b : a + line_position;
                            line_rating += priority_map[x, y];
                        }
                    }

                    if (line_rating > best_line_rating)
                    {
                        best_line = line;
                        best_line_rating = line_rating;
                    }
                }

                AircraftRecon recon = Effect.CreateEffect(typeof(AircraftRecon)) as AircraftRecon;
                recon.target = best_line;

                recon.visibleTo = player;
                recon.targetedPlayer = attacked_player;

                Effect.AddToStack(recon);

                bool line_vertical = best_line < attacked_player.board.tiles.GetLength(0);
                best_line %= attacked_player.board.tiles.GetLength(0) - 1;

                for (int a = 0; a < 2; a++)
                {
                    for (int b = 0; b < attacked_player.board.tiles.GetLength(line_vertical ? 1 : 0); b++)
                    {
                        int x = line_vertical ? a + best_line : b;
                        int y = line_vertical ? b : a + best_line;
                        priority_map[x, y] = 0;
                    }
                }

            }
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
