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
            }
        }

        struct Tile
        {
            public float gauss;
            public float importance;
            public int space;
            public int[] possibleShips;
        }
        public static Player processedPlayer;

    }
}