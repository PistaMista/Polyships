using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Gameplay
{
    public class Tile : MonoBehaviour
    {
        [Serializable]
        public struct TileData
        {
            public bool ownedByAttacker;
            public int[] coordinates;
            public bool hit;
            public int containedShip;
            public static implicit operator TileData(Tile tile)
            {
                TileData result = new TileData();
                result.ownedByAttacker = tile.parentBoard.owner == Battle.main.attacker;
                result.coordinates = new int[2];
                result.coordinates[0] = (int)tile.coordinates.x;
                result.coordinates[1] = (int)tile.coordinates.y;
                result.hit = tile.hit;
                result.containedShip = tile.containedShip ? tile.containedShip.index : -1;


                return result;
            }
        }
        public Board parentBoard;
        public Vector2Int coordinates;
        public bool hit;
        public Ship containedShip;

        public void Initialize(TileData data)
        {
            //owner - REF
            coordinates = new Vector2Int(data.coordinates[0], data.coordinates[1]);
            hit = data.hit;
        }

        public void AssignReferences(TileData data)
        {
            parentBoard = (data.ownedByAttacker ? Battle.main.attacker : Battle.main.defender).board;
            if (data.containedShip >= 0)
            {
                containedShip = parentBoard.ships[data.containedShip];
            }
        }
    }
}
