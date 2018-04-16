using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Gameplay.Effects;

namespace Gameplay.Ships
{
    public class Battleship : Ship
    {
        public int artilleryBonus;
        public override int[] GetMetadata()
        {
            return new int[] { artilleryBonus };
        }

        public override void Initialize(ShipData data)
        {
            base.Initialize(data);
            artilleryBonus = data.metadata[0];
        }

        public override void Place(Tile[] location)
        {
            base.Place(location);
            parentBoard.owner.arsenal.guns += artilleryBonus;
        }

        public override void Pickup()
        {
            if (tiles != null && tiles.Length > 0) parentBoard.owner.arsenal.guns -= artilleryBonus;
            base.Pickup();
        }

        public override void Destroy()
        {
            base.Destroy();
            parentBoard.owner.arsenal.guns -= artilleryBonus;
        }
    }
}
