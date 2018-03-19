using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    }
}
