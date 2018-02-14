using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cruiser : Ship
{
    public Ship concealing;
    public List<Tile> concealmentArea;

    public override void Destroy()
    {
        base.Destroy();
        if (concealing)
        {
            concealing.concealedBy = null;
            concealing = null;
        }
    }

    public override void OnOtherShipPlacementOntoBoard(Ship placedShip, Tile[] location)
    {
        base.OnOtherShipPlacementOntoBoard(placedShip, location);
        TryConcealingShip(placedShip);
    }

    public void ConcealAlreadyPlacedShipsInConcealmentArea()
    {
        List<Ship> foundShips = new List<Ship>();
        foreach (Tile tile in concealmentArea)
        {
            if (tile.containedShip)
            {
                if (!foundShips.Contains(tile.containedShip))
                {
                    foundShips.Add(tile.containedShip);
                }
            }
        }

        foreach (Ship ship in foundShips)
        {
            if (TryConcealingShip(ship))
            {
                break;
            }
        }
    }

    bool TryConcealingShip(Ship ship)
    {
        if (concealing == null)
        {
            if (ship.concealedBy != null || concealedBy == ship)
            {
                return false;
            }

            bool containsAll = true;
            for (int i = 0; i < ship.tiles.Length; i++)
            {
                if (!concealmentArea.Contains(ship.tiles[i]))
                {
                    containsAll = false;
                    break;
                }
            }

            if (containsAll)
            {
                ship.concealedBy = this;
                concealing = ship;
                return true;
            }
        }

        return false;
    }

    public override void OnOtherShipPickupFromBoard(Ship pickedShip, Tile[] location)
    {
        if (pickedShip == concealing)
        {
            pickedShip.concealedBy = null;
            concealing = null;
        }
    }

    public override void Place(Tile[] location)
    {
        base.Place(location);
        if (location != null)
        {
            concealmentArea = new List<Tile>();
            foreach (Tile shipTile in location)
            {
                for (int x = -2; x <= 2; x++)
                {
                    for (int y = -2; y <= 2; y++)
                    {
                        Vector2Int p = shipTile.coordinates + new Vector2Int(x, y);
                        if (p.x >= 0 && p.x < parentBoard.tiles.GetLength(0) && p.y >= 0 && p.y < parentBoard.tiles.GetLength(1))
                        {
                            Tile candidateTile = parentBoard.tiles[p.x, p.y];
                            if (!concealmentArea.Contains(candidateTile))
                            {
                                concealmentArea.Add(candidateTile);
                            }
                        }
                    }
                }
            }

            foreach (Tile shipTile in location)
            {
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        Vector2Int p = shipTile.coordinates + new Vector2Int(x, y);
                        if (p.x >= 0 && p.x < parentBoard.tiles.GetLength(0) && p.y >= 0 && p.y < parentBoard.tiles.GetLength(1))
                        {
                            Tile candidateTile = parentBoard.tiles[p.x, p.y];
                            if (concealmentArea.Contains(candidateTile))
                            {
                                concealmentArea.Remove(candidateTile);
                            }
                        }
                    }
                }
            }
        }
    }

    public override void Pickup()
    {
        base.Pickup();
        if (concealing)
        {
            concealing.concealedBy = null;
            concealing = null;
        }
    }

    public override int[] GetMetadata()
    {
        return new int[] { concealing ? concealing.index : -1 };
    }

    public override void AssignReferences(ShipData data)
    {
        base.AssignReferences(data);
        if (data.metadata[0] >= 0)
        {
            concealing = parentBoard.ships[data.metadata[0]];
        }
    }
}
