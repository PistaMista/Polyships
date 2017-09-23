using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum ShipType
{
    BATTLESHIP,
    CRUISER,
    DESTROYER,
    CARRIER,
    PATROLBOAT
}

public class Ship : MonoBehaviour
{
    [Serializable]
    public struct ShipData
    {
        public int index;
        public bool ownedByAttacker;
        public int[,] tiles;
        public int health;
        public ShipType type;
        public int[] metadata;

        public static implicit operator ShipData(Ship ship)
        {
            ShipData result = new ShipData();
            result.index = ship.index;
            result.ownedByAttacker = ship.owner == Battle.main.attacker;
            result.tiles = new int[ship.tiles.Length, 2];
            for (int i = 0; i < ship.tiles.Length; i++)
            {
                result.tiles[i, 0] = (int)ship.tiles[i].coordinates.x;
                result.tiles[i, 1] = (int)ship.tiles[i].coordinates.y;
            }

            result.health = ship.health;
            result.type = ship.type;
            result.metadata = ship.GetMetadata();
            return result;
        }
    }

    public int index;
    public Player owner;
    public Tile[] tiles;
    public int health;
    public ShipType type;

    public virtual void Initialize(ShipData data)
    {
        index = data.index;
        //owner - REF
        //tiles - REF
        health = data.health;
        type = data.type;
    }

    public virtual void AssignReferences(ShipData data)
    {
        owner = data.ownedByAttacker ? Battle.main.attacker : Battle.main.attacked;
        tiles = new Tile[data.tiles.GetLength(0)];
        for (int i = 0; i < data.tiles.GetLength(0); i++)
        {
            tiles[i] = owner.board.tiles[data.tiles[i, 0], data.tiles[i, 1]];
        }
    }

    public virtual int[] GetMetadata()
    {
        return new int[3];
    }
}
