using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destroyer : Ship
{
    public int torpedoCount;
    public int[] firingAreaBlockages;

    public override int[] GetMetadata()
    {
        return new int[] { torpedoCount };
    }

    public override void Initialize(ShipData data)
    {
        base.Initialize(data);
        torpedoCount = data.metadata[0];
    }

    public override void Place(Tile[] location)
    {
        base.Place(location);
        CalculateFiringArea();
    }

    public override void OnOtherShipPlacementOntoBoard(Ship placedShip, Tile[] location)
    {
        base.OnOtherShipPlacementOntoBoard(placedShip, location);
        CalculateFiringArea();
    }

    public override void OnOtherShipPickupFromBoard(Ship pickedShip, Tile[] location)
    {
        base.OnOtherShipPickupFromBoard(pickedShip, location);
        CalculateFiringArea();
    }

    void CalculateFiringArea()
    {
        Tile centerTile = tiles[1];
        firingAreaBlockages = new int[parentBoard.tiles.GetLength(0)];
        for (int i = 0; i < firingAreaBlockages.Length; i++)
        {
            firingAreaBlockages[i] = -1;
        }

        foreach (Tile tile in parentBoard.placementInfo.occupiedTiles)
        {
            if (tile.coordinates.y >= centerTile.coordinates.y)
            {
                firingAreaBlockages[tile.coordinates.x] = tile.coordinates.y;
            }
        }
    }
}
