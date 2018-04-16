using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Ships
{
    public class Carrier : Ship
    {
        public int aircraftBonus;
        public override void Place(Tile[] location)
        {
            base.Place(location);
            parentBoard.owner.arsenal.aircraft += aircraftBonus;
        }

        public override void Pickup()
        {
            base.Pickup();
            if (tiles != null && tiles.Length > 0) parentBoard.owner.arsenal.aircraft -= aircraftBonus;
        }

        public override void Destroy()
        {
            base.Destroy();
            parentBoard.owner.arsenal.aircraft -= aircraftBonus;
        }
    }
}
