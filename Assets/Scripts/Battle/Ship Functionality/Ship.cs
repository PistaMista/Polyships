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
        public bool concealed;
        public ShipType type;
        public int[] metadata;

        public static implicit operator ShipData(Ship ship)
        {
            ShipData result = new ShipData();
            result.index = ship.index;
            result.ownedByAttacker = ship.parentBoard.owner == Battle.main.attacker;
            result.tiles = new int[ship.tiles.Length, 2];
            for (int i = 0; i < ship.tiles.Length; i++)
            {
                result.tiles[i, 0] = (int)ship.tiles[i].coordinates.x;
                result.tiles[i, 1] = (int)ship.tiles[i].coordinates.y;
            }

            result.health = ship.health;
            result.concealed = ship.concealed;
            result.type = ship.type;
            result.metadata = ship.GetMetadata();
            return result;
        }
    }

    public int index;
    public Board parentBoard;
    public Tile[] tiles;
    public int health;
    public Cruiser concealedBy;
    public ShipType type;

    public virtual void Initialize(ShipData data)
    {
        index = data.index;
        //owner - REF
        //tiles - REF
        health = data.health;
        concealed = data.concealed;
        type = data.type;
    }

    public virtual void AssignReferences(ShipData data)
    {
        parentBoard = (data.ownedByAttacker ? Battle.main.attacker : Battle.main.defender).board;
        tiles = new Tile[data.tiles.GetLength(0)];
        for (int i = 0; i < data.tiles.GetLength(0); i++)
        {
            tiles[i] = parentBoard.tiles[data.tiles[i, 0], data.tiles[i, 1]];
        }

        transform.SetParent(parentBoard.transform);
        gameObject.SetActive(false);
    }

    public virtual int[] GetMetadata()
    {
        return new int[3];
    }

    public virtual void OnTurnStart()
    {

    }

    public virtual void OnTurnEnd()
    {

    }

    public virtual void Destroy()
    {
        parentBoard.intactShipCount--;
    }

    public virtual void Damage(int[] hitTilesIndexes)
    {
        health -= hitTilesIndexes.Length;
        if (health <= 0)
        {
            Destroy();
        }
    }

    //PLACEMENT FUNCTIONS
    public struct PlacementInfo
    {
        public Vector3 localDrawerPosition;
        public Quaternion localDrawerRotation;
        public List<Vector3> waypoints;
        public Vector3 animationVelocity;
        public Tile[] lastLocation;
    }

    public PlacementInfo placementInfo;

    public virtual void Place(Tile[] location)
    {
        if (location != null)
        {
            for (int i = 0; i < tiles.Length; i++)
            {
                location[i].containedShip = this;
            }


            FleetPlacementUI.notplacedShips.Remove(this);
            FleetPlacementUI.placedShips.Add(this);


            FleetPlacementUI.it.ReevaluateTiles();
        }
        else
        {

        }

        tiles = location;

        FleetPlacementUI.selectedShip = null;
    }

    public virtual void Pickup()
    {
        placementInfo.lastLocation = tiles;

        if (tiles != null)
        {
            for (int i = 0; i < tiles.Length; i++)
            {
                tiles[i].containedShip = null;
            }
        }

        tiles = null;

        FleetPlacementUI.it.ReevaluateTiles();

        if (FleetPlacementUI.placedShips.Contains(this))
        {
            FleetPlacementUI.placedShips.Remove(this);
            FleetPlacementUI.notplacedShips.Add(this);
        }

        FleetPlacementUI.selectedShip = this;
    }
}
