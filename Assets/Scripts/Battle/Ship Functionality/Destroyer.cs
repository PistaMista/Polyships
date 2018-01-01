using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destroyer : Ship
{
    public int torpedoCount;
    public int[] firingAreaBlockages;

    public override int[] GetMetadata()
    {
        List<int> result = new List<int>();
        result.Add(torpedoCount);

        for (int i = 0; i < firingAreaBlockages.Length; i++)
        {
            result.Add(firingAreaBlockages[i]);
        }

        return result.ToArray();
    }

    public override void Initialize(ShipData data)
    {
        base.Initialize(data);
        torpedoCount = data.metadata[0];

        List<int> blockages = new List<int>();
        for (int i = 1; i < data.metadata.Length; i++)
        {
            blockages.Add(data.metadata[i]);
        }

        firingAreaBlockages = blockages.ToArray();
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
        firingAreaBlockages = new int[parentBoard.tiles.GetLength(0)];
        for (int i = 0; i < firingAreaBlockages.Length; i++)
        {
            firingAreaBlockages[i] = -1;
        }

        if (tiles != null)
        {
            Tile centerTile = tiles[1];

            foreach (Tile tile in parentBoard.placementInfo.occupiedTiles)
            {
                if (tile.coordinates.y >= centerTile.coordinates.y && tile.containedShip != this)
                {
                    if (firingAreaBlockages[tile.coordinates.x] < 0 || tile.coordinates.y < firingAreaBlockages[tile.coordinates.x])
                    {
                        firingAreaBlockages[tile.coordinates.x] = tile.coordinates.y;
                    }
                }
            }
        }
    }
}
