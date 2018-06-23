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
            public Map(int width, int height)
            {
                tiles = new Tile[width, height];
                ratings = new float[width, height];
                ratingsToDate = false;
            }
            public Tile[,] tiles;
            float[,] ratings;
            public float[,] Ratings
            {
                get
                {
                    if (!ratingsToDate)
                    {

                    }
                    return ratings;
                }
            }
            bool ratingsToDate;
        }

        struct Tile
        {
            public float gauss;
            public float importance;
            public int space;
            public int[] possibleShips;
        }

        public static void Process(Player player)
        {
            if (Battle.main.fighting) FightFor(player); else PlaceFleetFor(player);
        }

        static void FightFor(Player player)
        {



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
                   1.0f,
                   (amount, dist) => Mathf.Pow(0.85f, dist) * amount);
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
                    cruiser_map = cruiser_map.AddHeat(concealee_map.Max(), concealee_map.Average(), (amount, dist) => Mathf.Pow(0.85f, dist) * amount);
                    cruiser_map = cruiser_map.AddHeat(concealee_map.Max(), -concealee_map.Average(), (amount, dist) => dist < 2 ? amount : 0);

                    ship.placementPriority = int.MaxValue - concealee_order * 2;
                    concealee.placementPriority = int.MaxValue - 1 - concealee_order * 2;
                    break;
                }
            }

            maps[ship.index] = cruiser_map;
        }
    }


}
