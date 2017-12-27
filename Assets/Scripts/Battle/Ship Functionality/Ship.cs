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
        public int concealedBy;
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
            result.concealedBy = ship.concealedBy ? ship.concealedBy.index : -1;
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
        //concealedBy - REF
        type = data.type;
    }

    public virtual void AssignReferences(ShipData data)
    {
        parentBoard = (data.ownedByAttacker ? Battle.main.attacker : Battle.main.defender).board;
        if (data.tiles != null)
        {
            if (data.tiles.Length > 0)
            {
                tiles = new Tile[data.tiles.GetLength(0)];
                for (int i = 0; i < data.tiles.GetLength(0); i++)
                {
                    tiles[i] = parentBoard.tiles[data.tiles[i, 0], data.tiles[i, 1]];
                }
            }
        }

        Vector3 directional = tiles[0].transform.position - tiles[tiles.Length - 1].transform.position;
        placementInfo.boardPosition = (tiles[0].transform.position + tiles[tiles.Length - 1].transform.position) / 2.0f;
        placementInfo.boardRotation = Quaternion.Euler(0, directional.z != 0 ? 0 : 90, 0);

        if (data.concealedBy >= 0)
        {
            concealedBy = (Cruiser)parentBoard.ships[data.concealedBy];
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
        public Vector3 boardPosition;
        public Quaternion boardRotation;

        public Vector3 localDrawerPosition;
        public Quaternion localDrawerRotation;
        public List<Vector3> waypoints;
        public Vector3 animationVelocity;
        public Tile[] lastLocation;
    }

    public PlacementInfo placementInfo;
    public virtual void Place(Tile[] location)
    {
        location = location != null ? (location.Length == 0 ? null : location) : null;
        if (location != null)
        {
            for (int i = 0; i < location.Length; i++)
            {
                location[i].containedShip = this;
            }

            foreach (Ship ship in parentBoard.placementInfo.placedShips)
            {
                if (ship.type == ShipType.CRUISER)
                {
                    Cruiser cruiser = (Cruiser)ship;
                    bool containsAll = true;
                    for (int i = 0; i < location.Length; i++)
                    {
                        if (!cruiser.concealmentArea.Contains(location[i]))
                        {
                            containsAll = false;
                            break;
                        }
                    }

                    if (containsAll)
                    {
                        concealedBy = cruiser;
                        cruiser.concealing = this;
                    }
                }
            }

            Vector3 directional = location[0].transform.position - location[location.Length - 1].transform.position;
            placementInfo.boardPosition = (location[0].transform.position + location[location.Length - 1].transform.position) / 2.0f;
            placementInfo.boardRotation = Quaternion.Euler(0, directional.z != 0 ? 0 : 90, 0);


            parentBoard.placementInfo.notplacedShips.Remove(this);
            parentBoard.placementInfo.placedShips.Add(this);
            transform.SetParent(parentBoard.transform);
        }
        else
        {
            // transform.SetParent(FleetPlacementUI.it.shipDrawer.transform);
        }

        tiles = location;

        placementInfo.waypoints = new List<Vector3>();

        Vector3 targetPosition = location != null ? placementInfo.boardPosition + Vector3.up * MiscellaneousVariables.it.boardUIRenderHeight : FleetPlacementUI.it.shipDrawer.transform.TransformPoint(placementInfo.localDrawerPosition);
        placementInfo.waypoints.Add(new Vector3(targetPosition.x, MiscellaneousVariables.it.boardUIRenderHeight + FleetPlacementUI.it.shipAnimationElevation, targetPosition.z));
        placementInfo.waypoints.Add(targetPosition);


        parentBoard.placementInfo.selectedShip = null;
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

        if (concealedBy)
        {
            concealedBy.concealing = null;
            concealedBy = null;
        }

        if (parentBoard.placementInfo.placedShips.Contains(this))
        {
            parentBoard.placementInfo.placedShips.Remove(this);
            parentBoard.placementInfo.notplacedShips.Add(this);
        }

        parentBoard.placementInfo.selectedShip = this;

        parentBoard.ReevaluateTiles();

        placementInfo.waypoints = new List<Vector3>();
        placementInfo.waypoints.Add(new Vector3(transform.position.x, MiscellaneousVariables.it.boardUIRenderHeight + FleetPlacementUI.it.shipAnimationElevation, transform.position.z));
    }
}
