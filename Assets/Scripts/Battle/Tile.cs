using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public struct TileData
    {
        public int[] coordinates;
        public static implicit operator TileData(Tile tile)
        {
            TileData result = new TileData();
            result.coordinates = new int[2];
            result.coordinates[0] = (int)tile.coordinates.x;
            result.coordinates[1] = (int)tile.coordinates.y;
            return result;
        }
    }
    public Vector2 coordinates;

}
