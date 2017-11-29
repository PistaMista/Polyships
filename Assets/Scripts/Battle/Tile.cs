using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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
            result.ownedByAttacker = Battle.main.attacker == tile.owner;
            result.coordinates = new int[2];
            result.coordinates[0] = (int)tile.coordinates.x;
            result.coordinates[1] = (int)tile.coordinates.y;
            return result;
        }
    }
    public Player owner;
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
        owner = data.ownedByAttacker ? Battle.main.attacker : Battle.main.defender;
        if (data.containedShip >= 0)
        {

        }
    }
}
