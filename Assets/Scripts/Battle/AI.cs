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
            if (player.board.ships == null) player.board.SpawnShips();

            Array.ForEach(player.board.ships, x => { x.Pickup(); x.Place(null); });

            float[][,] maps = new float[player.board.ships.Length][,];
            for (int i = 0; i < maps.Length; i++) maps[i] = new float[player.board.tiles.GetLength(0), player.board.tiles.GetLength(1)];

            int[] order = Array.ConvertAll<Ship, int>(player.board.ships.OrderByDescending<Ship, int>(x => x.placementPriority).ToArray(), x => x.index);

            for (int i = 0; i < order.Length; i++)
            {
                int index = order[i];
                Ship ship = player.board.ships[index];
                float[,] map = maps[index];

                map = map.Add(ship.GetPreferredMap());

                if (ship is Cruiser)
                {
                    float baseConcealChance = 1 / player.board.ships.Sum(x => x.concealmentAIValue);

                    for(int concealee_order = 0; concealee_order < order.Length; concealee_order++)
                    {
                        int concealee_index = order[concealee_order];
                        Ship concealee = player.board.ships[concealee_index];
                        float[,] concealee_map = maps[concealee_index];
                        
                        if (concealee != ship && UnityEngine.Random.Range(0.00f, 1.00f) < baseConcealChance * concealee.concealmentAIValue)
                        {
                                
                            ship.placementPriority = int.MaxValue - concealee_order * 2;
                            concealee.placementPriority = int.MaxValue - 1 - concealee_order * 2; 
                            break;
                        }
                    }
                }
                
                maps[index] = map;
            }

            order = Array.ConvertAll<Ship, int>(player.board.ships.OrderByDescending<Ship, int>(x => x.placementPriority).ToArray(), x => x.index);

            for (int i = 0; i < order.Length; i++)
            {
                int index = order[i];


            }
        }

        public static float[,] GetPreferredMap(this Ship ship)
        {
            return null;
        }
    }


}
