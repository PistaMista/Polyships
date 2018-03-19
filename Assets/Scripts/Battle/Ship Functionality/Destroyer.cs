using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Ships
{
    public class Destroyer : Ship
    {
        public int torpedoCount;
        public int torpedoCapacity;
        public int torpedoReloadTime;
        public int torpedoReloadBatchSize;
        int torpedoReloadTimeLeft;
        public int[] firingAreaBlockages;

        public override int[] GetMetadata()
        {
            List<int> result = new List<int>();
            result.Add(torpedoCount);
            result.Add(torpedoCapacity);
            result.Add(torpedoReloadTime);
            result.Add(torpedoReloadBatchSize);
            result.Add(torpedoReloadTimeLeft);

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
            torpedoCapacity = data.metadata[1];
            torpedoReloadTime = data.metadata[2];
            torpedoReloadBatchSize = data.metadata[3];
            torpedoReloadTimeLeft = data.metadata[4];

            List<int> blockages = new List<int>();
            for (int i = 5; i < data.metadata.Length; i++)
            {
                blockages.Add(data.metadata[i]);
            }

            firingAreaBlockages = blockages.ToArray();
        }

        public override void OnTurnEnd()
        {
            base.OnTurnEnd();
            if (torpedoCount < torpedoCapacity && health > 0)
            {
                torpedoReloadTimeLeft--;
                if (torpedoReloadTimeLeft <= 0)
                {
                    torpedoReloadTimeLeft = torpedoReloadTime;
                    torpedoCount = Mathf.Clamp(torpedoCount + torpedoReloadBatchSize, 0, torpedoCapacity);
                }
            }
        }

        public override void Place(Tile[] location)
        {
            base.Place(location);
            torpedoReloadTimeLeft = torpedoReloadTime;
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
}
