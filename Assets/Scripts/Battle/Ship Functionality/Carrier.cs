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
            if (location != null && location.Length > 0) parentBoard.owner.arsenal.aircraft += aircraftBonus;
        }

        public override void Pickup()
        {
            if (tiles != null && tiles.Length > 0) parentBoard.owner.arsenal.aircraft -= aircraftBonus;
            base.Pickup();
        }

        public override void Destroy()
        {
            base.Destroy();
            parentBoard.owner.arsenal.aircraft -= aircraftBonus;
        }
    }
}
