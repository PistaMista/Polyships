using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

using Gameplay.Effects;
using Gameplay.Ships;



namespace Gameplay
{
    public class AI : UnityEngine.Object
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
            public void AddGauss(Vector2Int position, float amount, float persistence)
            {
                ratingsToDate = false;
                float[] coefficients = new float[Mathf.Max(position.x, position.y, tiles.GetLength(0) - position.x, tiles.GetLength(1) - position.y)];
                for (int i = 0; i < coefficients.Length; i++)
                {
                    coefficients[i] = Mathf.Pow(persistence, i);
                }

                for (int x = 0; x < tiles.GetLength(0); x++)
                {
                    for (int y = 0; y < tiles.GetLength(1); y++)
                    {
                        int distance = Mathf.Abs(x - position.x) + Mathf.Abs(y - position.y);
                        tiles[x, y].gauss += amount * coefficients[distance];
                    }
                }
            }
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

            //Remove any placed owner.board.ships from the board
            for (int i = 0; i < player.board.ships.Length; i++)
            {
                Ship ship = player.board.ships[i];
                ship.Pickup();
                ship.Place(null);
            }

            //Each ship gets a heatmap of best placement spots
            Map[] shipLocationHeatmaps = new Map[player.board.ships.Length];
            for (int i = 0; i < player.board.ships.Length; i++)
            {
                shipLocationHeatmaps[i] = new Map(player.board.tiles.GetLength(0), player.board.tiles.GetLength(1));
            }

            //Determine heatmaps by individual tactical choices
            //1.Tactic - Dispersion
            float dispersionValue = UnityEngine.Random.Range(0.000f, 1.000f);
            for (int i = 0; i < player.board.ships.Length; i++)
            {
                shipLocationHeatmaps[i].AddGauss(new Vector2Int(UnityEngine.Random.Range(0, player.board.tiles.GetLength(0)), UnityEngine.Random.Range(0, player.board.tiles.GetLength(1))), 8.0f * dispersionValue, 0.15f);
            }

            //2.Tactic - Camouflage
            float concealmentAccuracyValue = 1.0f - (float)Math.Pow(UnityEngine.Random.Range(0.000f, 1.000f), 4);
            List<int> cruiserIDs = new List<int>();
            for (int i = 0; i < player.board.ships.Length; i++)
            {
                if (player.board.ships[i] is Cruiser)
                {
                    cruiserIDs.Add(i);
                }
            }

            int[] shipsToConcealIDs = new int[cruiserIDs.Count];
            for (int s = 0; s < shipsToConcealIDs.Length; s++)
            {
                int[] ranges = new int[player.board.ships.Length];
                for (int i = 0; i < player.board.ships.Length; i++)
                {
                    int lastRange = i > 0 ? ranges[i - 1] : 0;
                    ranges[i] = lastRange + player.board.ships[i].concealmentAIValue;
                }

                int chosen = UnityEngine.Random.Range(0, ranges[ranges.Length - 1] + 1);
                for (int i = 0; i < player.board.ships.Length; i++)
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

                if (player.aiEnabled)
                {
                    for (int i = 0; i < player.board.ships.Length; i++)
                    {
                        player.board.ships[i].gameObject.SetActive(false);
                    }
                }
            }
        }
    }
}